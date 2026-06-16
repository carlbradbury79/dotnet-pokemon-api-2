using Microsoft.AspNetCore.Mvc;
using PokemonWordle.Api.DTOs;
using PokemonWordle.Api.Services.Interfaces;

namespace PokemonWordle.Api.Controllers;

[ApiController]
[Route("api/daily-pokemon")]
public class DailyPokemonController(IDailyPokemonService dailyPokemonService) : ControllerBase
{
    /// <summary>
    /// Returns today's Pokemon number (national Pokedex ID) as a hint.
    /// The Pokemon's name is intentionally not revealed — use POST /api/games to start guessing!
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DailyPokemonResponse), StatusCodes.Status200OK)]
    public IActionResult GetDailyPokemon()
    {
        var pokemonId = dailyPokemonService.GetDailyPokemonId();

        return Ok(new DailyPokemonResponse
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            PokemonNumber = pokemonId
        });
    }
}
