using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tetrispp.Services;

namespace Tetrispp.Models;

public class GameRoom
{
    private readonly TetrisGameService _gameService = new();
    private Timer? _gameLoop;
    private readonly object _lockObject = new();
    private bool _gameStarted = false;

    public string RoomId { get; } = Guid.NewGuid().ToString();
    public List<Player> Players { get; } = new();
    public GameState GameState { get; } = new();
    public bool IsFull => Players.Count >= 2;
    public bool IsEmpty => Players.Count == 0;

    // 1 Sekunde zwischen den Spielupdates
    private readonly int _gameTickIntervalMs = 1000;

    /// <summary>
    /// Startet das Spiel
    /// </summary>
    public void StartGame()
    {
        lock (_lockObject)
        {
            if (_gameStarted)
                return;

            _gameStarted = true;
            GameState.IsGameActive = true;

            // Spieler benachrichtigen
            _ = BroadcastMessage(new
            {
                action = "GAME_START"
            });

            // Spielstatus an Spieler senden
            _ = BroadcastGameState();

            // Game Loop starten (Timer für automatisches Fallen der Blöcke)
            _gameLoop = new Timer(GameTick, null, 0, _gameTickIntervalMs);
        }
    }

    /// <summary>
    /// Beendet das Spiel und sendet eine Game-Over-Nachricht
    /// </summary>
    private async Task EndGame(string winnerId)
    {
        StopGame();

        await BroadcastMessage(new
        {
            action = "GAME_OVER",
            winnerId = winnerId
        });
    }

    /// <summary>
    /// Stoppt das Spiel
    /// </summary>
    public void StopGame()
    {
        lock (_lockObject)
        {
            _gameStarted = false;
            GameState.IsGameActive = false;
            _gameLoop?.Dispose();
            _gameLoop = null;
        }
    }

    /// <summary>
    /// Fügt einen Spieler zum Raum hinzu und initialisiert seinen Spielstatus
    /// </summary>
    public void AddPlayer(Player player)
    {
        if (!IsFull)
        {
            Players.Add(player);
            var playerState = _gameService.InitializePlayerState(player.PlayerId);
            GameState.Players[player.PlayerId] = playerState;

            // Wenn zwei Spieler da sind, das Spiel starten
            if (Players.Count == 2 && !_gameStarted)
            {
                StartGame();
            }
        }
    }

    /// <summary>
    /// Entfernt einen Spieler aus dem Raum
    /// </summary>
    public async Task RemovePlayer(WebSocket socket)
    {
        var player = Players.FirstOrDefault(p => p.Socket == socket);
        if (player != null)
        {
            Players.Remove(player);
            GameState.Players.Remove(player.PlayerId);

            // Spiel stoppen, wenn ein Spieler geht
            StopGame();

            // Spieler benachrichtigen
            foreach (var remainingPlayer in Players)
            {
                await SendMessage(remainingPlayer.Socket, new
                {
                    action = "PLAYER_DISCONNECTED",
                    playerId = player.PlayerId
                });
            }
        }
    }

    /// <summary>
    /// Wird periodisch (_gameTickIntervalMs) aufgerufen, um das Spiel zu aktualisieren
    /// </summary>
    private async void GameTick(object? state)
    {
        if (!GameState.IsGameActive)
            return;

        // Prüfen, ob ein Spieler das Spiel verloren hat
        if (GameState.Players.Values.Any(p => p.IsGameOver))
        {
            // Spiel beenden und Gewinner ermitteln
            string? winnerId = GameState.Players.FirstOrDefault(p => !p.Value.IsGameOver).Key;
            await EndGame(winnerId);
            return;
        }

        bool blockPlaced = false;

        // Für jeden Spieler den Block automatisch nach unten bewegen (fallenlassen der Blöcke d.h. sozusagen die Schwerkraft)
        foreach (var playerState in GameState.Players.Values)
        {
            if (playerState.CurrentBlock != null && !playerState.IsGameOver)
            {
                if (_gameService.MoveBlock(playerState, "DOWN"))
                {
                    blockPlaced = true;
                }
            }
        }

        // Übertrage vollständige Zeilen von einem Spieler zum Gegner
        foreach (var playerEntry in GameState.Players)
        {
            var playerState = playerEntry.Value;
            if (playerState.CompletedLines != null && playerState.CompletedLines.Count > 0)
            {
                var opponentEntry = GameState.Players.FirstOrDefault(p => p.Key != playerEntry.Key);
                if (opponentEntry.Value != null && !opponentEntry.Value.IsGameOver)
                {
                    // Füge die vollständigen Zeilen dem Gegner hinzu
                    _gameService.AddCompletedLinesToPlayer(opponentEntry.Value, playerState.CompletedLines);
                    blockPlaced = true;
                }

                // Leere die CompletedLines, nachdem sie verarbeitet wurden
                playerState.CompletedLines = null;
            }
        }

        // Spielstatus an Spieler senden, wenn ein Block platziert wurde oder wenn wieder _gameTickIntervalMs vergangen sind
        // dadurch kann das Spiel auch schneller aktualisiert werden, wenn ein Block platziert wurde!
        TimeSpan timeSinceLastUpdate = DateTime.Now - GameState.LastUpdateTime;
        if (blockPlaced || timeSinceLastUpdate.TotalSeconds >= (_gameTickIntervalMs / 1000))
        {
            GameState.LastUpdateTime = DateTime.Now;
            await BroadcastGameState();
        }
    }

    /// <summary>
    /// Verarbeitet eine Spielaktion
    /// verantwortlich für die Verarbeitung der Spieleraktionen, die von den Clients gesendet werden.
    /// </summary>
    public async Task HandleGameAction(GameAction action, Player player)
    {
        if (!GameState.IsGameActive || !GameState.Players.TryGetValue(player.PlayerId, out var playerState))
            return;

        if (playerState.IsGameOver)
            return;

        bool stateChanged = false;

        switch (action.ActionType)
        {
            case ActionType.Move:
                Console.WriteLine($"Player {player.PlayerId} moved block {action.Direction}");
                if (action.Direction != null)
                {
                    stateChanged = _gameService.MoveBlock(playerState, action.Direction);
                }
                break;
            case ActionType.Rotate:
                Console.WriteLine($"Player {player.PlayerId} rotated block");
                stateChanged = _gameService.RotateBlock(playerState);
                break;
            case ActionType.Drop:
                Console.WriteLine($"Player {player.PlayerId} dropped block");
                _gameService.DropBlock(playerState);
                stateChanged = true;
                break;
            case ActionType.Start:
                if (!_gameStarted)
                {
                    StartGame();
                    stateChanged = true;
                }
                break;
            case ActionType.Stop:
                if (_gameStarted)
                {
                    StopGame();
                    stateChanged = true;
                }
                break;
            default:
                await SendMessage(player.Socket, new
                {
                    action = "ERROR",
                    message = "Invalid action type"
                });
                break;
        }

        // Wenn sich der Spielstatus geändert hat, an alle Spieler darüber informieren
        if (stateChanged)
        {
            Console.WriteLine($"Player {player.PlayerId} changed game state");
            await BroadcastGameState();
        }
    }

    /// <summary>
    /// Sendet den aktuellen Spielstatus an alle Spieler
    /// </summary>
    private async Task BroadcastGameState()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        foreach (var player in Players)
        {
            // Spielstatus für diesen Spieler zusammenstellen
            var selfState = GameState.Players[player.PlayerId];

            // Gegner finden
            var opponent = GameState.Players.FirstOrDefault(p => p.Key != player.PlayerId);
            PlayerState? opponentState = opponent.Value;

            // Zusammengestellten Spielstatus senden
            await SendMessage(player.Socket, new
            {
                action = "UPDATE",
                gameState = new
                {
                    players = new
                    {
                        self = new
                        {
                            playerId = selfState.PlayerId,
                            grid = PopulateCurrentBlockIntoGrid(SerializeGrid(selfState.Grid), selfState.CurrentBlock),
                            currentBlock = selfState.CurrentBlock,
                            nextBlock = TransformBlockIntoGrid(selfState.NextBlock),
                            score = selfState.Score,
                            linesCleared = selfState.LinesCleared,
                            isGameOver = selfState.IsGameOver
                        },
                        opponent = opponentState != null ? new
                        {
                            playerId = opponentState.PlayerId,
                            grid = PopulateCurrentBlockIntoGrid(SerializeGrid(opponentState.Grid), opponentState.CurrentBlock),
                            currentBlock = opponentState.CurrentBlock,
                            score = opponentState.Score,
                            linesCleared = opponentState.LinesCleared,
                            isGameOver = opponentState.IsGameOver
                        } : null
                    },
                    isGameActive = GameState.IsGameActive
                }
            }, jsonOptions);
        }
    }


    /// <summary>
    /// Hilfsmethode zum Einfügen des aktuellen Blocks in das Grid
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="currentBlock"></param>
    /// <returns></returns>
    private List<List<int>> PopulateCurrentBlockIntoGrid(List<List<int>> grid, Tetromino currentBlock)
    {
        if (currentBlock == null)
            return grid;

        var shape = TetrominoShapes.Shapes[currentBlock.Type][currentBlock.Rotation];
        var size = TetrominoShapes.ShapeSizes[currentBlock.Type];
        var originX = currentBlock.Position.X;   
        var originY = currentBlock.Position.Y;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Check if the shape cell is filled (non-zero)
                if (shape[y][x] != 0)
                {
                    int gridX = originX + x;
                    int gridY = originY + y;

                    // Ensure we are within grid bounds
                    if (gridX >= 0 && gridX < grid[0].Count && gridY >= 0 && gridY < grid.Count)
                    {
                        grid[gridY][gridX] = shape[y][x];
                    }
                }
            }
        }

        return grid;
    }

    /// <summary>
    /// Hilfsmethode zum Transformieren des aktuellen Blocks in ein 2D-Array (Grid)
    /// </summary>
    /// <param name="currentBlock"></param>
    /// <returns></returns>
    private List<List<int>> TransformBlockIntoGrid(Tetromino currentBlock) {
        if (currentBlock == null)
            return new List<List<int>>();

        var shape = TetrominoShapes.Shapes[currentBlock.Type][currentBlock.Rotation];
        var size = shape.Length;
        var result = new List<List<int>>();

        for (int y = 0; y < size; y++) {
            var row = new List<int>();
            for (int x = 0; x < size; x++) {
                row.Add(shape[y][x]);
            }
            result.Add(row);
        }

        return result;
    }

    /// <summary>
    /// Hilfsmethode zum Serialisieren des 2D-Arrays (Grid) als Liste von Listen
    /// - wandelt das 2D-Array in eine Liste von Listen um
    /// - dieses Format kann problemlos zu JSON serialisiert werden -> kann von JavaScript-Clients einfach verarbeitet werden
    /// </summary>
    private List<List<int>> SerializeGrid(int[,] grid)
    {
        var result = new List<List<int>>();
        for (int y = 0; y < 20; y++)
        {
            var row = new List<int>();
            for (int x = 0; x < 10; x++)
            {
                row.Add(grid[x, y]);
            }
            result.Add(row);
        }
        return result;
    }

    /// <summary>
    /// Sendet eine Nachricht an einen bestimmten Spieler
    /// </summary>
    private async Task SendMessage(WebSocket socket, object message, JsonSerializerOptions? options = null)
    {
        if (socket.State != WebSocketState.Open)
            return;

        var json = JsonSerializer.Serialize(message, options ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var buffer = Encoding.UTF8.GetBytes(json);
        await socket.SendAsync(
            buffer,
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    /// <summary>
    /// Sendet eine Nachricht an alle Spieler im Raum
    /// </summary>
    private async Task BroadcastMessage(object message)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        foreach (var player in Players)
        {
            await SendMessage(player.Socket, message, jsonOptions);
        }
    }
}