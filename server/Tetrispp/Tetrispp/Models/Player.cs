using System.Net.WebSockets;

namespace Tetrispp.Models;

public class Player
{
    public WebSocket Socket { get; }
    public int UserId { get; }

    public Player(WebSocket socket, int userId)
    {
        Socket = socket;
        UserId = userId;
    }
}