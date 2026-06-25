namespace PokemonWordle.Api.DTOs;

public enum LetterResult { Correct, Present, Absent }

public class GuessHints
{
    /// <summary>True if the guessed Pokemon shares at least one type with the daily Pokemon.</summary>
    public bool SharesType { get; set; }

    /// <summary>
    /// Indicates whether the daily Pokemon's generation is "higher", "lower", or "correct"
    /// relative to the guessed Pokemon.
    /// </summary>
    public string GenerationHint { get; set; } = string.Empty;
    public List<LetterResult> LetterHints { get; set; } = [];
}
