using Tetrispp.Models;

namespace Tetrispp.Tetris.Randomizer;

/// <summary>
/// Interface for random generator.
/// </summary>
public interface IRandomizer
{
    Tetromino GetNext();
}