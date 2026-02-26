# AppSimple

AppSimple is a .NET 10 starter solution demonstrating clean architecture, separation of concerns, and testability. It provides a fully-implemented core business layer and SQLite data access layer, ready to be consumed by future host projects.

## Table of Contents

- [Project Structure](#project-structure)
- [Built Projects](#built-projects)
- [Future Projects](#future-projects)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Shared Paths (Database, Logs & Config)](#shared-paths-database-logs--config)
- [DI Wiring (host project startup)](#di-wiring-host-project-startup)
- [AppSimple.Core](#appsimplecore)
- [AppSimple.DataLib](#appsimpledatalib)
- [AppSimple.UserCLI](#appsimpleusercli)
- [AppSimple.MvvmApp](#appsimplemvvmapp)
- [AppSimple.WebApi](#appsimplewebapi)
- [AppSimple.WebApp](#appsimplewebapp)
- [AppSimple.AdminCli](#appsimpleadmincli)
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
│   ├── AppSimple.UserCLI/         # End-user console app (direct Core + DataLib)
│   ├── AppSimple.MvvmApp/         # Cross-platform Avalonia UI desktop app
│   ├── AppSimple.WebApi/          # ASP.NET Core 10 REST API with JWT auth
│   ├── AppSimple.WebApp/          # ASP.NET Core 10 MVC web front-end (connects via WebApi HTTP)
│   ├── AppSimple.AdminCli/        # Admin console CLI (connects via WebApi HTTP)
│   └── AppSimple.sln
├── .github/
│   └── copilot-instructions.md   # Code conventions for AI-assisted development
└── README.md
```

## Built Projects

| Project | Connects via | Purpose |
|---|---|---|
| `AppSimple.Core` | — | Domain models, services, auth, logging, validators |
| `AppSimple.DataLib` | Core (direct) | SQLite + Dapper data access |
| `AppSimple.UserCLI` | Core + DataLib (direct) | End-user console app — role-aware menus, offline-capable |
| `AppSimple.MvvmApp` | Core + DataLib (direct) | Cross-platform Avalonia UI desktop app (Windows/macOS/Linux) |
| `AppSimple.WebApi` | Core + DataLib (direct) | ASP.NET Core 10 REST API — JWT auth, role-based access |
| `AppSimple.WebApp` | WebApi (HTTP) | ASP.NET Core 10 MVC web front-end — dark theme, cookie session |
| `AppSimple.AdminCli` | WebApi (HTTP) | Admin console CLI — user management, seeding, smoke tests |

## Future Projects

No future projects planned at this time.

## Architecture

See [`docs/architecture.md`](docs/architecture.md) for a detailed breakdown.

```text
                  ┌────────────────────────────────────────────────────────────────┐
                  │              Projects that connect via HTTP                    │
                  │          WebApp (HTTP) · AdminCli (HTTP)                       │
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
```cd $HOME/

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

## Shared Paths (Database, Logs & Config)

All runtime projects share the same SQLite database, log directory, and app config through static path helpers:

| Helper | Namespace | Used for |
|---|---|---|
| `DatabasePath.Resolve(configValue?)` | `AppSimple.DataLib.Db` | SQLite connection string |
| `LogPath.Resolve(configValue?)` | `AppSimple.Core.Logging` | Log file directory |
| `AppConfigPath.Resolve(configValue?)` | `AppSimple.Core.Config` | `config.json` (theme selection) |

**Resolution order (same for all):**
1. Non-empty value from `appsettings.json`
2. Environment variable (`APPSIMPLE_DB` / `APPSIMPLE_LOGS` / `APPSIMPLE_CONFIG`)
3. **OS default** — `~/.local/share/AppSimple/` (Linux/macOS) or `%LOCALAPPDATA%\AppSimple\` (Windows)

Setting the config values to `""` opts in to the shared default — all runtime projects ship this way.

---

## DI Wiring (host project startup)

```csharp
var connectionString = DatabasePath.Resolve(config["Database:ConnectionString"]);
var logDir           = LogPath.Resolve(config["AppLogging:LogDirectory"]);

services
    .AddAppLogging(opts =>
    {
        opts.EnableFile   = config.GetValue("AppLogging:EnableFile", true);
        opts.LogDirectory = logDir;
    })
    .AddCoreServices()                          // validators, password hasher, services
    .AddJwtAuthentication(opts =>
    {
        opts.Secret            = "your-32-char-minimum-secret-here!!";
        opts.ExpirationMinutes = 60;
    })
    .AddDataLibServices(connectionString);      // SQLite connection + repositories
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
| `Note` | Extends `BaseEntity` — title, content, owning `UserUid`, associated `Tags` list |
| `Tag` | Extends `BaseEntity` — name, description, hex color, owning `UserUid` |
| `Contact` | Extends `BaseEntity` — owner `OwnerUserUid`, name, string Tags (JSON), child collections |
| `EmailAddress` | Extends `BaseEntity` — contact child; email, type, isPrimary, string Tags |
| `PhoneNumber` | Extends `BaseEntity` — contact child; number, type, isPrimary, string Tags |
| `ContactAddress` | Extends `BaseEntity` — contact child; street/city/state/postal/country, type, isPrimary |

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
| `INoteService` / `NoteService` | GetByUid, GetAll (admin), GetByUserUid, Create, Update, Delete, AddTag, RemoveTag |
| `ITagService` / `TagService` | GetByUid, GetAll (admin), GetByUserUid, Create, Update, Delete |
| `IContactService` / `ContactService` | GetByUid, GetAll (admin), GetByOwnerUid, Create, Update, Delete; full CRUD for child EmailAddresses, PhoneNumbers, Addresses |

### Logging (`Logging/`)

| Type | Description |
|---|---|
| `IAppLogger<T>` | Typed logger — Debug, Information, Warning, Error, Fatal, IsEnabled |
| `SerilogAppLogger<T>` | Wraps `Log.ForContext<T>()` |
| `LoggingOptions` | MinimumLevel, EnableConsole, EnableFile, LogDirectory, OutputTemplate, ApplicationName |
| `LogPath` | Static helper — resolves the shared log directory (config → env var → OS default) |

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
| `DatabasePath` | Static helper — resolves the shared SQLite connection string (config → env var → OS default) |
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

## AppSimple.WebApi

Full source map: [`docs/webapi-structure.md`](docs/webapi-structure.md)

ASP.NET Core 10 REST API. References Core + DataLib directly. Exposes JWT-secured endpoints consumed by `AppSimple.WebApp` and future HTTP clients.

```bash
cd $HOME/Projects/AppSimple/src/AppSimple.WebApi
$HOME/.dotnet/dotnet run
# Listens on http://localhost:5157
```

**Default credentials:** `admin` / `Admin123!`

### Endpoints (summary)

| Group | Auth | Examples |
|---|---|---|
| Public | None | `GET /api` · `GET /api/health` |
| Auth | None / Bearer | `POST /api/auth/login` → JWT · `GET /api/auth/validate` |
| Protected | Bearer | `GET /api/protected/me` · `PUT /api/protected/me` · `POST /api/protected/me/change-password` |
| Admin | Bearer + Admin role | `GET/POST /api/admin/users` · `PUT/DELETE /api/admin/users/{uid}` |

---

## AppSimple.UserCLI

Full source map: [`docs/usercli-structure.md`](docs/usercli-structure.md)

End-user console application. No HTTP layer — references Core + DataLib directly.

```bash
cd $HOME/Projects/AppSimple/src/AppSimple.UserCLI
$HOME/.dotnet/dotnet run
```

**Default credentials:** `admin` / `Admin123!`

### Features

- Login screen with masked password input
- Role-aware menu system:
  - **All users:** Profile menu — view details, edit profile, change password
  - **Admin role:** User management — list, create, edit, delete users
- System users (`IsSystem = true`) are protected from modification
- Pure `Console`/`ConsoleColor` UI — no third-party TUI libraries

---

## AppSimple.MvvmApp

Full source map: [`docs/mvvmapp-structure.md`](docs/mvvmapp-structure.md)

Cross-platform Avalonia UI desktop application (Windows · macOS · Linux).
References Core + DataLib directly — no HTTP layer required.

```bash
cd $HOME//Projects/AppSimple/src/AppSimple.MvvmApp
$HOME/.dotnet/dotnet run
```

**Default credentials:** `admin` / `Admin123!`

### Features

- Public landing page on startup
- Top navigation bar with login form (username + masked password)
- Left sidebar nav — items change based on logged-in role/permissions
- **Profile page** — view read-only info, edit profile fields, change password
- **Users page** (Admin only) — DataGrid + inline create/edit form panel
- Dark theme — Fluent Design + runtime-switchable theme (5 themes, persisted to `config.json`)
- Full MVVM — `[ObservableProperty]` + `[RelayCommand]` (CommunityToolkit.Mvvm), no code-behind password handling

---

## AppSimple.WebApp

Full source map: [`docs/webapp-structure.md`](docs/webapp-structure.md)

ASP.NET Core 10 MVC web front-end. Connects to `AppSimple.WebApi` over HTTP — no direct
reference to Core or DataLib. Stores the JWT token in a cookie claim and forwards it on
every API call.

```bash
# Start WebApi first, then:
cd $HOME/Projects/AppSimple/src/AppSimple.WebApi
$HOME/.dotnet/dotnet run
# In another terminal:
cd $HOME/Projects/AppSimple/src/AppSimple.WebApp
$HOME/.dotnet/dotnet run
```

**Default credentials:** `admin` / `Admin123!`

### Features

- Public landing page
- Login form → JWT from WebApi stored as cookie claim
- **Profile page** — view, edit profile fields, change password
- **Users page** (Admin only) — user table + create/edit/delete
- Runtime-switchable themes (5 themes, persisted to shared `config.json`) — matches MvvmApp palette
- No Bootstrap or external CSS — pure CSS variables inline in layout

---

## AppSimple.AdminCli

Full source map: [`docs/admincli-structure.md`](docs/admincli-structure.md)

Admin-only console CLI. Connects to `AppSimple.WebApi` over HTTP — no direct reference to
Core or DataLib. Rejects logins from non-Admin users before establishing a session.

```bash
# Start WebApi first, then:
cd $HOME/Projects/AppSimple/src/AppSimple.AdminCli
dotnet run
```

**Requires an Admin account.** Default: `admin` / `Admin123!`

### Features

- Login with Admin-role gate (non-Admin logins rejected at menu level)
- **User Management** — list, create, view details, edit, delete, change role
- **System & Health** — live health check, smoke test (3 checks), seed test users
- Rolling daily log file via Serilog (same shared log directory)
- No direct Core/DataLib dependency — HTTP only

---

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

CREATE TABLE Tags (
    Uid         TEXT NOT NULL PRIMARY KEY,
    UserUid     TEXT NOT NULL REFERENCES Users(Uid) ON DELETE CASCADE,
    Name        TEXT NOT NULL COLLATE NOCASE,
    Description TEXT,
    Color       TEXT NOT NULL DEFAULT '#CCCCCC',
    IsSystem    INTEGER NOT NULL DEFAULT 0,
    CreatedAt   TEXT NOT NULL,
    UpdatedAt   TEXT NOT NULL
);

CREATE TABLE Notes (
    Uid       TEXT NOT NULL PRIMARY KEY,
    UserUid   TEXT NOT NULL REFERENCES Users(Uid) ON DELETE CASCADE,
    Title     TEXT NOT NULL DEFAULT '',
    Content   TEXT NOT NULL,
    IsSystem  INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE TABLE NoteTags (
    NoteUid TEXT NOT NULL REFERENCES Notes(Uid) ON DELETE CASCADE,
    TagUid  TEXT NOT NULL REFERENCES Tags(Uid)  ON DELETE CASCADE,
    PRIMARY KEY (NoteUid, TagUid)
);

CREATE TABLE Contacts (
    Uid          TEXT NOT NULL PRIMARY KEY,
    OwnerUserUid TEXT NOT NULL REFERENCES Users(Uid) ON DELETE CASCADE,
    Name         TEXT NOT NULL COLLATE NOCASE,
    Tags         TEXT NOT NULL DEFAULT '[]',
    IsSystem     INTEGER NOT NULL DEFAULT 0,
    CreatedAt    TEXT NOT NULL,
    UpdatedAt    TEXT NOT NULL
);

CREATE TABLE ContactEmailAddresses (
    Uid        TEXT NOT NULL PRIMARY KEY,
    ContactUid TEXT NOT NULL REFERENCES Contacts(Uid) ON DELETE CASCADE,
    Email      TEXT NOT NULL COLLATE NOCASE,
    IsPrimary  INTEGER NOT NULL DEFAULT 0,
    Tags       TEXT NOT NULL DEFAULT '[]',
    Type       INTEGER NOT NULL DEFAULT 0,
    IsSystem   INTEGER NOT NULL DEFAULT 0,
    CreatedAt  TEXT NOT NULL,
    UpdatedAt  TEXT NOT NULL
);

CREATE TABLE ContactPhoneNumbers (
    Uid        TEXT NOT NULL PRIMARY KEY,
    ContactUid TEXT NOT NULL REFERENCES Contacts(Uid) ON DELETE CASCADE,
    Number     TEXT NOT NULL,
    IsPrimary  INTEGER NOT NULL DEFAULT 0,
    Tags       TEXT NOT NULL DEFAULT '[]',
    Type       INTEGER NOT NULL DEFAULT 0,
    IsSystem   INTEGER NOT NULL DEFAULT 0,
    CreatedAt  TEXT NOT NULL,
    UpdatedAt  TEXT NOT NULL
);

CREATE TABLE ContactAddresses (
    Uid        TEXT NOT NULL PRIMARY KEY,
    ContactUid TEXT NOT NULL REFERENCES Contacts(Uid) ON DELETE CASCADE,
    Street     TEXT NOT NULL,
    City       TEXT NOT NULL,
    State      TEXT NOT NULL DEFAULT '',
    PostalCode TEXT NOT NULL DEFAULT '',
    Country    TEXT NOT NULL,
    IsPrimary  INTEGER NOT NULL DEFAULT 0,
    Tags       TEXT NOT NULL DEFAULT '[]',
    Type       INTEGER NOT NULL DEFAULT 0,
    IsSystem   INTEGER NOT NULL DEFAULT 0,
    CreatedAt  TEXT NOT NULL,
    UpdatedAt  TEXT NOT NULL
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

- [x] `AppSimple.Core` — domain models, services, auth, logging, validators
- [x] `AppSimple.DataLib` — SQLite + Dapper data access
- [x] `AppSimple.Core.Tests` — 208 unit tests
- [x] `AppSimple.DataLib.Tests` — 36 integration tests
- [x] `AppSimple.UserCLI` — end-user console app (direct Core + DataLib) — **branch: UserCLI**
- [x] `AppSimple.MvvmApp` — Avalonia UI cross-platform desktop app — **branch: MvvmApp**
- [x] `AppSimple.WebApi` — ASP.NET Core 10 REST API with JWT auth — **branch: WebApi**
- [x] `AppSimple.WebApp` — ASP.NET Core 10 MVC web front-end (connects via WebApi HTTP)
- [x] Note-taking feature — `Note` + `Tag` models, services, repositories, DB schema (Notes, Tags, NoteTags)
- [x] Contact manager feature — `Contact` + child models, services, repositories, DB schema (Contacts, ContactEmailAddresses, ContactPhoneNumbers, ContactAddresses)
- [ ] `AppSimple.AdminCli` — admin console app connecting via WebApi HTTP
- [ ] Note-taking UI in UserCLI, MvvmApp, WebApi, and WebApp
- [ ] Test projects for WebApi, AdminCli, WebApp, MvvmApp

## Contributing

Follow the conventions in [`.github/copilot-instructions.md`](.github/copilot-instructions.md). All new code must have XML docs and tests.

## License

MIT — see [LICENSE](LICENSE).
