using System.Text.Json.Serialization;
using PokemonWordle.Api.Middleware;
using PokemonWordle.Api.Services;
using PokemonWordle.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ── OpenAPI / Swagger ─────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── HTTP clients ──────────────────────────────────────────────────────────────
// Typed client for the PokeAPI — DI injects the configured HttpClient into PokemonService.
builder.Services.AddHttpClient<IPokemonService, PokemonService>(client =>
{
    var baseUrl = builder.Configuration["PokeApi:BaseUrl"]
        ?? throw new InvalidOperationException("PokeApi:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
});

// ── Application services ──────────────────────────────────────────────────────
// Singleton keeps game state alive for the lifetime of the process.
// Replace with a database-backed implementation for a production app.
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IDailyPokemonService, DailyPokemonService>();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
