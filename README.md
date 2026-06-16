# Pokemon Wordle API

A bare-bones **.NET 10** REST API for a Pokémon Wordle-like guessing game — built as a learning foundation.

## 🎮 How the game works

1. **Get a hint** — `GET /api/daily-pokemon` returns today's Pokédex *number* (not the name).
2. **Start a game** — `POST /api/games` creates a session and gives you a `gameId`.
3. **Guess!** — `POST /api/games/{gameId}/guesses` with a Pokémon name. You get up to **6 attempts**.
4. **Read the hints** in each response to narrow it down:
   - `sharesType` — does your guess share a type with today's Pokémon?
   - `generationHint` — is the answer a `higher`, `lower`, or `correct` generation?
5. **Review your game** anytime — `GET /api/games/{gameId}`.

The same Pokémon is used for all players on the same UTC day.

---

## 📁 Project structure

```
src/
  PokemonWordle.Api/
    Controllers/        ← HTTP endpoints (GamesController, DailyPokemonController)
    DTOs/               ← Request and response objects
    Exceptions/         ← Custom domain exceptions
    External/           ← PokeAPI deserialization models
    Middleware/         ← Global exception handler
    Models/             ← Domain models (Game, Pokemon, Attempt)
    Services/
      Interfaces/       ← Service contracts (good place to swap implementations)
      *.cs              ← Implementations (PokemonService, GameService, DailyPokemonService)

tests/
  PokemonWordle.Tests/
    Controllers/        ← Controller unit tests
    Services/           ← Service unit tests

postman/
  PokemonWordle.postman_collection.json   ← Ready-to-import Postman collection
```

---

## 🚀 Running the API

```bash
cd src/PokemonWordle.Api
dotnet run
```

The API starts at `http://localhost:5141` (or `https://localhost:7208`).  
Swagger UI is available at `/openapi/v1.json` in development.

---

## 🧪 Running tests

```bash
dotnet test
```

The test suite uses **xUnit**, **Moq**, and **FluentAssertions**.

---

## 📬 Postman

Import `postman/PokemonWordle.postman_collection.json` into Postman.  
The collection:
- Sets `baseUrl` to `http://localhost:5141` (matches `launchSettings.json`)
- Automatically stores the `gameId` from **Create Game** for use in subsequent requests
- Includes test assertions on each request

---

## ⚙️ Configuration (`appsettings.json`)

| Key | Default | Description |
|-----|---------|-------------|
| `PokeApi:BaseUrl` | `https://pokeapi.co/api/v2/` | PokeAPI base URL |
| `Game:TotalPokemon` | `151` | Pool of Pokémon used for the daily pick (Gen 1 only by default) |

---

## 🗺️ API reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET`  | `/api/daily-pokemon` | Today's Pokédex number (hint) |
| `POST` | `/api/games` | Start a new game |
| `GET`  | `/api/games/{gameId}` | Get current game state |
| `POST` | `/api/games/{gameId}/guesses` | Submit a Pokémon name guess |

---

## 💡 What to build next

Some ideas for extending this project:

- **Persistent storage** — replace the in-memory `ConcurrentDictionary` in `GameService` with a proper database (e.g. EF Core + SQLite/Postgres)
- **Leaderboard** — track win streaks and share-able results
- **Authentication** — tie games to users with ASP.NET Identity or JWTs
- **Expanded hints** — compare height, weight, egg group, or habitat
- **Expanded Pokédex** — change `Game:TotalPokemon` in config to include more generations
- **Integration tests** — use `WebApplicationFactory<Program>` to test the full HTTP pipeline
