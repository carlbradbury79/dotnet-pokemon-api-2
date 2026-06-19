namespace PokemonWordle.Api.Exceptions;

public class InvalidPokemonException(string name)
    : Exception($"'{name}' is not a valid Pokemon name. Please check the spelling and try again.");
