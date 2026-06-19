using PokemonWordle.Api.Models;

namespace PokemonWordle.Api.DTOs;

public class SubmitGuessResponse
{
    public bool IsCorrect { get; set; }
    public int AttemptsUsed { get; set; }
    public int AttemptsRemaining { get; set; }
    public GameStatus GameStatus { get; set; }
    public GuessHints Hints { get; set; } = new();

    /// <summary>Revealed only when the game is over (won or lost).</summary>
    public string? Answer { get; set; }
}
