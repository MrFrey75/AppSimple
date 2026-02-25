# AppSimple.DataLib — Source Map

Full file listing with purpose for every type in the project.

## Project file

`AppSimple.DataLib.csproj` — targets `net10.0`, C# 14 preview, nullable enabled. References `AppSimple.Core`.

### NuGet dependencies

| Package | Purpose |
|---|---|
| `Dapper` 2.x | Micro-ORM — maps SQL results to C# objects |
| `Microsoft.Data.Sqlite` 10.x | SQLite ADO.NET driver |
| `Microsoft.Extensions.DependencyInjection.Abstractions` 10.x | `IServiceCollection` |
| `Microsoft.Extensions.Options` 10.x | `IOptions<T>` |

---

## `Db/`

### `IDbConnectionFactory.cs`
```csharp
IDbConnection CreateConnection();
```
Opens and returns a ready-to-use `IDbConnection`. Callers are responsible for disposal.

### `DatabaseOptions.cs`
POCO bound from configuration:
```json
{ "Database": { "ConnectionString": "Data Source=app.db" } }
```

### `SqliteConnectionFactory.cs`
Implementation of `IDbConnectionFactory`. Creates and opens a `SqliteConnection` from `DatabaseOptions.ConnectionString`.

### `DapperConfig.cs`
**Must be called once before any Dapper queries on SQLite.**

```csharp
DapperConfig.Register();
```

Registers two Dapper type handlers:

| Handler | Maps |
|---|---|
| `GuidTypeHandler` | `TEXT` ↔ `System.Guid` |
| `DateTimeUtcTypeHandler` | `TEXT` ↔ `System.DateTime` (UTC, ISO-8601) |

Without these handlers, Dapper cannot map GUID and DateTime columns from SQLite TEXT storage into C# types.

**Where it is called:**
- `DataLibServiceExtensions.AddDataLibServices()` — production startup
- `DatabaseTestBase` constructor — test setup

### `DbInitializer.cs`
Service for one-time database bootstrap. Register as a singleton (see DI section).

```csharp
void Initialize()
// Creates the Users table if it does not exist. Safe to call on every startup.

void SeedAdminUser(string hashedAdminPassword)
// Inserts the default admin user (IsSystem=true, Role=Admin) if not already present.
// Takes a pre-hashed password (use IPasswordHasher.Hash() to generate it).
```

**Schema created:**

```sql
CREATE TABLE IF NOT EXISTS Users (
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

---

## `Repositories/`

### `UserRepository.cs`
Implements `IUserRepository` (defined in Core) using Dapper.

| Method | SQL notes |
|---|---|
| `GetByUidAsync(uid)` | `SELECT … WHERE Uid = @Uid` |
| `GetByUsernameAsync(username)` | `WHERE Username = @Username` — case-insensitive via COLLATE NOCASE |
| `GetByEmailAsync(email)` | `WHERE Email = @Email` — case-insensitive |
| `UsernameExistsAsync(username)` | `SELECT COUNT(1)` — returns `bool` |
| `EmailExistsAsync(email)` | `SELECT COUNT(1)` — returns `bool` |
| `GetAllAsync()` | Full table scan |
| `AddAsync(entity)` | Full INSERT with all columns |
| `UpdateAsync(entity)` | `WHERE Uid = @Uid AND IsSystem = 0` — system users are silently skipped |
| `DeleteAsync(uid)` | `WHERE Uid = @Uid AND IsSystem = 0` — system users are silently skipped |

**`MapToParams` helper** serialises a `User` to a Dapper-compatible anonymous object, converting:
- `Guid` → `string` (`.ToString()`)
- `bool` → `int` (0/1)
- `DateTime?` → ISO-8601 string or `null`
- `DateTime` → ISO-8601 string

> **Note**: The `AND IsSystem = 0` guard in SQL is a defence-in-depth measure. The service layer (`UserService`) also checks `IsSystem` and throws `SystemEntityException` before calling the repository.

---

## `Extensions/DataLibServiceExtensions.cs`

```csharp
services.AddDataLibServices("Data Source=app.db");
```

Registers:

| Service | Lifetime |
|---|---|
| `DatabaseOptions` (via `IOptions<DatabaseOptions>`) | Singleton |
| `IDbConnectionFactory` → `SqliteConnectionFactory` | Singleton |
| `DbInitializer` | Singleton |
| `IUserRepository` → `UserRepository` | Scoped |

Also calls `DapperConfig.Register()` exactly once.

---

## Test helpers (`AppSimple.DataLib.Tests/Helpers/`)

These helpers solve the problem of SQLite in-memory databases being destroyed when a connection is closed.

### `InMemoryDbConnectionFactory.cs`
Wraps a single persistent `SqliteConnection("Data Source=:memory:")`. Returns a `NonClosingConnectionWrapper` on every `CreateConnection()` call — the underlying connection is never actually closed.

### `NonClosingConnectionWrapper.cs`
Decorator over `IDbConnection`. All methods delegate to the real connection **except** `Dispose()` and `Close()`, which are no-ops. This prevents `using var connection = _factory.CreateConnection()` in the repository from destroying the in-memory database.

### `DatabaseTestBase.cs`
Abstract base class for all DataLib tests.
- Constructor: opens the in-memory connection, calls `DapperConfig.Register()`, runs `DbInitializer.Initialize()`.
- `IDisposable.Dispose()`: closes and disposes the real underlying connection.

### `UserFactory.cs`
Creates `User` instances pre-populated with realistic test data. Overloads accept a username/email suffix to avoid unique-constraint collisions across tests.
