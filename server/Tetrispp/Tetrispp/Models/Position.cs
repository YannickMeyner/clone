namespace Tetrispp.Models;

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