# DI Setup Guide

Complete wiring example for a **direct-reference** host project — one that references `AppSimple.Core` and `AppSimple.DataLib` directly (WebApi, UserCLI, MvvmApp).

> **HTTP clients** (AdminCli, WebApp) connect via the WebApi REST layer and do not register Core or DataLib services. They only need an `HttpClient` pointed at the WebApi base address.

## Real-world examples

| Project | DI registration file | Notes |
|---|---|---|
| `AppSimple.UserCLI` | `Program.cs` | File-only logging (`EnableConsole = false`) to avoid mixing log output with interactive UI |
| `AppSimple.MvvmApp` | `App.axaml.cs` via `AddMvvmAppServices()` | Also registers ViewModels and `MainWindow` as singletons/transients |

## Package references

Add these to your host project's `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\AppSimple.Core\AppSimple.Core.csproj" />
  <ProjectReference Include="..\AppSimple.DataLib\AppSimple.DataLib.csproj" />
</ItemGroup>
```

## Registration order

**The order matters.** `AddAppLogging` configures the global Serilog `Log.Logger` — this must happen before any logger is used.

```csharp
services
    // 1. Configure Serilog global logger + register IAppLogger<> open-generic
    .AddAppLogging(opts =>
    {
        opts.ApplicationName  = "MyApp";
        opts.MinimumLevel     = LogEventLevel.Information;
        opts.EnableConsole    = true;
        opts.EnableFile       = true;
        opts.LogDirectory     = "logs";
        opts.RollingInterval  = RollingInterval.Day;
    })

    // 2. Register validators, IPasswordHasher, IUserService, IAuthService
    .AddCoreServices()

    // 3. Register IJwtTokenService and configure JwtOptions
    .AddJwtAuthentication(opts =>
    {
        // Load from configuration in production:
        //   opts.Secret = configuration["Jwt:Secret"]!;
        opts.Secret            = "your-secret-key-must-be-32-chars!!";
        opts.Issuer            = "MyApp";
        opts.Audience          = "MyApp";
        opts.ExpirationMinutes = 60;
    })

    // 4. Register database connection factory, DbInitializer, IUserRepository
    .AddDataLibServices(connectionString: "Data Source=app.db");
```

## Startup / bootstrap

After building the service container, initialise the database:

```csharp
var initializer = app.Services.GetRequiredService<DbInitializer>();

// Create tables (safe to call every startup — uses CREATE TABLE IF NOT EXISTS)
initializer.Initialize();

// Seed the default admin user (only inserts if the user does not already exist)
var hasher = app.Services.GetRequiredService<IPasswordHasher>();
initializer.SeedAdminUser(hasher.Hash("ChangeMe123!"));
```

## Using services

### IUserService

```csharp
public class UserController
{
    private readonly IUserService _users;
    private readonly IAppLogger<UserController> _logger;

    public UserController(IUserService users, IAppLogger<UserController> logger)
    {
        _users  = users;
        _logger = logger;
    }

    public async Task<User> Create(string username, string email, string password)
    {
        try
        {
            var user = await _users.CreateAsync(username, email, password);
            _logger.Information("Created user {Username}", username);
            return user;
        }
        catch (DuplicateEntityException ex)
        {
            _logger.Warning("Duplicate {Field}: {Value}", ex.Field, ex.Value);
            throw;
        }
    }
}
```

### IAuthService

```csharp
public class AuthController
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    public async Task<string?> Login(string username, string password)
    {
        var result = await _auth.LoginAsync(username, password);
        return result.Succeeded ? result.Token : null;
    }

    public string? GetUsername(string token) => _auth.ValidateToken(token);
}
```

### IAppLogger\<T\>

```csharp
public class MyService
{
    private readonly IAppLogger<MyService> _log;

    public MyService(IAppLogger<MyService> log) => _log = log;

    public void DoWork()
    {
        _log.Information("Starting work at {Time}", DateTime.UtcNow);

        try { /* ... */ }
        catch (Exception ex)
        {
            _log.Error(ex, "Work failed: {Message}", ex.Message);
        }
    }
}
```

## Configuration via appsettings.json (ASP.NET Core example)

```json
{
  "Jwt": {
    "Secret": "your-secret-key-must-be-32-chars!!",
    "Issuer": "MyApp",
    "Audience": "MyApp",
    "ExpirationMinutes": 60
  },
  "Database": {
    "ConnectionString": "Data Source=app.db"
  }
}
```

```csharp
services
    .AddAppLogging()
    .AddCoreServices()
    .AddJwtAuthentication(opts => configuration.GetSection("Jwt").Bind(opts))
    .AddDataLibServices(configuration.GetConnectionString("Default")
        ?? configuration["Database:ConnectionString"]!);
```

## Exception → HTTP status mapping (global handler example)

```csharp
app.UseExceptionHandler(exHandler =>
    exHandler.Run(async ctx =>
    {
        var feature = ctx.Features.Get<IExceptionHandlerFeature>();
        var (status, message) = feature?.Error switch
        {
            EntityNotFoundException e  => (404, e.Message),
            DuplicateEntityException e => (409, e.Message),
            SystemEntityException e    => (403, e.Message),
            UnauthorizedException e    => (401, e.Message),
            ValidationException e      => (422, string.Join("; ", e.Errors.Select(x => x.ErrorMessage))),
            _                          => (500, "An unexpected error occurred")
        };
        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(new { error = message });
    }));
```
