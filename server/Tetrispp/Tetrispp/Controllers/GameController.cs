using Microsoft.AspNetCore.Mvc;
using Tetrispp.Services;

namespace Tetrispp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameConnectionManager _gameManager;

    public GameController(GameConnectionManager gameManager)
    {
        _gameManager = gameManager;
    }

    [HttpGet("active")]
    public IActionResult GetActiveGames()
    {
        var activeGames = _gameManager.GetActiveGames();
        return Ok(activeGames);
    }
}