using PokemonWordle.Api.Services.Interfaces;

namespace PokemonWordle.Api.Services;

public class DailyPokemonService(IConfiguration configuration) : IDailyPokemonService
{
    private readonly int _totalPokemon = configuration.GetValue<int>("Game:TotalPokemon", 151);

    /// <inheritdoc />
    public int GetDailyPokemonId()
    {
        // Use the ordinal day number (days since epoch) as a stable, daily-changing seed.
        // This guarantees every player worldwide sees the same Pokemon on the same UTC date.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return (today.DayNumber % _totalPokemon) + 1;
    }
}
