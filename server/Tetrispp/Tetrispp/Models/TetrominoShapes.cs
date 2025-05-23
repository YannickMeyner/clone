namespace Tetrispp.Models;

public class TetrominoShapes
{
    // Nur die Grundformen (0° Rotation) speichern
    private static readonly Dictionary<TetrominoType, int[][]> BaseShapes = new()
    {
        // 1en auf zweiter Zeile wegen Rotationszentrums (SRS) -> dadurch dreht es sich gleichmässiger
        [TetrominoType.I] = new[]
        {
            new[] {0,0,0,0},
            new[] {1,1,1,1},
            new[] {0,0,0,0},
            new[] {0,0,0,0}
        },
        [TetrominoType.O] = new[]
        {
            new[] {2,2},
            new[] {2,2}
        },
        [TetrominoType.T] = new[]
        {
            new[] {0,3,0},
            new[] {3,3,3},
            new[] {0,0,0}
        },
        [TetrominoType.S] = new[]
        {
            new[] {0,4,4},
            new[] {4,4,0},
            new[] {0,0,0}
        },
        [TetrominoType.Z] = new[]
        {
            new[] {5,5,0},
            new[] {0,5,5},
            new[] {0,0,0}
        },
        [TetrominoType.J] = new[]
        {
            new[] {6,0,0},
            new[] {6,6,6},
            new[] {0,0,0}
        },
        [TetrominoType.L] = new[]
        {
            new[] {0,0,7},
            new[] {7,7,7},
            new[] {0,0,0}
        }
    };

    // Tetromino-Grössen (werden für Rotationsberechnungen und platzieren des CurrentBlocks im Grid benötigt)
    public static readonly Dictionary<TetrominoType, int> ShapeSizes = new()
    {
        [TetrominoType.I] = 4, // I ist 4x4
        [TetrominoType.O] = 2, // O ist 2x2
        [TetrominoType.T] = 3, // T ist 3x3
        [TetrominoType.S] = 3, // S ist 3x3
        [TetrominoType.Z] = 3, // Z ist 3x3
        [TetrominoType.J] = 3, // J ist 3x3
        [TetrominoType.L] = 3  // L ist 3x3
    };

    // Gibt die berechnete Matrix/Form je nach Rotation zurück
    public static int[][] GetShape(TetrominoType type, int rotation)
    {
        // O-Tetromino ändert sich nicht bei Rotation
        if (type == TetrominoType.O)
            return BaseShapes[type];

        // Grundform für 0° Rotation zurückgeben
        if (rotation == 0)
            return BaseShapes[type];

        // Rotation für andere Winkel berechnen
        int[][] baseShape = BaseShapes[type];
        int size = ShapeSizes[type];

        // Rotation mehrmals anwenden, falls nötig (pro Durchlauf wird die Matrix um 90° gedreht)
        int[][] result = baseShape;
        for (int i = 0; i < rotation; i++)
        {
            result = RotateMatrixClockwise(result, size);
        }

        return result;
    }

    /*
    Dreht die Matrix um 90° im Uhrzeigersinn
    Die Zeilen werden zu Spalten, wobei die erste Zeile zur letzten Spalte wird
    [0,0] [0,1] [0,2]     [2,0] [1,0] [0,0]
    [1,0] [1,1] [1,2] --> [2,1] [1,1] [0,1]
    [2,0] [2,1] [2,2]     [2,2] [1,2] [0,2]
     */
    private static int[][] RotateMatrixClockwise(int[][] matrix, int size)
    {
        int[][] rotated = new int[size][];
        for (int i = 0; i < size; i++)
        {
            rotated[i] = new int[size];
        }

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                rotated[col][size - 1 - row] = matrix[row][col];
            }
        }

        return rotated;
    }
}