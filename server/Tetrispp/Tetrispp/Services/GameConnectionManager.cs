using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Tetrispp.Data;
using Tetrispp.Models;

namespace Tetrispp.Services;

public class GameConnectionManager
{
    // Thread-safe
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly ConcurrentDictionary<WebSocket, string> _connections = new();
    private readonly IServiceProvider _serviceProvider;

    public GameConnectionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Bei einer neuen Websocket-Connection wird ein Player initialisiert und dieser einem passenden Raum zugewiesen (sobald die Join-Nachricht vom Client kommt)
    /// </summary>
    public async Task HandlePlayer(WebSocket socket, string token)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            var userClaims = authService.ValidateToken(token);
            if (userClaims == null)
            {
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid token", CancellationToken.None);
                return;
            }

            // extract user information von den claims
            int userId = int.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Auf JOIN-Nachricht warten
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var action = JsonSerializer.Deserialize<GameAction>(message);

                if (action?.ActionType == ActionType.Join)
                {
                    var room = FindAvailableRoom(userId);
                    var player = new Player(socket, userId);
                    room.AddPlayer(player);
                    _connections.TryAdd(socket, room.RoomId);

                    // Initialisierungsnachricht senden
                    var initMessage = JsonSerializer.Serialize(new
                    {
                        action = ActionType.Init,
                        roomId = room.RoomId,
                        userId = player.UserId
                    }, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    await socket.SendAsync(
                        Encoding.UTF8.GetBytes(initMessage),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);

                    // Nachrichten-Loop für diesen Spieler
                    await HandleMessages(socket, room, player, true);
                }
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error handling player: {ex.Message}");
        } finally
        {
            await CleanUpConnection(socket, true);
        }
    }

    public async Task HandleSpectator(WebSocket socket, string token, string roomId)
    {
        try
        {
            // Token validieren wie bei normalen Spielern
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            var userClaims = authService.ValidateToken(token);
            if (userClaims == null)
            {
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid token", CancellationToken.None);
                return;
            }

            int userId = int.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // zuerst muss der entsprechende Raum gefunden werden
            if (_rooms.TryGetValue(roomId, out GameRoom? room))
            {
                var spectator = new Player(socket, userId);
                room.AddSpectator(spectator);
                _connections.TryAdd(socket, roomId);

                // Nachrichten-Loop für diesen Zuschauer
                await HandleMessages(socket, room, spectator, false);
            } else
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Room not found", CancellationToken.None);
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error handling spectator: {ex.Message}");
        } finally
        {
            await CleanUpConnection(socket, false);
        }
    }

    /// <summary>
    /// Verarbeitet eingehende Nachrichten von einem Spieler und Spectator
    /// </summary>
    private async Task HandleMessages(WebSocket socket, GameRoom room, Player user, bool isPlayer)
    {
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open)
        {
            try
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Nur Spieler verarbeiten Text-Nachrichten
                    if (isPlayer)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await HandleGameMessage(message, room, user);
                    }
                } else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(
                        result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                        result.CloseStatusDescription,
                        CancellationToken.None);
                    break;
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"Error in message handling: {ex.Message}");
                break;
            }
        }
    }

    /// <summary>
    /// Verarbeitet eine eingehende Nachricht
    /// </summary>
    private async Task HandleGameMessage(string message, GameRoom room, Player sender)
    {
        try
        {
            var action = JsonSerializer.Deserialize<GameAction>(message);
            if (action != null)
            {
                await room.HandleGameAction(action, sender);
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error parsing message: {ex.Message}");
        }
    }

    /// <summary>
    /// Findet einen verfügbaren Raum oder erstellt einen neuen
    /// </summary>
    private GameRoom FindAvailableRoom(int userId)
    {
        // Suche nach Raum mit freiem Platz (max 2 Spieler) ohne den aktuellen Spieler
        var availableRoom = _rooms.Values.FirstOrDefault(room => !room.IsFull && !room.Players.Any(p => p.UserId == userId));
        if (availableRoom == null)
        {
            var newRoom = new GameRoom(_serviceProvider);
            _rooms.TryAdd(newRoom.RoomId, newRoom);
            return newRoom;
        }
        return availableRoom;
    }

    /// <summary>
    /// Bereinigt die Verbindung eines Spielers
    /// </summary>
    private async Task CleanUpConnection(WebSocket socket, bool isPlayer)
    {
        if (_connections.TryRemove(socket, out string? roomId) && roomId != null)
        {
            if (_rooms.TryGetValue(roomId, out GameRoom? room))
            {
                if (isPlayer)
                {
                    await room.RemovePlayer(socket);
                    if (room.IsEmpty)
                    {
                        _rooms.TryRemove(roomId, out _);
                    }
                } else
                {
                    await room.RemoveSpectator(socket);
                }
            }
        }
    }

    public List<ActiveGameInfo> GetActiveGames()
    {
        return _rooms.Values
            .Where(room => room.GameState.IsGameActive)
            .Select(room => new ActiveGameInfo
            {
                RoomId = room.RoomId,
                Players = room.GameState.Players.Select(p => new GamePlayerInfo
                {
                    UserId = p.Key,
                    Username = GetUsername(p.Key),
                    LinesCleared = p.Value.LinesCleared
                }).ToList()
            })
            .ToList();
    }

    private string GetUsername(int userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SqlContext>();
        var user = context.Users.FirstOrDefault(u => u.Id == userId);
        return user?.Username ?? $"unknown user";
    }
}