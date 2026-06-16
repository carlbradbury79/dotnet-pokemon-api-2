namespace PokemonWordle.Api.DTOs;

public class DailyPokemonResponse
{
    public DateOnly Date { get; set; }

    /// <summary>
    /// The national Pokedex number of today's Pokemon — a hint without spoiling the name.
    /// </summary>
    public int PokemonNumber { get; set; }
}
