using PokemonWordle.Api.Models;

namespace PokemonWordle.Api.DTOs;

public class GameStateResponse
{
    public Guid GameId { get; set; }
    public DateOnly Date { get; set; }
    public GameStatus Status { get; set; }
    public int AttemptsUsed { get; set; }
    public int AttemptsRemaining { get; set; }
    public int MaxAttempts { get; set; }
    public List<AttemptRecord> Attempts { get; set; } = [];

    /// <summary>Revealed only when the game is over (won or lost).</summary>
    public string? Answer { get; set; }
}

public class AttemptRecord
{
    public string PokemonName { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public GuessHints Hints { get; set; } = new();
}
