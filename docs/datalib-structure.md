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
{ "Database": { "ConnectionString": "" } }
```
Setting `ConnectionString` to an empty string (or omitting it) causes `DatabasePath.Resolve()` to fall back to the shared default location.

### `DatabasePath.cs`
Static helper that resolves the SQLite connection string for all host projects.

```csharp
string cs = DatabasePath.Resolve(config["Database:ConnectionString"]);
// Pass the resolved connection string to AddDataLibServices(cs)
```

**Resolution priority:**

| Priority | Source | Example value |
|---|---|---|
| 1 | `Database:ConnectionString` in `appsettings.json` | any non-empty connection string |
| 2 | `APPSIMPLE_DB` environment variable | file path — wrapped in `Data Source=...` automatically |
| 3 | **Default shared location** | `~/.local/share/AppSimple/appsimple.db` (Linux/macOS) / `%LOCALAPPDATA%\AppSimple\appsimple.db` (Windows) |

The directory is created automatically if it does not exist.

`DatabasePath.FilePath(configValue?)` returns the raw file path extracted from any connection string produced by `Resolve()`.

### `SqliteConnectionFactory.cs`
Implementation of `IDbConnectionFactory`. Creates and opens a `SqliteConnection` from `DatabaseOptions.ConnectionString`.

### `DapperConfig.cs`
**Must be called once before any Dapper queries on SQLite.**

```csharp
DapperConfig.Register();
```

Registers three Dapper type handlers:

| Handler | Maps |
|---|---|
| `GuidTypeHandler` | `TEXT` ↔ `System.Guid` |
| `DateTimeUtcTypeHandler` | `TEXT` ↔ `System.DateTime` (UTC, ISO-8601) |
| `JsonStringListTypeHandler` | `TEXT` (JSON) ↔ `List<string>` |

Without these handlers, Dapper cannot map GUID and DateTime columns from SQLite TEXT storage into C# types.

**Where it is called:**
- `DataLibServiceExtensions.AddDataLibServices()` — production startup
- `DatabaseTestBase` constructor — test setup

### `DbInitializer.cs`
Service for one-time database bootstrap. Register as a singleton (see DI section).

```csharp
void Initialize()
// Creates all tables (Users, Tags, Notes, NoteTags) if they do not exist. Safe to call on every startup.

void SeedAdminUser(string hashedAdminPassword)
// Inserts the default admin user (IsSystem=true, Role=Admin) if not already present.
// Takes a pre-hashed password (use IPasswordHasher.Hash() to generate it).
```

**Schema created:**

```sql
CREATE TABLE IF NOT EXISTS Notes ( ... );        -- see README for full DDL
CREATE TABLE IF NOT EXISTS NoteTags ( ... );     -- NoteUid + TagUid junction

CREATE TABLE IF NOT EXISTS Contacts (
    Uid          TEXT NOT NULL PRIMARY KEY,
    OwnerUserUid TEXT NOT NULL REFERENCES Users(Uid) ON DELETE CASCADE,
    Name         TEXT NOT NULL COLLATE NOCASE,
    Tags         TEXT NOT NULL DEFAULT '[]',     -- JSON array of strings
    IsSystem     INTEGER NOT NULL DEFAULT 0,
    CreatedAt    TEXT NOT NULL,
    UpdatedAt    TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ContactEmailAddresses (
    Uid        TEXT NOT NULL PRIMARY KEY,
    ContactUid TEXT NOT NULL REFERENCES Contacts(Uid) ON DELETE CASCADE,
    Email      TEXT NOT NULL COLLATE NOCASE,
    IsPrimary  INTEGER NOT NULL DEFAULT 0,
    Tags       TEXT NOT NULL DEFAULT '[]',
    Type       INTEGER NOT NULL DEFAULT 0,   -- EmailType enum value
    ...
);

CREATE TABLE IF NOT EXISTS ContactPhoneNumbers ( ... );
CREATE TABLE IF NOT EXISTS ContactAddresses ( ... );
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

### `NoteRepository.cs`
Implements `INoteRepository` using Dapper. Tags are loaded via a second query joining on `NoteTags`.

| Method | SQL notes |
|---|---|
| `GetByUidAsync(uid)` | SELECT from Notes + `LoadTagsAsync` |
| `GetAllAsync()` | All notes ordered by `UpdatedAt DESC` + tags per note |
| `GetByUserUidAsync(userUid)` | `WHERE UserUid = @UserUid` + tags per note |
| `AddAsync(entity)` | INSERT into Notes |
| `UpdateAsync(entity)` | UPDATE Title, Content, UpdatedAt |
| `DeleteAsync(uid)` | DELETE from Notes (NoteTags removed by CASCADE) |
| `AddTagAsync(noteUid, tagUid)` | `INSERT OR IGNORE INTO NoteTags` |
| `RemoveTagAsync(noteUid, tagUid)` | DELETE from NoteTags |

### `ContactRepository.cs`
Implements `IContactRepository`. Child collections are loaded via separate queries per contact.
`List<string>` Tags columns are serialised as JSON by `JsonStringListTypeHandler`.

| Method | SQL notes |
|---|---|
| `GetByUidAsync(uid)` | SELECT from Contacts + `PopulateChildrenAsync` |
| `GetAllAsync()` | All contacts ordered by Name + children per contact |
| `GetByOwnerUidAsync(ownerUid)` | `WHERE OwnerUserUid = @OwnerUserUid` + children |
| `AddAsync(entity)` | INSERT into Contacts |
| `UpdateAsync(entity)` | UPDATE Name, Tags, UpdatedAt |
| `DeleteAsync(uid)` | DELETE from Contacts (children removed by CASCADE) |
| `AddEmailAddressAsync` / `UpdateEmailAddressAsync` / `DeleteEmailAddressAsync` | ContactEmailAddresses table |
| `AddPhoneNumberAsync` / `UpdatePhoneNumberAsync` / `DeletePhoneNumberAsync` | ContactPhoneNumbers table |
| `AddAddressAsync` / `UpdateAddressAsync` / `DeleteAddressAsync` | ContactAddresses table |

**`PopulateChildrenAsync` helper** — fires three queries (emails, phones, addresses) for one contact using the shared connection.
Implements `ITagRepository` using Dapper.

| Method | SQL notes |
|---|---|
| `GetByUidAsync(uid)` | `WHERE Uid = @Uid` |
| `GetAllAsync()` | All tags ordered by Name |
| `GetByUserUidAsync(userUid)` | `WHERE UserUid = @UserUid ORDER BY Name` |
| `GetByNameAsync(userUid, name)` | `WHERE UserUid = @UserUid AND Name = @Name COLLATE NOCASE` |
| `AddAsync(entity)` | Full INSERT |
| `UpdateAsync(entity)` | UPDATE Name, Description, Color, UpdatedAt |
| `DeleteAsync(uid)` | DELETE from Tags — NoteTags rows removed by `ON DELETE CASCADE` |

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
| `INoteRepository` → `NoteRepository` | Scoped |
| `ITagRepository` → `TagRepository` | Scoped |
| `IContactRepository` → `ContactRepository` | Scoped |
| `IDatabaseResetService` → `DatabaseResetService` | Scoped |

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
