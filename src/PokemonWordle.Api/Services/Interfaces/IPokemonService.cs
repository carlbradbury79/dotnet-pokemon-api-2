using PokemonWordle.Api.Models;

namespace PokemonWordle.Api.Services.Interfaces;

public interface IPokemonService
{
    Task<Pokemon?> GetPokemonByNameAsync(string name);
    Task<Pokemon?> GetPokemonByIdAsync(int id);
}
