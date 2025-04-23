namespace Tetrispp.Models;

public class GameState
{
    public Dictionary<int, PlayerState> Players { get; } = new Dictionary<int, PlayerState>();
    public bool IsGameActive { get; set; } = false;
    // wichtig fürs Nachrichten senden
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;
}