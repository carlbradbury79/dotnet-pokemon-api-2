namespace PokemonWordle.Api.Exceptions;

public class GameAlreadyCompleteException(Guid gameId)
    : Exception($"Game with ID '{gameId}' is already complete. Start a new game to play again.");
