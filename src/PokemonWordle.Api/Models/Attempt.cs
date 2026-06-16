namespace PokemonWordle.Api.Models;

public class Attempt
{
    public string PokemonName { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public bool SharesType { get; set; }
    public string GenerationHint { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
