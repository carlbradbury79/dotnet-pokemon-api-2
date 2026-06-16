using PokemonWordle.Api.Models;

namespace PokemonWordle.Api.DTOs;

public class CreateGameResponse
{
    public Guid GameId { get; set; }
    public DateOnly Date { get; set; }
    public int MaxAttempts { get; set; }
    public string Message { get; set; } = string.Empty;
}
