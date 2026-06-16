using Microsoft.AspNetCore.Mvc;
using PokemonWordle.Api.DTOs;
using PokemonWordle.Api.Models;
using PokemonWordle.Api.Services.Interfaces;

namespace PokemonWordle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(IGameService gameService) : ControllerBase
{
    /// <summary>Start a new game for today's daily Pokemon.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateGameResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGame()
    {
        var game = await gameService.CreateGameAsync();

        var response = new CreateGameResponse
        {
            GameId = game.Id,
            Date = game.Date,
            MaxAttempts = Game.MaxAttempts,
            Message = $"Game started! You have {Game.MaxAttempts} attempts to guess today's Pokemon."
        };

        return CreatedAtAction(nameof(GetGame), new { gameId = game.Id }, response);
    }

    /// <summary>Get the current state of a game.</summary>
    [HttpGet("{gameId:guid}")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGame(Guid gameId)
    {
        var game = await gameService.GetGameAsync(gameId);

        if (game is null)
            return NotFound(new { message = $"Game '{gameId}' not found." });

        return Ok(MapToGameStateResponse(game));
    }

    /// <summary>Submit a Pokemon name as a guess.</summary>
    [HttpPost("{gameId:guid}/guesses")]
    [ProducesResponseType(typeof(SubmitGuessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitGuess(Guid gameId, [FromBody] SubmitGuessRequest request)
    {
        var (game, hints) = await gameService.SubmitGuessAsync(gameId, request.PokemonName);

        var isGameOver = game.Status != GameStatus.InProgress;

        var response = new SubmitGuessResponse
        {
            IsCorrect = game.Attempts[^1].IsCorrect,
            AttemptsUsed = game.AttemptsUsed,
            AttemptsRemaining = game.AttemptsRemaining,
            GameStatus = game.Status,
            Hints = hints,
            Answer = isGameOver ? game.DailyPokemon.Name : null
        };

        return Ok(response);
    }

    private static GameStateResponse MapToGameStateResponse(Game game)
    {
        var isGameOver = game.Status != GameStatus.InProgress;

        return new GameStateResponse
        {
            GameId = game.Id,
            Date = game.Date,
            Status = game.Status,
            AttemptsUsed = game.AttemptsUsed,
            AttemptsRemaining = game.AttemptsRemaining,
            MaxAttempts = Game.MaxAttempts,
            Answer = isGameOver ? game.DailyPokemon.Name : null,
            Attempts = game.Attempts.Select(a => new AttemptRecord
            {
                PokemonName = a.PokemonName,
                IsCorrect = a.IsCorrect,
                Hints = new DTOs.GuessHints
                {
                    SharesType = a.SharesType,
                    GenerationHint = a.GenerationHint
                }
            }).ToList()
        };
    }
}
