# IAppLogger — Developer Guide

This document describes the AppSimple structured logging system: its design, how to use it, how to configure it, and how to extend it.

---

## Overview

AppSimple uses a **typed logger abstraction** (`IAppLogger<T>`) backed by [Serilog](https://serilog.net/). Every class that needs logging receives its own context-enriched logger instance via dependency injection.

```
IAppLogger<T>                  ← your code depends on this
  └─ SerilogAppLogger<T>       ← Core implementation (hidden behind interface)
       └─ Serilog.Log           ← global static logger; configured once at startup
```

Key properties:
- **Typed** — the generic `T` sets Serilog's `SourceContext` automatically (namespace + class name).
- **DI-native** — registered as an open-generic transient; inject it like any other service.
- **Structured** — uses Serilog message templates; log values are properties, not string-formatted text.
- **Testable** — any test can provide a mock `IAppLogger<T>` without touching Serilog at all.

---

## Source Files

| File | Purpose |
|---|---|
| `Core/Logging/IAppLogger.cs` | Public interface — the only type your code should reference |
| `Core/Logging/LoggingOptions.cs` | Configuration model passed to `AddAppLogging()` |
| `Core/Logging/LogPath.cs` | Resolves the shared log directory (config → env var → OS default) |
| `Core/Logging/Impl/SerilogAppLogger.cs` | Serilog-backed implementation of `IAppLogger<T>` |
| `Core/Logging/Impl/SerilogLoggerFactory.cs` | Builds and configures the global `Serilog.Core.Logger` from `LoggingOptions` |
| `Core/Extensions/CoreServiceExtensions.cs` | `AddAppLogging()` DI extension method |

---

## Log Levels

| Method | Serilog Level | When to use |
|---|---|---|
| `Debug(...)` | `Debug` | Diagnostic details useful during development — not enabled in production by default |
| `Information(...)` | `Information` | Normal operational events (startup, successful login, entity created) |
| `Warning(...)` | `Warning` | Unexpected but recoverable situations (token expired, empty result set, retry) |
| `Error(...)` | `Error` | Failures that affect an operation but don't crash the process |
| `Fatal(...)` | `Fatal` | Unrecoverable failures; process should exit |

---

## Using IAppLogger<T>

### 1 — Inject into any class

```csharp
using AppSimple.Core.Logging;

public sealed class UserService : IUserService
{
    private readonly IAppLogger<UserService> _logger;

    public UserService(IAppLogger<UserService> logger, ...)
    {
        _logger = logger;
    }
}
```

The DI container resolves `SerilogAppLogger<UserService>` automatically — no extra registration required per class.

### 2 — Write log events

Always use **message templates** (curly-brace placeholders), not string interpolation. Serilog captures the raw values as structured properties.

```csharp
// ✅ Correct — structured properties captured
_logger.Information("User '{Username}' logged in with role {Role}", user.Username, user.Role);

// ❌ Wrong — string interpolation loses structure
_logger.Information($"User '{user.Username}' logged in");
```

### 3 — Log exceptions

Pass the exception as the **first argument** to the overload that accepts one:

```csharp
try
{
    await _repo.SaveAsync(entity);
}
catch (Exception ex)
{
    _logger.Error(ex, "Failed to save entity {Uid}", entity.Uid);
    throw;
}
```

### 4 — Guard expensive calls with IsEnabled

Skip building expensive log state when the level is disabled:

```csharp
if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
{
    var dump = JsonSerializer.Serialize(request);
    _logger.Debug("Incoming request payload: {Payload}", dump);
}
```

---

## Recommended Log Events by Layer

### Services (Core)

| Event | Level |
|---|---|
| Method entered with key input | `Debug` |
| Entity created / updated / deleted | `Information` |
| Entity not found | `Warning` |
| Duplicate detected | `Warning` |
| Unexpected exception | `Error` |

```csharp
public async Task<User> CreateAsync(string username, string email, string plainPassword)
{
    _logger.Debug("CreateAsync called for username '{Username}'", username);
    // ...
    _logger.Information("User '{Username}' created with UID {Uid}", user.Username, user.Uid);
    return user;
}
```

### API Controllers (WebApi)

| Event | Level |
|---|---|
| Request received (key parameter) | `Debug` |
| Successful response | `Information` |
| Validation failure / not found | `Warning` |
| Unexpected error | `Error` |

```csharp
[HttpGet("{uid:guid}")]
public async Task<IActionResult> GetUser(Guid uid)
{
    _logger.Debug("GetUser called for UID {Uid}", uid);
    var user = await _users.GetByUidAsync(uid);
    if (user is null)
    {
        _logger.Warning("User {Uid} not found", uid);
        return NotFound();
    }
    return Ok(UserDto.From(user));
}
```

### Console Menus (UserCLI / AdminCli)

| Event | Level |
|---|---|
| Menu option selected | `Debug` |
| Successful action (create/edit/delete) | `Information` |
| User cancelled | `Debug` |
| API/DB call failed | `Error` |

```csharp
_logger.Debug("User selected 'Delete User' option");
// ...
_logger.Information("Admin '{Admin}' deleted user '{Username}'", _session.Username, target.Username);
```

### ViewModels (MvvmApp)

| Event | Level |
|---|---|
| Login attempt / outcome | `Information` / `Warning` |
| Save/update outcome | `Information` / `Error` |
| Command failed | `Error` |

---

## Setup & Configuration

### Registration

Call `AddAppLogging()` once during startup, **before** any services that use `IAppLogger<T>` are resolved:

```csharp
// Program.cs or ServiceExtensions.cs
services.AddAppLogging(opts =>
{
    opts.EnableConsole = true;
    opts.EnableFile    = true;
    opts.MinimumLevel  = LogEventLevel.Information;
    opts.LogDirectory  = LogPath.Resolve(config["AppLogging:LogDirectory"]);
});

services.AddCoreServices();
```

`AddCoreServices()` automatically calls `AddAppLogging()` with defaults if you don't call it yourself, but calling it explicitly lets you override options.

### LoggingOptions Reference

| Property | Type | Default | Description |
|---|---|---|---|
| `MinimumLevel` | `LogEventLevel` | `Information` | Minimum level written to any sink |
| `EnableConsole` | `bool` | `true` | Write to stdout |
| `EnableFile` | `bool` | `false` | Write to rolling log files |
| `LogDirectory` | `string` | `"logs"` | Directory for file sink |
| `OutputTemplate` | `string` | (see below) | Serilog output template |
| `ApplicationName` | `string` | `"AppSimple"` | Enriched `Application` property on every event |
| `RollingInterval` | `RollingInterval` | `Day` | How often log files roll |
| `RetainedFileCountLimit` | `int` | `7` | Number of old files to keep |

Default output template:
```
[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}
```

### Per-Project Conventions

| Project | Console | File | Reason |
|---|---|---|---|
| **UserCLI** | ❌ | ✅ | Console output would corrupt the TUI |
| **AdminCli** | ✅ | ✅ | Operator-facing terminal |
| **WebApi** | ✅ | ✅ | Server process |
| **WebApp** | ✅ | ✅ | Server process |
| **MvvmApp** | ❌ | ✅ | Desktop GUI; no visible console window |

### Log File Location

```
LogPath.Resolve(config["AppLogging:LogDirectory"])
```

Resolution priority:
1. `AppLogging:LogDirectory` value in `appsettings.json`
2. `APPSIMPLE_LOGS` environment variable
3. OS default: `~/.local/share/AppSimple/logs/` (Linux/macOS) or `%LOCALAPPDATA%\AppSimple\logs\` (Windows)

Log files are named `app-YYYYMMDD.log` and rotate daily.

---

## Configuration via appsettings.json

```json
{
  "AppLogging": {
    "EnableFile": true,
    "LogDirectory": "/var/log/appsimple"
  }
}
```

Consumed in project startup:

```csharp
services.AddAppLogging(opts =>
{
    opts.EnableFile   = config.GetValue<bool>(AppConstants.ConfigLoggingEnableFile, true);
    opts.LogDirectory = LogPath.Resolve(config[AppConstants.ConfigLoggingDirectory]);
});
```

Config key constants are defined in `AppConstants`:

```csharp
AppConstants.ConfigLoggingEnableFile   // "AppLogging:EnableFile"
AppConstants.ConfigLoggingDirectory    // "AppLogging:LogDirectory"
```

---

## Extending IAppLogger<T>

### Option A — Add a new overload to the interface

Add the method to `IAppLogger<T>` and implement it in `SerilogAppLogger<T>`:

```csharp
// IAppLogger.cs
/// <summary>Writes a verbose trace-level message (below Debug).</summary>
void Verbose(string messageTemplate, params object?[] args);

// SerilogAppLogger.cs
public void Verbose(string messageTemplate, params object?[] args)
    => _logger.Verbose(messageTemplate, args);
```

### Option B — Add a custom enricher in SerilogLoggerFactory

Open `SerilogLoggerFactory.Create()` and add an enricher to the `LoggerConfiguration`:

```csharp
config.Enrich.WithProperty("Environment", options.EnvironmentName);
// or a custom enricher:
config.Enrich.With<CorrelationIdEnricher>();
```

### Option C — Add a new sink

Add another `WriteTo.*` call inside `SerilogLoggerFactory.Create()`:

```csharp
if (options.EnableSeq)
{
    config.WriteTo.Seq(options.SeqServerUrl);
}
```

And add `EnableSeq` / `SeqServerUrl` properties to `LoggingOptions`.

### Option D — Write a test double

For unit tests, implement the interface with an in-memory store:

```csharp
public sealed class FakeLogger<T> : IAppLogger<T>
{
    public List<string> Messages { get; } = [];

    public void Information(string template, params object?[] args)
        => Messages.Add(string.Format(template.Replace("{", "").Replace("}", ""), args));

    public void Debug(string t, params object?[] args) { }
    public void Warning(string t, params object?[] args) { }
    public void Error(string t, params object?[] args) { }
    public void Error(Exception ex, string t, params object?[] args) { }
    public void Fatal(string t, params object?[] args) { }
    public void Fatal(Exception ex, string t, params object?[] args) { }
    public bool IsEnabled(Serilog.Events.LogEventLevel level) => false;
}
```

Or use NSubstitute:

```csharp
var logger = Substitute.For<IAppLogger<MyService>>();
var sut = new MyService(logger);
sut.DoSomething();
logger.Received(1).Information(Arg.Any<string>(), Arg.Any<object[]>());
```

---

## Structured Property Naming Conventions

Follow these naming conventions for log properties to keep log queries consistent:

| Property | Example | Description |
|---|---|---|
| `{Username}` | `"admin"` | Login name |
| `{Uid}` | `"a1b2-..."` | Entity GUID |
| `{Role}` | `"Admin"` | User role |
| `{Count}` | `42` | Result count |
| `{Duration}` | `123` | Elapsed milliseconds |
| `{Status}` | `404` | HTTP status code |
| `{Url}` | `"/api/admin/users"` | Request URL |
| `{BaseAddress}` | `"http://..."` | HTTP client base URL |

---

## Example: Full Service with Logging

```csharp
using AppSimple.Core.Logging;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IAppLogger<UserService> _logger;

    public UserService(IUserRepository repo, IPasswordHasher hasher, IAppLogger<UserService> logger)
    {
        _repo   = repo;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<User> CreateAsync(string username, string email, string plainPassword)
    {
        _logger.Debug("CreateAsync called for '{Username}'", username);

        var existing = await _repo.GetByUsernameAsync(username);
        if (existing is not null)
        {
            _logger.Warning("Duplicate username '{Username}' on create attempt", username);
            throw new DuplicateEntityException($"Username '{username}' is already taken.");
        }

        var user = new User { Username = username, Email = email, /* ... */ };
        user.PasswordHash = _hasher.Hash(plainPassword);
        await _repo.AddAsync(user);

        _logger.Information("User '{Username}' (UID {Uid}) created", user.Username, user.Uid);
        return user;
    }
}
```
