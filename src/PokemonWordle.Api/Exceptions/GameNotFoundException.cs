namespace PokemonWordle.Api.Exceptions;

public class GameNotFoundException(Guid gameId)
    : Exception($"Game with ID '{gameId}' was not found.");
