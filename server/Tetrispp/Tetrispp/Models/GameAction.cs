using System.Text.Json.Serialization;

namespace Tetrispp.Models;

public class GameAction
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ActionType ActionType { get; set; }
    public string? Direction { get; set; }
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