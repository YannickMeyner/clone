using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Tetrispp.Models;

namespace Tetrispp.Services;

public class GameConnectionManager
{
    // Thread-safe
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private readonly ConcurrentDictionary<WebSocket, string> _connections = new();

    /// <summary>
    /// Bei einer neuen Websocket-Connection wird ein Player initialisiert und dieser einem passenden Raum zugewiesen
    /// </summary>
    public async Task HandlePlayer(WebSocket socket)
    {
        try
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Auf JOIN-Nachricht warten
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var action = JsonSerializer.Deserialize<GameAction>(message);

                if (action?.ActionType == ActionType.Join)
                {
                    var room = FindAvailableRoom();
                    var player = new Player(socket);
                    room.AddPlayer(player);
                    _connections.TryAdd(socket, room.RoomId);

                    // Initialisierungsnachricht senden
                    var initMessage = JsonSerializer.Serialize(new
                    {
                        action = ActionType.Init,
                        roomId = room.RoomId,
                        playerId = player.PlayerId
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
                    await HandlePlayerMessages(socket, room, player);
                }
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error handling player: {ex.Message}");
        } finally
        {
            await CleanUpConnection(socket);
        }
    }

    /// <summary>
    /// Verarbeitet eingehende Nachrichten von einem Spieler
    /// </summary>
    private async Task HandlePlayerMessages(WebSocket socket, GameRoom room, Player player)
    {
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open)
        {
            try
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await HandleMessage(message, room, player);
                } else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
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
    private async Task HandleMessage(string message, GameRoom room, Player sender)
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
    private GameRoom FindAvailableRoom()
    {
        // Suche nach Raum mit freiem Platz (max 2 Spieler)
        var availableRoom = _rooms.Values.FirstOrDefault(room => !room.IsFull);
        if (availableRoom == null)
        {
            var newRoom = new GameRoom();
            _rooms.TryAdd(newRoom.RoomId, newRoom);
            return newRoom;
        }
        return availableRoom;
    }

    /// <summary>
    /// Bereinigt die Verbindung eines Spielers
    /// </summary>
    private async Task CleanUpConnection(WebSocket socket)
    {
        if (_connections.TryRemove(socket, out string? roomId) && roomId != null)
        {
            if (_rooms.TryGetValue(roomId, out GameRoom? room))
            {
                await room.RemovePlayer(socket);
                if (room.IsEmpty)
                {
                    _rooms.TryRemove(roomId, out _);
                }
            }
        }
    }
}