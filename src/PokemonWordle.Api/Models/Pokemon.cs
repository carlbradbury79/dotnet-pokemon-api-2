namespace PokemonWordle.Api.Models;

public class Pokemon
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Types { get; set; } = [];

    /// <summary>
    /// Derives the generation number from the Pokemon's national dex ID.
    /// </summary>
    public int Generation => GetGenerationFromId(Id);

    private static int GetGenerationFromId(int id) => id switch
    {
        <= 151 => 1,
        <= 251 => 2,
        <= 386 => 3,
        <= 493 => 4,
        <= 649 => 5,
        <= 721 => 6,
        <= 809 => 7,
        <= 905 => 8,
        _ => 9
    };
}
