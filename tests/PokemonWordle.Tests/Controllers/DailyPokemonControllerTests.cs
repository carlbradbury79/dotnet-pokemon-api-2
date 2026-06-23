using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PokemonWordle.Api.Controllers;
using PokemonWordle.Api.DTOs;
using PokemonWordle.Api.Models;
using PokemonWordle.Api.Services.Interfaces;

namespace PokemonWordle.Tests.Controllers;

public class DailyPokemonControllerTests
{
private readonly Mock<IDailyPokemonService> _dailyServiceMock = new();
private readonly Mock<IPokemonService> _pokemonServiceMock = new();

[Fact]
public async Task GetDailyPokemon_ReturnsOk_WithNumberAndNameLength()
{
    _dailyServiceMock.Setup(s => s.GetDailyPokemonId()).Returns(25);
    _pokemonServiceMock
        .Setup(s => s.GetPokemonByIdAsync(25))
        .ReturnsAsync(new Pokemon { Id = 25, Name = "pikachu", Types = ["electric"] });

    var sut = new DailyPokemonController(_dailyServiceMock.Object, _pokemonServiceMock.Object);

    var result = await sut.GetDailyPokemon();

    var ok = result.Should().BeOfType<OkObjectResult>().Subject;
    var body = ok.Value.Should().BeOfType<DailyPokemonResponse>().Subject;
    body.PokemonNumber.Should().Be(25);
    body.PokemonNameLength.Should().Be(7);
}

[Fact]
public async Task GetDailyPokemon_WhenPokemonLookupFails_Returns503()
{
    _dailyServiceMock.Setup(s => s.GetDailyPokemonId()).Returns(25);
    _pokemonServiceMock
        .Setup(s => s.GetPokemonByIdAsync(25))
        .ReturnsAsync((Pokemon?)null);

    var sut = new DailyPokemonController(_dailyServiceMock.Object, _pokemonServiceMock.Object);

    var result = await sut.GetDailyPokemon();
    result.Should().BeOfType<NotFoundResult>();

}
}