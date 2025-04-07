namespace Tetrispp.Models;

public class GameState
{
    public Dictionary<string, PlayerState> Players { get; } = new Dictionary<string, PlayerState>();
    public bool IsGameActive { get; set; } = false;
    // wichtig fürs Nachrichten senden
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;
}