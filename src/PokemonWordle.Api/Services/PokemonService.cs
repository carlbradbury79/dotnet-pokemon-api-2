using PokemonWordle.Api.External;
using PokemonWordle.Api.Models;
using PokemonWordle.Api.Services.Interfaces;

namespace PokemonWordle.Api.Services;

public class PokemonService(HttpClient httpClient, ILogger<PokemonService> logger) : IPokemonService
{
    public async Task<Pokemon?> GetPokemonByNameAsync(string name)
    {
        return await FetchPokemonAsync(name.ToLowerInvariant());
    }

    public async Task<Pokemon?> GetPokemonByIdAsync(int id)
    {
        return await FetchPokemonAsync(id.ToString());
    }

    private async Task<Pokemon?> FetchPokemonAsync(string nameOrId)
    {
        try
        {
            var response = await httpClient.GetAsync($"pokemon/{nameOrId}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("PokeAPI returned {StatusCode} for pokemon '{NameOrId}'",
                    response.StatusCode, nameOrId);
                return null;
            }

            var pokeApiResponse = await response.Content
                .ReadFromJsonAsync<PokeApiPokemonResponse>();

            if (pokeApiResponse is null)
                return null;

            return new Pokemon
            {
                Id = pokeApiResponse.Id,
                Name = pokeApiResponse.Name,
                Types = pokeApiResponse.Types
                    .Select(t => t.Type.Name)
                    .ToList()
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to contact PokeAPI for pokemon '{NameOrId}'", nameOrId);
            return null;
        }
    }
}
