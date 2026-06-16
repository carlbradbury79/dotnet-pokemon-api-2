using System.ComponentModel.DataAnnotations;

namespace PokemonWordle.Api.DTOs;

public class SubmitGuessRequest
{
    [Required]
    [MinLength(1)]
    public string PokemonName { get; set; } = string.Empty;
}
