using System.Text.Json.Serialization;

namespace Tetrispp.Models;

public class GameAction
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ActionType ActionType { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Direction? Direction { get; set; }
    public int? Rotation { get; set; }
}

public enum ActionType
{
    Move,
    Rotate,
    Drop,
    Start,
    Stop,
    Join,
    Init
}

public enum Direction
{
    Left,
    Right,
    Down,
    Drop
}