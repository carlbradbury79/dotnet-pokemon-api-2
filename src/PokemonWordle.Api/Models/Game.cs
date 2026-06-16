namespace PokemonWordle.Api.Models;

public class Game
{
    public const int MaxAttempts = 6;

    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly Date { get; set; }
    public Pokemon DailyPokemon { get; set; } = null!;
    public List<Attempt> Attempts { get; set; } = [];
    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public int AttemptsUsed => Attempts.Count;
    public int AttemptsRemaining => MaxAttempts - AttemptsUsed;
}
