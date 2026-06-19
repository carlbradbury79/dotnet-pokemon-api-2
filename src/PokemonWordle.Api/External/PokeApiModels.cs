using System.Text.Json.Serialization;

namespace PokemonWordle.Api.External;

/// <summary>
/// Represents the relevant fields from the PokeAPI /pokemon/{name} endpoint.
/// See: https://pokeapi.co/docs/v2#pokemon
/// </summary>
public class PokeApiPokemonResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("types")]
    public List<PokeApiTypeSlot> Types { get; set; } = [];
}

public class PokeApiTypeSlot
{
    [JsonPropertyName("type")]
    public PokeApiType Type { get; set; } = new();
}

public class PokeApiType
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
