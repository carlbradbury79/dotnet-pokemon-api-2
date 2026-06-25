using System.Collections.Concurrent;
using PokemonWordle.Api.DTOs;
using PokemonWordle.Api.Exceptions;
using PokemonWordle.Api.Models;
using PokemonWordle.Api.Services.Interfaces;

namespace PokemonWordle.Api.Services;

/// <summary>
/// Manages game sessions using an in-memory store.
/// In a production application this would be replaced with a database-backed repository.
/// </summary>
public class GameService(
    IDailyPokemonService dailyPokemonService,
    IPokemonService pokemonService,
    ILogger<GameService> logger) : IGameService
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public async Task<Game> CreateGameAsync()
    {
        var dailyId = dailyPokemonService.GetDailyPokemonId();
        var pokemon = await pokemonService.GetPokemonByIdAsync(dailyId);

        if (pokemon is null)
        {
            logger.LogError("Failed to fetch daily Pokemon with ID {PokemonId}", dailyId);
            throw new InvalidOperationException("Unable to fetch the daily Pokemon. Please try again later.");
        }

        var game = new Game
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            DailyPokemon = pokemon
        };

        _games[game.Id] = game;

        logger.LogInformation("Game {GameId} created for Pokemon #{PokemonId}", game.Id, dailyId);

        return game;
    }

    public Task<Game?> GetGameAsync(Guid gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return Task.FromResult(game);
    }

    public async Task<(Game Game, GuessHints Hints)> SubmitGuessAsync(Guid gameId, string pokemonName)
    {
        if (!_games.TryGetValue(gameId, out var game))
            throw new GameNotFoundException(gameId);

        if (game.Status != GameStatus.InProgress)
            throw new GameAlreadyCompleteException(gameId);

        var guessedPokemon = await pokemonService.GetPokemonByNameAsync(pokemonName);

        var isCorrect = string.Equals(
            guessedPokemon?.Name ?? pokemonName,
            game.DailyPokemon.Name,
            StringComparison.OrdinalIgnoreCase);

        var hints = BuildHints(guessedPokemon, game.DailyPokemon, pokemonName);

        lock (game)
        {
            if (game.Status != GameStatus.InProgress)
                throw new GameAlreadyCompleteException(gameId);

            game.Attempts.Add(new Attempt
            {
                PokemonName = guessedPokemon?.Name ?? pokemonName,
                IsCorrect = isCorrect,
                SharesType = hints.SharesType,
                GenerationHint = hints.GenerationHint,
                SubmittedAt = DateTime.UtcNow
            });

            if (isCorrect) game.Status = GameStatus.Won;
            else if (game.AttemptsUsed >= Game.MaxAttempts) game.Status = GameStatus.Lost;
        }
        logger.LogInformation(
            "Game {GameId}: guess '{PokemonName}' — correct={IsCorrect}, status={Status}",
            gameId, Sanitize(pokemonName), isCorrect, game.Status);

        return (game, hints);
    }

    private static List<LetterResult> BuildLetterHints(string guess, string answer)
    {
        // Normalise to uppercase, strip non-alpha if desired
        var g = guess.ToUpperInvariant();
        var a = answer.ToUpperInvariant();

        var results = Enumerable.Repeat(LetterResult.Absent, g.Length).ToArray();
        var answerPool = a.ToCharArray();         // tracks which answer letters are "unconsumed"

        // Pass 1: exact matches
        for (int i = 0; i < g.Length; i++)
        {
            if (i < a.Length && g[i] == a[i])
            {
                results[i] = LetterResult.Correct;
                answerPool[i] = '\0'; // consume
            }
        }

        // Pass 2: present but wrong position
        for (int i = 0; i < g.Length; i++)
        {
            if (results[i] == LetterResult.Correct) continue;

            int poolIdx = Array.IndexOf(answerPool, g[i]);
            if (poolIdx >= 0)
            {
                results[i] = LetterResult.Present;
                answerPool[poolIdx] = '\0'; // consume so duplicates aren't double-counted
            }
            // else stays Absent
        }

        return results.ToList();
    }

    private static GuessHints BuildHints(Pokemon? guessed, Pokemon daily, string guessedName)
    {
        var sharesType = guessed is not null &&
            guessed.Types.Intersect(daily.Types, StringComparer.OrdinalIgnoreCase).Any();

        var generationHint = guessed is null ? "unknown" : guessed.Generation.CompareTo(daily.Generation) switch
        {
            < 0 => "higher",
            > 0 => "lower",
            _ => "correct"
        };

        var letterHints = BuildLetterHints(guessedName, daily.Name);

        return new GuessHints
        {
            SharesType = sharesType,
            GenerationHint = generationHint,
            LetterHints = letterHints
        };
    }

    /// <summary>Removes newline characters from a value to prevent log-injection attacks.</summary>
    private static string Sanitize(string? value) =>
        value?.Replace("\r", " ").Replace("\n", " ").Trim() ?? string.Empty;
}
