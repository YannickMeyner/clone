using Tetrispp.Models;

namespace Tetrispp.Tetris.Randomizer;

/// <summary>
/// Classic Tetris random generator.
/// Picks a random Tetromino from a "bag" of the 7 types.
/// Once the "bag" is empty, it is refilled.
/// </summary>
public class ClassicRandomizer: IRandomizer
{
    private static readonly int NumberOfTypes = Enum.GetValues<TetrominoType>().Length;
    
    private int pos = 0;
    private TetrominoType[] bag;
    
    public ClassicRandomizer()
    {
        Refill();
    }
    
    public Tetromino GetNext()
    {
        if (pos < NumberOfTypes) 
            return new Tetromino(bag[pos++]);
        
        Refill();
        pos = 0;
        return new Tetromino(bag[pos++]);
    }

    private void Refill()
    {
        bag = Enum.GetValues<TetrominoType>();
        Random.Shared.Shuffle(bag);
    }
}