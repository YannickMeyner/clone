using Tetrispp.Models;

namespace Tetrispp.Services;

public class TetrisGameService
{
    /// <summary>
    /// Erstellt einen neuen Block für den Spieler
    /// </summary>
    public Tetromino CreateNewBlock()
    {
        TetrominoType blockType = TetrominoShapes.GetRandomType();
        return new Tetromino
        {
            Type = blockType,
            Rotation = 0,
            Position = new Position(3, 0) // Startposition in der Mitte oben
        };
    }

    /// <summary>
    /// Initialisiert den Spielstatus für einen Spieler
    /// CurrentBlock = repräsentiert den aktuell fallenden Block, der "aktive" Block
    /// NextBlock = nächste Tetromino, der ins Spiel kommen wird. Kann z.B. als Vorschau angezeigt werden
    /// </summary>
    public PlayerState InitializePlayerState(string playerId)
    {
        var playerState = new PlayerState
        {
            PlayerId = playerId,
            CurrentBlock = CreateNewBlock(),
            NextBlock = CreateNewBlock()
        };

        return playerState;
    }

    /// <summary>
    /// Bewegt den Block nach links, rechts oder unten
    /// </summary>
    public bool MoveBlock(PlayerState state, string direction)
    {
        if (state.CurrentBlock == null || state.IsGameOver)
            return false;

        var newPosition = new Position(
            state.CurrentBlock.Position.X + (direction == "LEFT" ? -1 : direction == "RIGHT" ? 1 : 0),
            state.CurrentBlock.Position.Y + (direction == "DOWN" ? 1 : 0)
        );
        Console.WriteLine($"Bewege Block {state.CurrentBlock.Type} von ({state.CurrentBlock.Position.X}, {state.CurrentBlock.Position.Y}) nach ({newPosition.X}, {newPosition.Y})");

        // Position überprüfen
        if (IsValidPosition(state, newPosition, state.CurrentBlock.Type, state.CurrentBlock.Rotation))
        {
            state.CurrentBlock.Position = newPosition;

            // Wenn nach unten bewegt wurde, überprüfen, ob der Block auf etwas stösst
            if (direction == "DOWN" && !CanMoveDown(state))
            {
                PlaceBlock(state);
                return true; // Block wurde platziert
            }

            return true; // Bewegung erfolgreich
        }

        // Wenn nach unten bewegt wurde und die Position ungültig ist, wird der Block platziert
        if (direction == "DOWN")
        {
            PlaceBlock(state);
            return true; // Block wurde platziert
        }

        return false; // Bewegung fehlgeschlagen
    }

    /// <summary>
    /// Dreht den aktuellen Block
    /// </summary>
    public bool RotateBlock(PlayerState state)
    {
        if (state.CurrentBlock == null || state.IsGameOver)
            return false;

        // jeder Block hat 4 mögliche Rotationszustände
        // wenn aktuelle Rotation 0 ist dann: (0 + 1) % 4 = 1 -> nächste Rotation ist 1
        // wenn aktuelle Rotation 3 ist dann; (3 + 1) % 4 = 0 -> zurück zur Rotation 0
        int newRotation = (state.CurrentBlock.Rotation + 1) % 4;

        // Rotation überprüfen
        if (IsValidPosition(state, state.CurrentBlock.Position, state.CurrentBlock.Type, newRotation))
        {
            state.CurrentBlock.Rotation = newRotation;
            return true; // Rotation erfolgreich
        }

        return false; // Rotation fehlgeschlagen
    }

    /// <summary>
    /// Lässt den Block sofort fallen (Harddrop)
    /// </summary>
    public void DropBlock(PlayerState state)
    {
        if (state.CurrentBlock == null || state.IsGameOver)
            return;

        // Block so weit wie möglich nach unten bewegen
        while (CanMoveDown(state))
        {
            state.CurrentBlock.Position = new Position(
                state.CurrentBlock.Position.X,
                state.CurrentBlock.Position.Y + 1
            );
        }

        // Block im Grid platzieren
        PlaceBlock(state);
    }

    /// <summary>
    /// Überprüft, ob der Block nach unten bewegt werden kann
    /// </summary>
    private bool CanMoveDown(PlayerState state)
    {
        if (state.CurrentBlock == null)
            return false;

        var newPosition = new Position(
            state.CurrentBlock.Position.X,
            state.CurrentBlock.Position.Y + 1
        );

        return IsValidPosition(state, newPosition, state.CurrentBlock.Type, state.CurrentBlock.Rotation);
    }

    /// <summary>
    /// Überprüft, ob die angegebene Position für den Block gültig ist
    /// </summary>
    public bool IsValidPosition(PlayerState state, Position position, TetrominoType blockType, int rotation)
    {
        var shape = TetrominoShapes.Shapes[blockType][rotation];
        int shapeSize = TetrominoShapes.ShapeSizes[blockType];

        for (int y = 0; y < shapeSize; y++)
        {
            for (int x = 0; x < shapeSize; x++)
            {
                // Nur gefüllte Zellen prüfen
                if (shape[y][x] == 0)
                    continue;

                int gridX = position.X + x;
                int gridY = position.Y + y;

                // Grenzen prüfen
                if (gridX < 0 || gridX >= 10 || gridY < 0 || gridY >= 20)
                    return false;

                // Kollision mit anderen Blöcken prüfen
                if (state.Grid[gridX, gridY] != 0)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Platziert den aktuellen Block im Grid und generiert einen neuen Block
    /// </summary>
    private void PlaceBlock(PlayerState state)
    {
        if (state.CurrentBlock == null)
            return;

        var shape = TetrominoShapes.Shapes[state.CurrentBlock.Type][state.CurrentBlock.Rotation];
        int shapeSize = TetrominoShapes.ShapeSizes[state.CurrentBlock.Type];

        // Block im Grid platzieren
        for (int y = 0; y < shapeSize; y++)
        {
            for (int x = 0; x < shapeSize; x++)
            {
                if (shape[y][x] != 0)
                {
                    int gridX = state.CurrentBlock.Position.X + x;
                    int gridY = state.CurrentBlock.Position.Y + y;

                    if (gridX >= 0 && gridX < 10 && gridY >= 0 && gridY < 20)
                    {
                        state.Grid[gridX, gridY] = shape[y][x]; // Nutzt direkt den Wert aus dem Shape
                    }
                }
            }
        }

        // Rest der Methode bleibt gleich
        int linesCleared = ClearLines(state);
        state.LinesCleared += linesCleared;
        state.Score += CalculateScore(linesCleared);

        state.CurrentBlock = state.NextBlock;
        state.NextBlock = CreateNewBlock();

        if (!IsValidPosition(state, state.CurrentBlock.Position, state.CurrentBlock.Type, state.CurrentBlock.Rotation))
        {
            state.IsGameOver = true;
        }
    }

    /// <summary>
    /// Entfernt gefüllte Reihen und speichert sie für den Transfer zum Gegner
    /// </summary>
    private int ClearLines(PlayerState state)
    {
        int linesCleared = 0;
        List<int[]> completedLines = new List<int[]>();

        for (int y = 0; y < 20; y++)
        {
            bool isLineFull = true;

            // Prüfen, ob die Reihe voll ist
            for (int x = 0; x < 10; x++)
            {
                if (state.Grid[x, y] == 0)
                {
                    isLineFull = false;
                    break;
                }
            }

            if (isLineFull)
            {
                // Speichere die vollständige Zeile für den Gegner
                int[] completedLine = new int[10];
                for (int x = 0; x < 10; x++)
                {
                    completedLine[x] = state.Grid[x, y];
                }
                completedLines.Add(completedLine);

                // Reihe löschen und alles darüber nach unten verschieben
                for (int moveY = y; moveY > 0; moveY--)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        state.Grid[x, moveY] = state.Grid[x, moveY - 1];
                    }
                }

                // Oberste Reihe leeren
                for (int x = 0; x < 10; x++)
                {
                    state.Grid[x, 0] = 0;
                }

                linesCleared++;
            }
        }

        // Speichere die vollständigen Zeilen im Spielerstatus zum späteren Transfer
        if (completedLines.Count > 0)
        {
            state.CompletedLines = completedLines;
        }

        return linesCleared;
    }

    /// <summary>
    /// Fügt vervollständige Zeilen vom Gegner in das Spielfeld ein
    /// </summary>
    public void AddCompletedLinesToPlayer(PlayerState state, List<int[]> completedLines)
    {
        if (completedLines == null || completedLines.Count == 0 || state.IsGameOver)
            return;

        foreach (var line in completedLines)
        {
            // Verschiebe alle Zeilen um 1 nach oben
            for (int y = 0; y < 19; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    state.Grid[x, y] = state.Grid[x, y + 1];
                }
            }

            // Füge die vollständige Zeile ganz unten ein (Index 19)
            for (int x = 0; x < 10; x++)
            {
                state.Grid[x, 19] = line[x];
            }

            // Wenn der aktuelle Block mit der neuen Zeile kollidiert, bewege ihn nach oben
            if (state.CurrentBlock != null)
            {
                if (!IsValidPosition(state, state.CurrentBlock.Position, state.CurrentBlock.Type, state.CurrentBlock.Rotation))
                {
                    // Versuche, den Block nach oben zu verschieben
                    Position newPosition = new Position(
                        state.CurrentBlock.Position.X,
                        state.CurrentBlock.Position.Y - 1
                    );

                    if (IsValidPosition(state, newPosition, state.CurrentBlock.Type, state.CurrentBlock.Rotation))
                    {
                        state.CurrentBlock.Position = newPosition;
                    } else
                    {
                        // Wenn der Block nicht mehr verschoben werden kann, Game Over
                        state.IsGameOver = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Berechnet die Punktzahl basierend auf der Anzahl der gelöschten Reihen
    /// </summary>
    private int CalculateScore(int linesCleared)
    {
        return linesCleared switch
        {
            1 => 100,
            2 => 300, // 2 Reihen gleichzeitig
            3 => 500, // 3 Reihen gleichzeitig
            4 => 800, // 4 Reihen gleichzeitig
            _ => 0
        };
    }
}