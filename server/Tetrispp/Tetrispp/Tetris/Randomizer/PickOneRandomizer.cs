using Tetrispp.Models;

namespace Tetrispp.Tetris.Randomizer;

/// <summary>
/// Random generator for Tetrominos.
/// Picks a type at random.
/// </summary>
public class PickOneRandomizer: IRandomizer
{
    private static readonly TetrominoType[] Types = Enum.GetValues<TetrominoType>();
    private static readonly int NumberOfTypes = Types.Length;
    
    public Tetromino GetNext()
    {
        return new Tetromino(Types[Random.Shared.Next(NumberOfTypes - 1)]);
    }
}