# AppSimple

AppSimple is a .NET 10 starter solution demonstrating clean architecture, separation of concerns, and testability. It provides a fully-implemented core business layer and SQLite data access layer, ready to be consumed by future host projects.

## Table of Contents

- [Project Structure](#project-structure)
- [Future Projects](#future-projects)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [DI Wiring (host project startup)](#di-wiring-host-project-startup)
- [AppSimple.Core](#appsimplecore)
- [AppSimple.DataLib](#appsimpledatalib)
- [Database schema](#database-schema)
- [Testing](#testing)
- [Tech Stack](#tech-stack)
- [Security notes](#security-notes)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)

## Project Structure

```text
AppSimple/
├── docs/                          # Architecture, design decisions, and diagrams
├── src/
│   ├── AppSimple.Core/            # Domain models, interfaces, services, auth, logging, validators
│   ├── AppSimple.Core.Tests/      # Unit tests for Core (208 tests)
│   ├── AppSimple.DataLib/         # SQLite/Dapper data access, implements Core interfaces
│   ├── AppSimple.DataLib.Tests/   # Integration tests for DataLib (36 tests)
│   └── AppSimple.sln
├── .github/
│   └── copilot-instructions.md   # Code conventions for AI-assisted development
└── README.md
```

## Future Projects

| Project | Connects via | Purpose |
|---|---|---|
| `AppSimple.WebApi` | Core + DataLib (direct) | ASP.NET Core Web API — REST endpoints |
| `AppSimple.AdminCli` | WebApi (HTTP) | Admin CLI — seed admin user, manage users via API |
| `AppSimple.UserCLI` | Core + DataLib (direct) | End-user CLI — local access to Core services, no WebApi required — **branch: UserCLI** |
| `AppSimple.WebApp` | WebApi (HTTP) | ASP.NET Core MVC — user-facing GUI served from the browser |
| `AppSimple.MvvmApp` | Core + DataLib (direct) | WPF/MVVM desktop application — offline-capable, uses CommunityToolkit.Mvvm — **branch: MvvmApp** (Windows only) |
| `AppSimple.WebApi.Tests` | — | Unit + integration tests for Web API |
| `AppSimple.AdminCli.Tests` | — | Unit tests for Admin CLI |
| `AppSimple.UserCLI.Tests` | — | Unit tests for User CLI |
| `AppSimple.WebApp.Tests` | — | Unit + integration tests for Web App |
| `AppSimple.MvvmApp.Tests` | — | Unit tests for MVVM application |

## Architecture

See [`docs/architecture.md`](docs/architecture.md) for a detailed breakdown.

```text
                  ┌────────────────────────────────────────────────────────────────┐
                  │              Projects that connect via HTTP                    │
                  │          AdminCli (HTTP) · WebApp (HTTP)                       │
                  └───────────────────────────────┬────────────────────────────────┘
                                                  │ HTTP / REST
                  ┌───────────────────────────────▼────────────────────────────────┐
                  │                    AppSimple.WebApi                            │
                  └───────────────────────────────┬────────────────────────────────┘
                                                  │ depends on
┌─────────────────────────────────────────────────▼────────────────────────────────────────────┐
│                   Projects that reference Core + DataLib directly                            │
│              WebApi · UserCLI · MvvmApp                                                      │
└───────────────────────────────────────┬──────────────────────────────────────────────────────┘
                                        │ depends on
            ┌───────────────────────────▼───────────────────────────┐
            │                   AppSimple.Core                      │
            │  Models · Services · Auth · Logging · Validators      │
            └───────────────────────────┬───────────────────────────┘
                                        │ implements interfaces from
            ┌───────────────────────────▼───────────────────────────┐
            │                AppSimple.DataLib                      │
            │        SQLite · Dapper · UserRepository               │
            └───────────────────────────────────────────────────────┘
```

## Prerequisites

- .NET SDK `10.0.103` — pinned in `src/global.json`

```bash
# Use the local SDK binary if dotnet is not on PATH
$HOME/.dotnet/dotnet --version   # should print 10.0.103
```

## Quick Start

```bash
cd src
$HOME/.dotnet/dotnet restore AppSimple.sln
$HOME/.dotnet/dotnet build   AppSimple.sln -c Debug
$HOME/.dotnet/dotnet test    AppSimple.sln
```

## DI Wiring (host project startup)

```csharp
services
    .AddAppLogging(opts =>
    {
        opts.EnableConsole = true;
        opts.EnableFile    = true;              // writes to logs/app-.log
        opts.MinimumLevel  = LogEventLevel.Information;
    })
    .AddCoreServices()                          // validators, password hasher, services
    .AddJwtAuthentication(opts =>
    {
        opts.Secret            = "your-32-char-minimum-secret-here!!";
        opts.ExpirationMinutes = 60;
    })
    .AddDataLibServices("Data Source=app.db"); // SQLite connection + repositories
```

Then call once at startup:

```csharp
app.Services.GetRequiredService<DbInitializer>().Initialize();
// optionally seed admin:
app.Services.GetRequiredService<DbInitializer>().SeedAdminUser(hashedAdminPassword);
```

## AppSimple.Core

Full source map: [`docs/core-structure.md`](docs/core-structure.md)

### Domain models (`Models/`)

| Class | Description |
|---|---|
| `BaseEntity` | Abstract base — `Uid` (Guid v7), `CreatedAt`, `UpdatedAt`, `IsSystem` |
| `User` | Extends `BaseEntity` — username, email, password hash, profile fields, role, active flag |

### Enums (`Enums/`)

| Enum | Values |
|---|---|
| `UserRole` | `User = 0`, `Admin = 1` |
| `Permission` | `ViewProfile=10`, `EditProfile=11`, `ViewUsers=20`, `CreateUser=21`, `EditUser=22`, `DeleteUser=23` |

### Auth (`Auth/`)

| Type | Description |
|---|---|
| `IPasswordHasher` | `Hash(plain)` / `Verify(plain, hash)` |
| `BcryptPasswordHasher` | BCrypt work-factor 12 implementation |
| `IJwtTokenService` | `GenerateToken(user)` / `GetUsernameFromToken(tok)` / `IsTokenValid(tok)` |
| `JwtTokenService` | HS256 JWT — embeds sub, unique_name, email, role, jti claims |
| `JwtOptions` | Secret, Issuer, Audience, ExpirationMinutes |

### Common (`Common/`)

| Type | Description |
|---|---|
| `Result<T>` | `Success(value)` / `Failure(error)` / implicit conversion from `T` |
| `Result` | Non-generic variant for void operations |
| `AppException` | Base domain exception |
| `EntityNotFoundException` | 404 — entity not found by id |
| `DuplicateEntityException` | 409 — uniqueness constraint violated |
| `SystemEntityException` | 403 — cannot modify system entity |
| `UnauthorizedException` | 401 — authentication / authorisation failure |

### Services (`Services/`)

| Interface / Impl | Key methods |
|---|---|
| `IUserService` / `UserService` | GetByUid, GetByUsername, GetAll, Create, Update, Delete, ChangePassword |
| `IAuthService` / `AuthService` | LoginAsync → `AuthResult` (JWT on success), ValidateToken |

### Logging (`Logging/`)

| Type | Description |
|---|---|
| `IAppLogger<T>` | Typed logger — Debug, Information, Warning, Error, Fatal, IsEnabled |
| `SerilogAppLogger<T>` | Wraps `Log.ForContext<T>()` |
| `LoggingOptions` | MinimumLevel, EnableConsole, EnableFile, LogDirectory, OutputTemplate, ApplicationName |

### Validators (`Validators/`)

| Validator | Rules |
|---|---|
| `CreateUserRequestValidator` | Username (regex, max 50), email, password (complexity), optional names |
| `UpdateUserRequestValidator` | Optional phone (regex), DOB (past, >1900), bio (max 500) |
| `ChangePasswordRequestValidator` | Complexity, must differ from current, confirmation must match |

### DI extension methods (`Extensions/CoreServiceExtensions`)

```csharp
AddCoreServices()             // validators, IPasswordHasher, IUserService, IAuthService
AddJwtAuthentication(opts)    // IJwtTokenService + configures JwtOptions
AddAppLogging(opts)           // Serilog global logger + IAppLogger<> open-generic
```

## AppSimple.DataLib

Full source map: [`docs/datalib-structure.md`](docs/datalib-structure.md)

### Db (`Db/`)

| Type | Description |
|---|---|
| `IDbConnectionFactory` | Abstracts connection creation |
| `SqliteConnectionFactory` | Opens SQLite connections from `DatabaseOptions.ConnectionString` |
| `DatabaseOptions` | Connection string POCO |
| `DbInitializer` | Creates schema (`Users` table); `SeedAdminUser(hash)` seeds admin row idempotently |
| `DapperConfig` | Registers Guid and DateTime type handlers for SQLite — call once at startup |

### Repositories (`Repositories/`)

| Type | Description |
|---|---|
| `UserRepository` | Implements `IUserRepository` — full CRUD + GetByUsername, GetByEmail, UsernameExists, EmailExists; system users are protected from update/delete |

### DI extension method

```csharp
AddDataLibServices(connectionString)
// Calls DapperConfig.Register(), registers IDbConnectionFactory, DbInitializer, IUserRepository
```

## Database schema

```sql
CREATE TABLE Users (
    Uid          TEXT NOT NULL PRIMARY KEY,
    Username     TEXT NOT NULL UNIQUE COLLATE NOCASE,
    PasswordHash TEXT NOT NULL,
    Email        TEXT NOT NULL UNIQUE COLLATE NOCASE,
    FirstName    TEXT,
    LastName     TEXT,
    PhoneNumber  TEXT,
    DateOfBirth  TEXT,
    Bio          TEXT,
    AvatarUrl    TEXT,
    Role         INTEGER NOT NULL DEFAULT 0,
    IsActive     INTEGER NOT NULL DEFAULT 1,
    IsSystem     INTEGER NOT NULL DEFAULT 0,
    CreatedAt    TEXT NOT NULL,
    UpdatedAt    TEXT NOT NULL
);
```

## Testing

| Project | Tests | Coverage |
|---|---|---|
| `AppSimple.Core.Tests` | 208 | Models, enums, constants, auth, Result, exceptions, validators, services (NSubstitute mocks), logging, DI |
| `AppSimple.DataLib.Tests` | 36 | DbInitializer schema + seed, UserRepository CRUD + case-insensitive lookups + system-user protection |

Run all tests:

```bash
cd src && $HOME/.dotnet/dotnet test AppSimple.sln
```

## Tech Stack

| Package | Version | Purpose |
|---|---|---|
| .NET / C# | 10 / 14 | Runtime and language |
| SQLite | via `Microsoft.Data.Sqlite` 10 | Database engine |
| Dapper | 2.x | Micro-ORM |
| FluentValidation | 11.x | Input validation |
| BCrypt.Net-Next | 4.x | Password hashing |
| System.IdentityModel.Tokens.Jwt | 8.x | JWT generation / validation |
| Serilog + sinks/enrichers | 4.x | Structured logging |
| Microsoft.Extensions.DependencyInjection | 10.x | DI container |
| xUnit | 2.x | Unit testing |
| NSubstitute | latest | Test mocking |

## Security notes

- Passwords are hashed with BCrypt (work-factor 12) — never stored in plain text.
- JWT tokens use HMAC-SHA256 — keep `JwtOptions.Secret` at least 32 characters.
- System entities (`IsSystem = true`) cannot be updated or deleted through any service method.
- Username and email lookups are case-insensitive (SQLite `COLLATE NOCASE`).

## Roadmap

- [ ] `AppSimple.WebApi` — REST API consuming Core services directly
- [ ] `AppSimple.AdminCli` — Admin CLI connecting to Core services via the WebApi HTTP layer
- [ ] `AppSimple.UserCLI` — End-user CLI referencing Core + DataLib directly — **branch: UserCLI**
- [ ] `AppSimple.WebApp` — MVC front-end connecting to Core services via the WebApi HTTP layer
- [ ] `AppSimple.MvvmApp` — WPF desktop application referencing Core + DataLib directly — **branch: MvvmApp** (Windows only)

## Contributing

Follow the conventions in [`.github/copilot-instructions.md`](.github/copilot-instructions.md). All new code must have XML docs and tests.

## License

MIT — see [LICENSE](LICENSE).