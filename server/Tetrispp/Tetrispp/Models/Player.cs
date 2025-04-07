using System.Net.WebSockets;

namespace Tetrispp.Models;

public class Player(WebSocket socket)
{
    public WebSocket Socket { get; } = socket;
    public string PlayerId { get; } = Guid.NewGuid().ToString();
}