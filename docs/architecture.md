# Architecture

AppSimple follows a layered clean architecture where dependencies only point inward — from infrastructure toward the domain.

## Layer overview

AppSimple has two distinct integration tiers for host projects: those that reference Core and DataLib **directly** (local/offline capable), and those that connect through `AppSimple.WebApi` over **HTTP**.

```
┌──────────────────────────────────────────────────────────────────┐
│               HTTP Clients (connect via WebApi)                  │
│  WebApp · AdminCli                                               │
│  - REST calls to WebApi endpoints                                │
│  - No direct reference to Core or DataLib                        │
└───────────────────────────────┬──────────────────────────────────┘
                                │ HTTP / REST
┌───────────────────────────────▼──────────────────────────────────┐
│                       AppSimple.WebApi                           │
│  ASP.NET Core Web API — exposes Core services over HTTP          │
└───────────────────────────────┬──────────────────────────────────┘
                                │
                                │ (same direct-reference tier below)
                                │
┌───────────────────────────────▼──────────────────────────────────┐
│          Direct-Reference Clients (no HTTP layer)                │
│  WebApi · UserCLI · MvvmApp                                      │
│  - Project-reference AppSimple.Core + AppSimple.DataLib          │
│  - Full local / offline capability                               │
└───────────────────────────────┬──────────────────────────────────┘
                                │ depends on
┌───────────────────────────────▼──────────────────────────────────┐
│                       AppSimple.Core                             │
│  Domain Layer — owns business rules                              │
│                                                                  │
│  Models/          BaseEntity, User                               │
│  Enums/           UserRole, Permission                           │
│  Constants/       AppConstants                                   │
│  Interfaces/      IRepository<T>, IUserRepository                │
│  Services/        IUserService, IAuthService + impls             │
│  Auth/            IPasswordHasher, IJwtTokenService              │
│  Common/          Result<T>, typed exceptions                    │
│  Logging/         IAppLogger<T> abstraction, LogPath             │
│  Validators/      FluentValidation validators                    │
└───────────────────────────────┬──────────────────────────────────┘
                                │ implements interfaces from
┌───────────────────────────────▼──────────────────────────────────┐
│                     AppSimple.DataLib                            │
│  Infrastructure Layer — SQLite + Dapper                          │
│                                                                  │
│  Db/              Connection factory, DbInitializer, DatabasePath│
│  Repositories/    UserRepository : IUserRepository               │
└──────────────────────────────────────────────────────────────────┘
```

## Project catalogue

| Project | Type | Status | Connects via | Purpose |
|---|---|---|---|---|
| `AppSimple.Core` | Class library | ✅ Built | — | Domain models, services, auth, logging, validators |
| `AppSimple.DataLib` | Class library | ✅ Built | Core (direct) | SQLite + Dapper data access |
| `AppSimple.WebApi` | ASP.NET Core Web API | ✅ Built | Core + DataLib (direct) | REST API host — exposes Core services over HTTP |
| `AppSimple.WebApp` | ASP.NET Core MVC | ✅ Built | WebApi (HTTP) | Browser-based user GUI — dark Catppuccin theme |
| `AppSimple.UserCLI` | Console application | ✅ Built | Core + DataLib (direct) | End-user CLI — local/offline, no WebApi required |
| `AppSimple.MvvmApp` | Avalonia UI application | ✅ Built | Core + DataLib (direct) | Cross-platform desktop app (Windows/macOS/Linux) |
| `AppSimple.AdminCli` | Console application | ✅ Built | WebApi (HTTP) | Admin tooling — user management, seeding, smoke tests |

## Dependency rules

| From | May depend on | May NOT depend on |
|---|---|---|
| WebApi | Core, DataLib | AdminCli, WebApp, UserCLI, MvvmApp |
| AdminCli | WebApi (HTTP) | Core, DataLib directly |
| WebApp | WebApi (HTTP) | Core, DataLib directly |
| UserCLI | Core, DataLib | WebApi |
| MvvmApp | Core, DataLib | WebApi |
| DataLib | Core | All host projects |
| Core | (no project refs) | All other projects |

Core is intentionally free of infrastructure concerns — it knows nothing about SQLite, HTTP, or file I/O.

## DI composition

**Direct-reference projects** (WebApi, UserCLI, MvvmApp) wire Core and DataLib themselves:

```csharp
var connectionString = DatabasePath.Resolve(config["Database:ConnectionString"]);
var logDir           = LogPath.Resolve(config["AppLogging:LogDirectory"]);

services
    .AddAppLogging(opts =>
    {
        opts.EnableFile   = config.GetValue("AppLogging:EnableFile", true);
        opts.LogDirectory = logDir;
    })                                   // Core — Serilog + IAppLogger<>
    .AddCoreServices()                   // Core — validators, auth services, user services
    .AddJwtAuthentication(opts => { ... }) // Core — IJwtTokenService + JwtOptions
    .AddDataLibServices(connectionString); // DataLib — DB connection + repositories
```

**HTTP clients** (WebApp, AdminCli) have no Core/DataLib registrations. WebApp uses cookie auth and a typed `HttpClient`:

```csharp
// WebApp only
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts => { opts.LoginPath = "/login"; ... });
builder.Services.AddHttpClient<IApiClient, ApiClient>(c =>
    c.BaseAddress = new Uri(config["WebApi:BaseUrl"]!));
```

## Authentication flow

```
Client
  │  POST /auth/login { username, password }
  │
  ▼
AuthService.LoginAsync(username, password)
  │  1. IUserRepository.GetByUsernameAsync(username)
  │  2. Check IsActive
  │  3. IPasswordHasher.Verify(password, user.PasswordHash)
  │  4. IJwtTokenService.GenerateToken(user)
  │
  ▼
AuthResult.Success(jwtToken)   ──►  client stores token
```

Subsequent requests carry the `Authorization: Bearer <token>` header. The host project validates it via `IAuthService.ValidateToken(token)` (delegates to `IJwtTokenService.GetUsernameFromToken`).

## Data access pattern

```
IUserService (Core interface)
    └── UserService (Core impl)
            └── IUserRepository (Core interface)
                    └── UserRepository (DataLib impl, Dapper + SQLite)
```

All repositories are scoped to the DI request lifetime. The connection factory is a singleton that creates a new open connection per call.

## Error handling strategy

| Scenario | Exception | Suggested HTTP status |
|---|---|---|
| Entity not found | `EntityNotFoundException` | 404 Not Found |
| Duplicate username / email | `DuplicateEntityException` | 409 Conflict |
| Modify system entity | `SystemEntityException` | 403 Forbidden |
| Bad credentials / wrong password | `UnauthorizedException` | 401 Unauthorized |
| Validation failure | `ValidationException` (FluentValidation) | 422 Unprocessable Entity |

Host projects catch these in a global exception handler / middleware and map them to structured API responses.

## Testing strategy

- **Unit tests** (`Core.Tests`, 208 tests): all logic tested in isolation with NSubstitute mocks. No I/O.
- **Integration tests** (`DataLib.Tests`, 36 tests): real SQLite `:memory:` database — schema creation, seeding, and full repository CRUD tested end-to-end.
- **Future**: WebApi tests via `WebApplicationFactory`; WebApp/AdminCli tests via mocked `HttpClient` or a test WebApi instance.
