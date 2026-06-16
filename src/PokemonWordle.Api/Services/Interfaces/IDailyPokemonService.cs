namespace PokemonWordle.Api.Services.Interfaces;

public interface IDailyPokemonService
{
    /// <summary>
    /// Returns the national Pokedex ID for today's daily Pokemon.
    /// The selection is deterministic — the same ID is returned for the entire UTC day.
    /// </summary>
    int GetDailyPokemonId();
}
