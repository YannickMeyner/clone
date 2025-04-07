namespace Tetrispp.Models;

public class Tetromino
{
    public TetrominoType Type { get; set; }
    public int Rotation { get; set; } = 0;
    public Position Position { get; set; } = new Position(3, 0); // Standard-Startposition bei Block Breite 4 (also ziemlich in der Mitte)
}

public class Position
{
    // 0-9, da das Spielfeld 10 Spalten breit ist
    public int X { get; set; }
    // 0-19, da das Spielfeld 20 Spalten hoch ist
    public int Y { get; set; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }
}