using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PokemonWordle.Api.Exceptions;
using PokemonWordle.Api.Models;
using PokemonWordle.Api.Services;
using PokemonWordle.Api.Services.Interfaces;
using PokemonWordle.Api.DTOs;

namespace PokemonWordle.Tests.Services;

public class GameServiceTests
{
    private readonly Mock<IDailyPokemonService> _dailyPokemonServiceMock;
    private readonly Mock<IPokemonService> _pokemonServiceMock;
    private readonly GameService _sut;

    public GameServiceTests()
    {
        _dailyPokemonServiceMock = new Mock<IDailyPokemonService>();
        _pokemonServiceMock = new Mock<IPokemonService>();
        _sut = new GameService(
            _dailyPokemonServiceMock.Object,
            _pokemonServiceMock.Object,
            NullLogger<GameService>.Instance);
    }

    // ── CreateGameAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateGame_ReturnsGame_WithInProgressStatus()
    {
        SetupDailyPokemon(BuildPokemon(25, "pikachu", ["electric"]));

        var game = await _sut.CreateGameAsync();

        game.Status.Should().Be(GameStatus.InProgress);
        game.Attempts.Should().BeEmpty();
        game.AttemptsUsed.Should().Be(0);
        game.AttemptsRemaining.Should().Be(Game.MaxAttempts);
    }

    [Fact]
    public async Task CreateGame_AssignsTodaysDate()
    {
        SetupDailyPokemon(BuildPokemon(1, "bulbasaur", ["grass", "poison"]));

        var game = await _sut.CreateGameAsync();

        game.Date.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public async Task CreateGame_ThrowsInvalidOperationException_WhenPokemonFetchFails()
    {
        _dailyPokemonServiceMock.Setup(s => s.GetDailyPokemonId()).Returns(1);
        _pokemonServiceMock.Setup(s => s.GetPokemonByIdAsync(1)).ReturnsAsync((Pokemon?)null);

        var act = () => _sut.CreateGameAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── GetGameAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetGame_ReturnsNull_WhenGameDoesNotExist()
    {
        var result = await _sut.GetGameAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGame_ReturnsGame_AfterItIsCreated()
    {
        SetupDailyPokemon(BuildPokemon(4, "charmander", ["fire"]));
        var created = await _sut.CreateGameAsync();

        var retrieved = await _sut.GetGameAsync(created.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(created.Id);
    }

    // ── SubmitGuessAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitGuess_WithCorrectPokemon_SetsGameStatusToWon()
    {
        var daily = BuildPokemon(25, "pikachu", ["electric"]);
        SetupDailyPokemon(daily);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("pikachu")).ReturnsAsync(daily);

        var game = await _sut.CreateGameAsync();
        var (updatedGame, _) = await _sut.SubmitGuessAsync(game.Id, "pikachu");

        updatedGame.Status.Should().Be(GameStatus.Won);
        updatedGame.Attempts.Should().HaveCount(1);
        updatedGame.Attempts[0].IsCorrect.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitGuess_WithIncorrectPokemon_KeepsGameInProgress()
    {
        var daily = BuildPokemon(25, "pikachu", ["electric"]);
        SetupDailyPokemon(daily);
        var wrong = BuildPokemon(4, "charmander", ["fire"]);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("charmander")).ReturnsAsync(wrong);

        var game = await _sut.CreateGameAsync();
        var (updatedGame, _) = await _sut.SubmitGuessAsync(game.Id, "charmander");

        updatedGame.Status.Should().Be(GameStatus.InProgress);
        updatedGame.Attempts.Should().HaveCount(1);
        updatedGame.Attempts[0].IsCorrect.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitGuess_After6WrongGuesses_SetsGameStatusToLost()
    {
        var daily = BuildPokemon(25, "pikachu", ["electric"]);
        SetupDailyPokemon(daily);
        var wrong = BuildPokemon(4, "charmander", ["fire"]);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("charmander")).ReturnsAsync(wrong);

        var game = await _sut.CreateGameAsync();

        for (var i = 0; i < Game.MaxAttempts; i++)
        {
            await _sut.SubmitGuessAsync(game.Id, "charmander");
        }

        game.Status.Should().Be(GameStatus.Lost);
    }

    [Fact]
    public async Task SubmitGuess_OnCompletedGame_ThrowsGameAlreadyCompleteException()
    {
        var daily = BuildPokemon(25, "pikachu", ["electric"]);
        SetupDailyPokemon(daily);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("pikachu")).ReturnsAsync(daily);

        var game = await _sut.CreateGameAsync();
        await _sut.SubmitGuessAsync(game.Id, "pikachu"); // win the game

        var act = () => _sut.SubmitGuessAsync(game.Id, "pikachu");

        await act.Should().ThrowAsync<GameAlreadyCompleteException>();
    }

    [Fact]
    public async Task SubmitGuess_WithUnknownGameId_ThrowsGameNotFoundException()
    {
        var act = () => _sut.SubmitGuessAsync(Guid.NewGuid(), "pikachu");

        await act.Should().ThrowAsync<GameNotFoundException>();
    }

    [Fact]
    public async Task SubmitGuess_WithUnknownName_IsRecordedAsAttempt()
    {
        SetupDailyPokemon(BuildPokemon(25, "pikachu", ["electric"]));
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("notapokemon")).ReturnsAsync((Pokemon?)null);

        var game = await _sut.CreateGameAsync();
        var (updatedGame, hints) = await _sut.SubmitGuessAsync(game.Id, "notapokemon");

        updatedGame.Attempts.Should().HaveCount(1);
        updatedGame.Attempts[0].PokemonName.Should().Be("notapokemon");
        updatedGame.Attempts[0].IsCorrect.Should().BeFalse();
        hints.SharesType.Should().BeFalse();
        hints.GenerationHint.Should().Be("unknown");
    }

    [Fact]
    public async Task SubmitGuess_WithSharedType_ReturnsSharesTypeTrue()
    {
        var daily = BuildPokemon(25, "pikachu", ["electric"]);
        SetupDailyPokemon(daily);
        var guess = BuildPokemon(135, "jolteon", ["electric"]);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("jolteon")).ReturnsAsync(guess);

        var game = await _sut.CreateGameAsync();
        var (_, hints) = await _sut.SubmitGuessAsync(game.Id, "jolteon");

        hints.SharesType.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitGuess_WithDifferentGeneration_ReturnsCorrectGenerationHint()
    {
        var daily = BuildPokemon(25, "pikachu", ["electric"]); // Gen 1
        SetupDailyPokemon(daily);
        var guess = BuildPokemon(155, "cyndaquil", ["fire"]); // Gen 2 — answer is lower gen
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("cyndaquil")).ReturnsAsync(guess);

        var game = await _sut.CreateGameAsync();
        var (_, hints) = await _sut.SubmitGuessAsync(game.Id, "cyndaquil");

        hints.GenerationHint.Should().Be("lower");
    }

    // ── BuildLetterHints ─────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitGuess_ExactMatch_AllLetterHintsAreCorrect()
    {
        var daily = BuildPokemon(25, "pikachu", ["electric"]);
        SetupDailyPokemon(daily);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("pikachu")).ReturnsAsync(daily);

        var game = await _sut.CreateGameAsync();
        var (_, hints) = await _sut.SubmitGuessAsync(game.Id, "pikachu");

        hints.LetterHints.Should().OnlyContain(r => r == LetterResult.Correct);
    }

    [Fact]
    public async Task SubmitGuess_NoLettersInCommon_AllLetterHintsAreAbsent()
    {
        // answer has only 'a's; guess has only 'b's — zero overlap
        var daily = BuildPokemon(1, "aaa", ["normal"]);
        SetupDailyPokemon(daily);
        var guess = BuildPokemon(2, "bbb", ["fire"]);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("bbb")).ReturnsAsync(guess);

        var game = await _sut.CreateGameAsync();
        var (_, hints) = await _sut.SubmitGuessAsync(game.Id, "bbb");

        hints.LetterHints.Should().OnlyContain(r => r == LetterResult.Absent);
    }

    [Fact]
    public async Task SubmitGuess_CorrectLettersInWrongPosition_MarkedPresent()
    {
        // guess="act", answer="cat" → [Present, Present, Correct]
        var daily = BuildPokemon(1, "cat", ["normal"]);
        SetupDailyPokemon(daily);
        var guess = BuildPokemon(2, "act", ["fire"]);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("act")).ReturnsAsync(guess);

        var game = await _sut.CreateGameAsync();
        var (_, hints) = await _sut.SubmitGuessAsync(game.Id, "act");

        hints.LetterHints.Should().Equal(LetterResult.Present, LetterResult.Present, LetterResult.Correct);
    }

    [Fact]
    public async Task SubmitGuess_DuplicateLetterInGuessButOnlyOneInAnswer_SecondOccurrenceIsAbsent()
    {
        // guess="aab", answer="bca" — answer has one 'a'; only first 'a' in guess should be Present
        // Pass1: no exact matches. Pass2: guess[0]='a' → Present (consumes pool 'a'); guess[1]='a' → Absent
        var daily = BuildPokemon(1, "bca", ["normal"]);
        SetupDailyPokemon(daily);
        var guess = BuildPokemon(2, "aab", ["fire"]);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("aab")).ReturnsAsync(guess);

        var game = await _sut.CreateGameAsync();
        var (_, hints) = await _sut.SubmitGuessAsync(game.Id, "aab");

        hints.LetterHints.Should().Equal(LetterResult.Present, LetterResult.Absent, LetterResult.Present);
    }

    [Fact]
    public async Task SubmitGuess_GuessLongerThanAnswer_ExtraLettersAreAbsent()
    {
        // guess="abcde", answer="abc" → first three Correct, last two Absent
        var daily = BuildPokemon(1, "abc", ["normal"]);
        SetupDailyPokemon(daily);
        var guess = BuildPokemon(2, "abcde", ["fire"]);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("abcde")).ReturnsAsync(guess);

        var game = await _sut.CreateGameAsync();
        var (_, hints) = await _sut.SubmitGuessAsync(game.Id, "abcde");

        hints.LetterHints.Should().Equal(
            LetterResult.Correct, LetterResult.Correct, LetterResult.Correct,
            LetterResult.Absent, LetterResult.Absent);
    }

    [Fact]
    public async Task SubmitGuess_GuessShorterThanAnswer_HintsOnlyCoverGuessLength()
    {
        // guess="abc", answer="abcde" → only three hints, all Correct
        var daily = BuildPokemon(1, "abcde", ["normal"]);
        SetupDailyPokemon(daily);
        var guess = BuildPokemon(2, "abc", ["fire"]);
        _pokemonServiceMock.Setup(s => s.GetPokemonByNameAsync("abc")).ReturnsAsync(guess);

        var game = await _sut.CreateGameAsync();
        var (_, hints) = await _sut.SubmitGuessAsync(game.Id, "abc");

        hints.LetterHints.Should().HaveCount(3)
            .And.OnlyContain(r => r == LetterResult.Correct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetupDailyPokemon(Pokemon pokemon)
    {
        _dailyPokemonServiceMock.Setup(s => s.GetDailyPokemonId()).Returns(pokemon.Id);
        _pokemonServiceMock.Setup(s => s.GetPokemonByIdAsync(pokemon.Id)).ReturnsAsync(pokemon);
    }

    private static Pokemon BuildPokemon(int id, string name, string[] types) =>
        new() { Id = id, Name = name, Types = [.. types] };
}
