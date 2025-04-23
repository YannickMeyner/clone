using Microsoft.EntityFrameworkCore;
using Tetrispp.Data;
using Tetrispp.Models.Db;

namespace Tetrispp.Services;

public class ScoreService
{
    private readonly SqlContext _context;

    public ScoreService(SqlContext context)
    {
        _context = context;
    }

    public async Task SavePlayerScoreAsync(int userId, string roomId, int linesCleared, bool isWinner)
    {
        var playerScore = new PlayerScore
        {
            UserId = userId,
            RoomId = roomId,
            LinesCleared = linesCleared,
            IsWinner = isWinner
        };

        _context.PlayerScores.Add(playerScore);
        await _context.SaveChangesAsync();
    }

    public async Task<List<HighscoreDto>> GetHighscoresAsync(int count = 10)
    {
        return await _context.PlayerScores
            .OrderByDescending(p => p.LinesCleared)
            .ThenBy(p => p.Date)
            .Take(count)
            .Select(p => new HighscoreDto
            {
                Username = p.User!.Username,
                LinesCleared = p.LinesCleared,
                Date = p.Date
            })
            .ToListAsync();
    }
}

public class HighscoreDto
{
    public required string Username { get; set; }
    public int LinesCleared { get; set; }
    public DateTime Date { get; set; }
}
