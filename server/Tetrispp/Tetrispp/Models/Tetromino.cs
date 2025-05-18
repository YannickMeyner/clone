namespace Tetrispp.Models;

public class Tetromino
{
    public TetrominoType Type { get; set; }
    public int Rotation { get; set; }
    public Position Position { get; set; } = new(3, 0); // Standard-Startposition bei Block Breite 4 (also ziemlich in der Mitte)
    
    // Floor-Kick Properties
    public bool IsOnFloor { get; set; }
    public DateTime FloorTouchTime { get; set; } = DateTime.MinValue;
    public int MovesSinceFloorTouch { get; set; }
    
    public Tetromino(TetrominoType type)
    {
        Type = type;
    }
}
