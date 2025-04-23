using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tetrispp.Services;

namespace Tetrispp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreController : ControllerBase
{
    private readonly ScoreService _scoreService;

    public ScoreController(ScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    [HttpGet("highscores")]
    public async Task<IActionResult> GetHighscores([FromQuery] int count = 10)
    {
        var highscores = await _scoreService.GetHighscoresAsync(count);
        return Ok(highscores);
    }
}