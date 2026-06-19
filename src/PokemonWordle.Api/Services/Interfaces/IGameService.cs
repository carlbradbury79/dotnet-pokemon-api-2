using PokemonWordle.Api.DTOs;
using PokemonWordle.Api.Models;

namespace PokemonWordle.Api.Services.Interfaces;

public interface IGameService
{
    Task<Game> CreateGameAsync();
    Task<Game?> GetGameAsync(Guid gameId);
    Task<(Game Game, GuessHints Hints)> SubmitGuessAsync(Guid gameId, string pokemonName);
}
