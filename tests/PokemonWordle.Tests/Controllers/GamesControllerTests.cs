using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PokemonWordle.Api.Controllers;
using PokemonWordle.Api.DTOs;
using PokemonWordle.Api.Exceptions;
using PokemonWordle.Api.Models;
using PokemonWordle.Api.Services.Interfaces;

namespace PokemonWordle.Tests.Controllers;

public class GamesControllerTests
{
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly GamesController _sut;

    public GamesControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _sut = new GamesController(_gameServiceMock.Object);
    }

    // ── POST /api/games ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateGame_ReturnsCreatedResult_WithGameId()
    {
        var game = BuildGame();
        _gameServiceMock.Setup(s => s.CreateGameAsync()).ReturnsAsync(game);

        var result = await _sut.CreateGame();

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<CreateGameResponse>().Subject;
        response.GameId.Should().Be(game.Id);
        response.MaxAttempts.Should().Be(Game.MaxAttempts);
    }

    // ── GET /api/games/{gameId} ───────────────────────────────────────────────

    [Fact]
    public async Task GetGame_WhenGameExists_ReturnsOk()
    {
        var game = BuildGame();
        _gameServiceMock.Setup(s => s.GetGameAsync(game.Id)).ReturnsAsync(game);

        var result = await _sut.GetGame(game.Id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<GameStateResponse>().Subject;
        response.GameId.Should().Be(game.Id);
        response.Status.Should().Be(GameStatus.InProgress);
    }

    [Fact]
    public async Task GetGame_WhenGameDoesNotExist_ReturnsNotFound()
    {
        _gameServiceMock.Setup(s => s.GetGameAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null);

        var result = await _sut.GetGame(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── POST /api/games/{gameId}/guesses ──────────────────────────────────────

    [Fact]
    public async Task SubmitGuess_WithCorrectGuess_ReturnsOkWithIsCorrectTrue()
    {
        var game = BuildGame(GameStatus.Won, isCorrectLastAttempt: true);
        var hints = new GuessHints { SharesType = true, GenerationHint = "correct" };
        _gameServiceMock
            .Setup(s => s.SubmitGuessAsync(game.Id, "pikachu"))
            .ReturnsAsync((game, hints));

        var result = await _sut.SubmitGuess(game.Id, new SubmitGuessRequest { PokemonName = "pikachu" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SubmitGuessResponse>().Subject;
        response.IsCorrect.Should().BeTrue();
        response.GameStatus.Should().Be(GameStatus.Won);
        response.Answer.Should().Be("pikachu"); // revealed when game is over
    }

    [Fact]
    public async Task SubmitGuess_WithIncorrectGuess_ReturnsOkWithIsCorrectFalse()
    {
        var game = BuildGame(GameStatus.InProgress, isCorrectLastAttempt: false, addAttempt: true);
        var hints = new GuessHints { SharesType = false, GenerationHint = "higher" };
        _gameServiceMock
            .Setup(s => s.SubmitGuessAsync(game.Id, "charmander"))
            .ReturnsAsync((game, hints));

        var result = await _sut.SubmitGuess(game.Id, new SubmitGuessRequest { PokemonName = "charmander" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SubmitGuessResponse>().Subject;
        response.IsCorrect.Should().BeFalse();
        response.GameStatus.Should().Be(GameStatus.InProgress);
        response.Answer.Should().BeNull(); // not revealed until game is over
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <param name="status">The game status to simulate.</param>
    /// <param name="isCorrectLastAttempt">Whether the last attempt should be correct.</param>
    /// <param name="addAttempt">When true, adds a recorded attempt (use whenever simulating a post-guess state).</param>
    private static Game BuildGame(
        GameStatus status = GameStatus.InProgress,
        bool isCorrectLastAttempt = false,
        bool addAttempt = false)
    {
        var game = new Game
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            DailyPokemon = new Pokemon { Id = 25, Name = "pikachu", Types = ["electric"] },
            Status = status
        };

        // Add an attempt whenever the game is over OR the caller explicitly requests one.
        if (status != GameStatus.InProgress || addAttempt)
        {
            game.Attempts.Add(new Attempt
            {
                PokemonName = isCorrectLastAttempt ? "pikachu" : "charmander",
                IsCorrect = isCorrectLastAttempt,
                GenerationHint = "correct",
                SubmittedAt = DateTime.UtcNow
            });
        }

        return game;
    }
}
