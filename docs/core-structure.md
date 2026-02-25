# AppSimple.Core — Source Map

Full file listing with purpose for every type in the project.

## Project file

`AppSimple.Core.csproj` — targets `net10.0`, C# 14 preview, nullable enabled.

### NuGet dependencies

| Package | Purpose |
|---|---|
| `BCrypt.Net-Next` 4.x | Password hashing |
| `FluentValidation` 11.x | Validation rules |
| `FluentValidation.DependencyInjectionExtensions` 11.x | DI auto-registration |
| `Microsoft.Extensions.DependencyInjection.Abstractions` 10.x | `IServiceCollection` |
| `Microsoft.Extensions.Options` 10.x | `IOptions<T>` |
| `Serilog` 4.x | Core logging library |
| `Serilog.Sinks.Console` 6.x | Console sink |
| `Serilog.Sinks.File` 7.x | Rolling file sink |
| `Serilog.Enrichers.Environment` | Machine name + env name enricher |
| `Serilog.Enrichers.Thread` | Thread ID enricher |
| `Serilog.Enrichers.Process` | Process ID enricher |
| `Serilog.Extensions.Logging` | Bridge to `Microsoft.Extensions.Logging` |
| `System.IdentityModel.Tokens.Jwt` 8.x | JWT generation and validation |

---

## `Models/`

### `BaseEntity.cs`
Abstract base class for all domain entities.

| Property | Type | Default | Notes |
|---|---|---|---|
| `Uid` | `Guid` | `Guid.CreateVersion7()` | Time-ordered v7 GUID for DB performance |
| `CreatedAt` | `DateTime` | `default` | Set by service layer on create |
| `UpdatedAt` | `DateTime` | `default` | Set by service layer on every write |
| `IsSystem` | `bool` | `false` | If true, entity cannot be modified or deleted |

### `User.cs`
Concrete entity representing an application user.

| Property | Type | Required | Notes |
|---|---|---|---|
| `Username` | `string` | ✓ | Unique, max 50 chars, alphanumeric + underscore |
| `PasswordHash` | `string` | ✓ | BCrypt hash — never store plain text |
| `Email` | `string` | ✓ | Unique, max 256 chars |
| `FirstName` | `string?` | — | max 100 chars |
| `LastName` | `string?` | — | max 100 chars |
| `PhoneNumber` | `string?` | — | max 30 chars |
| `DateOfBirth` | `DateTime?` | — | UTC |
| `Bio` | `string?` | — | max 500 chars |
| `AvatarUrl` | `string?` | — | URL or relative path |
| `Role` | `UserRole` | — | Defaults to `User` |
| `IsActive` | `bool` | — | Defaults to `true` |
| `FullName` | `string?` | — | **Computed** — `$"{FirstName} {LastName}".Trim()`, null if both empty |

### `Models/Requests/`

| Class | Used by | Key fields |
|---|---|---|
| `CreateUserRequest` | `IUserService.CreateAsync` callers | Username, Email, Password, optional FirstName/LastName |
| `UpdateUserRequest` | `IUserService.UpdateAsync` callers | All-optional profile fields |
| `ChangePasswordRequest` | `IUserService.ChangePasswordAsync` callers | CurrentPassword, NewPassword, ConfirmNewPassword |

---

## `Enums/`

### `UserRole.cs`
```
User  = 0   Standard user
Admin = 1   Administrator
```

### `Permission.cs`
```
ViewProfile  = 10   View own profile
EditProfile  = 11   Edit own profile
ViewUsers    = 20   View all users (Admin)
CreateUser   = 21   Create users (Admin)
EditUser     = 22   Edit any user (Admin)
DeleteUser   = 23   Delete any user (Admin)
```

---

## `Constants/AppConstants.cs`

| Constant | Value | Purpose |
|---|---|---|
| `AppName` | `"AppSimple"` | Application name property in logs |
| `DefaultAdminUsername` | `"admin"` | Seeded admin username |
| `MinPasswordLength` | `8` | Minimum password length |
| `MaxUsernameLength` | `50` | Maximum username length |
| `MaxEmailLength` | `256` | Maximum email length |
| `MaxNameLength` | `100` | Maximum first/last name length |
| `MaxPhoneLength` | `30` | Maximum phone number length |
| `MaxBioLength` | `500` | Maximum bio length |

---

## `Interfaces/`

### `IRepository<T>.cs`
Generic CRUD contract.

```csharp
Task<T?>              GetByUidAsync(Guid uid)
Task<IEnumerable<T>>  GetAllAsync()
Task                  AddAsync(T entity)
Task                  UpdateAsync(T entity)
Task                  DeleteAsync(Guid uid)
```

### `IUserRepository.cs`
Extends `IRepository<User>` with user-specific lookups.

```csharp
Task<User?>  GetByUsernameAsync(string username)   // case-insensitive
Task<User?>  GetByEmailAsync(string email)          // case-insensitive
Task<bool>   UsernameExistsAsync(string username)
Task<bool>   EmailExistsAsync(string email)
```

---

## `Auth/`

### `IPasswordHasher.cs`
```csharp
string Hash(string plainPassword)
bool   Verify(string plainPassword, string hash)
```

### `IJwtTokenService.cs`
```csharp
string  GenerateToken(User user)
string? GetUsernameFromToken(string token)
bool    IsTokenValid(string token)
```

### `JwtOptions.cs`
Bind from `appsettings.json → "Jwt"` section.

| Property | Default | Notes |
|---|---|---|
| `Secret` | — | **Required.** Min 32 chars. |
| `Issuer` | `"AppSimple"` | JWT `iss` claim |
| `Audience` | `"AppSimple"` | JWT `aud` claim |
| `ExpirationMinutes` | `60` | Token lifetime |

### `Auth/Impl/BcryptPasswordHasher.cs`
BCrypt with work-factor 12.

### `Auth/Impl/JwtTokenService.cs`
HMAC-SHA256 (HS256). Embeds claims: `sub` (Uid), `unique_name` (Username), `email`, `role`, `jti` (Guid v7).

---

## `Common/`

### `Result.cs`

Two types sharing the same pattern:

**`Result<T>`** — for operations that return a value:
```csharp
Result<User> r = Result<User>.Success(user);
Result<User> r = Result<User>.Failure("error message");
Result<User> r = Result<User>.Failure(errors);   // IEnumerable<string>
User value     = r.Value;          // non-null on success
string? error  = r.Error;          // first error, or null
IReadOnlyList<string> errors = r.Errors;
// implicit: Result<T> r = someValue;
```

**`Result`** — for void operations:
```csharp
Result r = Result.Success();
Result r = Result.Failure("error");
```

### `Common/Exceptions/`

| Exception | Constructor | Extra properties |
|---|---|---|
| `AppException` | `(string message)` or `(string message, Exception inner)` | — |
| `EntityNotFoundException` | `(string entityType, object entityId)` | `EntityType`, `EntityId` |
| `DuplicateEntityException` | `(string field, string value)` | `Field`, `Value` |
| `SystemEntityException` | `(string entityType)` | `EntityType` |
| `UnauthorizedException` | `(string message)` | — |

---

## `Services/`

### `IUserService.cs`
```csharp
Task<User?>             GetByUidAsync(Guid uid)
Task<User?>             GetByUsernameAsync(string username)
Task<IEnumerable<User>> GetAllAsync()
Task<User>              CreateAsync(string username, string email, string plainPassword)
Task                    UpdateAsync(User user)
Task                    DeleteAsync(Guid uid)
Task                    ChangePasswordAsync(Guid uid, string currentPassword, string newPassword)
```

**Exceptions thrown by `UserService`:**

| Method | Throws |
|---|---|
| `CreateAsync` | `DuplicateEntityException` (username or email taken) |
| `UpdateAsync` | `EntityNotFoundException`, `SystemEntityException` |
| `DeleteAsync` | `EntityNotFoundException`, `SystemEntityException` |
| `ChangePasswordAsync` | `EntityNotFoundException`, `UnauthorizedException` |

### `IAuthService.cs`
```csharp
Task<AuthResult> LoginAsync(string username, string plainPassword)
string?          ValidateToken(string token)
```

### `AuthResult.cs`
```csharp
AuthResult.Success(token)   // Succeeded=true, Token set
AuthResult.Failure(message) // Succeeded=false, Token=null
```

---

## `Logging/`

### `IAppLogger<T>.cs`
Inject with `IAppLogger<MyClass>` — each instance is enriched with `SourceContext = typeof(T).Name`.

```csharp
void Debug(string template, params object?[] args)
void Information(string template, params object?[] args)
void Warning(string template, params object?[] args)
void Error(string template, params object?[] args)
void Error(Exception ex, string template, params object?[] args)
void Fatal(string template, params object?[] args)
void Fatal(Exception ex, string template, params object?[] args)
bool IsEnabled(LogEventLevel level)
```

### `LoggingOptions.cs`

| Property | Default | Notes |
|---|---|---|
| `MinimumLevel` | `Information` | Serilog `LogEventLevel` |
| `EnableConsole` | `true` | Write to stdout |
| `EnableFile` | `false` | Write rolling file |
| `LogDirectory` | `"logs"` | Relative to working dir |
| `OutputTemplate` | `[HH:mm:ss LVL] [SourceContext] Message` | Serilog template |
| `ApplicationName` | `"AppSimple"` | Added as `Application` property |
| `RollingInterval` | `Day` | New file per day |
| `RetainedFileCountLimit` | `7` | Keep last 7 files |

---

## `Validators/`

### `CreateUserRequestValidator`
- `Username`: not empty, max 50, pattern `^[a-zA-Z0-9_]+$`
- `Email`: not empty, max 256, valid email format
- `Password`: not empty, min 8 chars, must have upper, lower, digit
- `FirstName` / `LastName`: optional, max 100 when provided

### `UpdateUserRequestValidator`
- `PhoneNumber`: optional, max 30, pattern `^[+\d\s\-().]+$`
- `DateOfBirth`: optional, must be past, must be after 1900-01-01
- `Bio`: optional, max 500
- `FirstName` / `LastName`: optional, max 100

### `ChangePasswordRequestValidator`
- `CurrentPassword`: not empty
- `NewPassword`: same complexity rules as creation + must differ from current
- `ConfirmNewPassword`: must equal `NewPassword`

---

## `Extensions/CoreServiceExtensions.cs`

```csharp
// Register validators, IPasswordHasher, IUserService, IAuthService
services.AddCoreServices();

// Register IJwtTokenService + configure JwtOptions
services.AddJwtAuthentication(opts => {
    opts.Secret = "32-char-minimum-secret-key!!!!!";
    opts.ExpirationMinutes = 60;
});

// Configure Serilog + register IAppLogger<> as open-generic transient
services.AddAppLogging(opts => {
    opts.EnableConsole = true;
    opts.EnableFile    = true;
});
```
