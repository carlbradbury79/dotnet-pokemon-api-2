using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PokemonWordle.Api.Services;

namespace PokemonWordle.Tests.Services;

public class DailyPokemonServiceTests
{
    private static DailyPokemonService CreateService(int totalPokemon = 151)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Game:TotalPokemon"] = totalPokemon.ToString()
            })
            .Build();

        return new DailyPokemonService(config);
    }

    [Fact]
    public void GetDailyPokemonId_ReturnsIdWithinValidRange()
    {
        var service = CreateService(151);

        var id = service.GetDailyPokemonId();

        id.Should().BeGreaterThanOrEqualTo(1);
        id.Should().BeLessThanOrEqualTo(151);
    }

    [Fact]
    public void GetDailyPokemonId_ReturnsSameIdWhenCalledTwiceOnSameDay()
    {
        var service = CreateService();

        var id1 = service.GetDailyPokemonId();
        var id2 = service.GetDailyPokemonId();

        id1.Should().Be(id2);
    }

    [Theory]
    [InlineData(151)]
    [InlineData(251)]
    [InlineData(386)]
    public void GetDailyPokemonId_RespectsConfiguredTotalPokemon(int totalPokemon)
    {
        var service = CreateService(totalPokemon);

        var id = service.GetDailyPokemonId();

        id.Should().BeGreaterThanOrEqualTo(1);
        id.Should().BeLessThanOrEqualTo(totalPokemon);
    }
}
