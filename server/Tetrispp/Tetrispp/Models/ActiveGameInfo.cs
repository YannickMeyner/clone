namespace Tetrispp.Models;

public class ActiveGameInfo
{
    public string? RoomId { get; set; }
    public List<GamePlayerInfo> Players { get; set; } = new();
}

public class GamePlayerInfo
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public int LinesCleared { get; set; }
}