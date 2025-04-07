namespace Tetrispp.Models;

public class TetrominoShapes
{
    public static readonly Dictionary<TetrominoType, int[][][]> Shapes = new()
    {
        [TetrominoType.I] = new[]
        {
            // 0° Rotation
            new[]
            {
                new[] {0,0,0,0},
                new[] {1,1,1,1},
                new[] {0,0,0,0},
                new[] {0,0,0,0}
            },
            // 90° Rotation
            new[]
            {
                new[] {0,0,1,0},
                new[] {0,0,1,0},
                new[] {0,0,1,0},
                new[] {0,0,1,0}
            },
            // 180° Rotation
            new[]
            {
                new[] {0,0,0,0},
                new[] {0,0,0,0},
                new[] {1,1,1,1},
                new[] {0,0,0,0}
            },
            // 270° Rotation
            new[]
            {
                new[] {0,1,0,0},
                new[] {0,1,0,0},
                new[] {0,1,0,0},
                new[] {0,1,0,0}
            }
        },

        [TetrominoType.O] = new[]
        {
            new[]
            {
                new[] {2,2},
                new[] {2,2}
            },
            new[]
            {
                new[] {2,2},
                new[] {2,2}
            },
            new[]
            {
                new[] {2,2},
                new[] {2,2}
            },
            new[]
            {
                new[] {2,2},
                new[] {2,2}
            }
        },

        [TetrominoType.T] = new[]
        {
            new[]
            {
                new[] {0,3,0},
                new[] {3,3,3},
                new[] {0,0,0}
            },
            new[]
            {
                new[] {0,3,0},
                new[] {0,3,3},
                new[] {0,3,0}
            },
            new[]
            {
                new[] {0,0,0},
                new[] {3,3,3},
                new[] {0,3,0}
            },
            new[]
            {
                new[] {0,3,0},
                new[] {3,3,0},
                new[] {0,3,0}
            }
        },

        [TetrominoType.S] = new[]
        {
            new[]
            {
                new[] {0,4,4},
                new[] {4,4,0},
                new[] {0,0,0}
            },
            new[]
            {
                new[] {0,4,0},
                new[] {0,4,4},
                new[] {0,0,4}
            },
            new[]
            {
                new[] {0,0,0},
                new[] {0,4,4},
                new[] {4,4,0}
            },
            new[]
            {
                new[] {4,0,0},
                new[] {4,4,0},
                new[] {0,4,0}
            }
        },

        [TetrominoType.Z] = new[]
        {
            new[]
            {
                new[] {5,5,0},
                new[] {0,5,5},
                new[] {0,0,0}
            },
            new[]
            {
                new[] {0,0,5},
                new[] {0,5,5},
                new[] {0,5,0}
            },
            new[]
            {
                new[] {0,0,0},
                new[] {5,5,0},
                new[] {0,5,5}
            },
            new[]
            {
                new[] {0,5,0},
                new[] {5,5,0},
                new[] {5,0,0}
            }
        },

        [TetrominoType.J] = new[]
        {
            new[]
            {
                new[] {6,0,0},
                new[] {6,6,6},
                new[] {0,0,0}
            },
            new[]
            {
                new[] {0,6,6},
                new[] {0,6,0},
                new[] {0,6,0}
            },
            new[]
            {
                new[] {0,0,0},
                new[] {6,6,6},
                new[] {0,0,6}
            },
            new[]
            {
                new[] {0,6,0},
                new[] {0,6,0},
                new[] {6,6,0}
            }
        },

        [TetrominoType.L] = new[]
        {
            new[]
            {
                new[] {0,0,7},
                new[] {7,7,7},
                new[] {0,0,0}
            },
            new[]
            {
                new[] {0,7,0},
                new[] {0,7,0},
                new[] {0,7,7}
            },
            new[]
            {
                new[] {0,0,0},
                new[] {7,7,7},
                new[] {7,0,0}
            },
            new[]
            {
                new[] {7,7,0},
                new[] {0,7,0},
                new[] {0,7,0}
            }
        }
    };

    // Tetromino-Grössen
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

    // Zufälligen Tetromino-Typ generieren
    public static TetrominoType GetRandomType()
    {
        var values = Enum.GetValues<TetrominoType>();
        Random rnd = new();
        return values[rnd.Next(values.Length)];
    }

    // Tetromino-Typ zu Wert im Grid
    public static int GetValueForType(TetrominoType type)
    {
        // Wir nehmen einfach den Enum-Wert + 1, da Enum bei 0 beginnt
        // und wir 0 für leere Zellen reservieren
        return (int)type + 1;
    }
}