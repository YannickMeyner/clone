namespace Tetrispp.Models;


public class PlayerState
{
    public int UserId { get; set; }

    // 2D-Array: 10 Spalten, 20 Zeilen
    // 0 = leere Zelle; 1-7 = gefüllte Zelle (je nach Tetromino-Typ)
    public int[,] Grid { get; } = new int[10, 20];

    public Tetromino? CurrentBlock { get; set; }
    public Tetromino? NextBlock { get; set; }
    // Gesamtzahl der Reihen, die ein Spieler gelöscht hat
    public int LinesCleared { get; set; } = 0;
    public bool IsGameOver { get; set; } = false;
    // Vollständige Zeilen, die zum Gegner geschickt werden sollen
    public List<int[]>? CompletedLines { get; set; } = null;
}