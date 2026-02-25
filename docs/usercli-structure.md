# AppSimple.UserCLI — Source Map

End-user console application targeting `net10.0`. References `AppSimple.Core`
and `AppSimple.DataLib` **directly** — no WebApi layer required.

> **Platform**: Windows · macOS · Linux  
> **UI library**: Pure `Console`/`ConsoleColor` — no third-party TUI dependencies.

## Project file

`AppSimple.UserCLI.csproj`

### NuGet dependencies

| Package | Purpose |
|---|---|
| `CommunityToolkit.Mvvm` | Not used directly — pulled transitively |
| `Microsoft.Extensions.DependencyInjection` 10.x | DI container |
| `Microsoft.Extensions.Configuration.Json` | `appsettings.json` support |
| `Serilog` (via Core) | Structured logging |

Core and DataLib packages are brought in via project references.

---

## Directory layout

```
AppSimple.UserCLI/
├── Program.cs                  DI wiring, DB bootstrap, app entry
├── App.cs                      Top-level login → menu → logout loop controller
├── appsettings.json            DB connection string, JWT, logging config
├── Session/
│   └── UserSession.cs          Singleton session state (CurrentUser, Token, HasPermission)
├── UI/
│   └── ConsoleUI.cs            Static rendering helpers — headers, menus, input, tables
└── Menus/
    ├── LoginMenu.cs            Login screen — username/password, masked input, exit option
    ├── MainMenu.cs             Post-login menu — role-aware items, logout
    ├── ProfileMenu.cs          View/edit own profile + change password
    └── AdminMenu.cs            Admin-only — list/create/edit/delete users
```

---

## Application flow

```
startup
  │  Program.cs — DI, DB init, admin seed
  │
  ▼
App.RunAsync()
  │
  ├─[not logged in]──► LoginMenu.ShowAsync()
  │                      ├── [1] Enter username + masked password
  │                      │      └─ AuthService.LoginAsync() → session.Login()
  │                      └── [0] Exit app
  │
  └─[logged in]──────► MainMenu.ShowAsync()
                          ├── [1] My Profile ──────► ProfileMenu.ShowAsync()
                          │                            ├── [1] View Profile
                          │                            ├── [2] Edit Profile
                          │                            └── [3] Change Password
                          │
                          ├── [2] User Management ──► AdminMenu.ShowAsync()   (Admin only)
                          │                            ├── [1] List All Users
                          │                            ├── [2] Create New User
                          │                            ├── [3] Edit a User
                          │                            └── [4] Delete a User
                          │
                          └── [0] Log Out → back to login
```

---

## `Session/UserSession.cs`

Singleton that holds the authenticated user for the lifetime of the process.

| Member | Description |
|---|---|
| `CurrentUser` | The logged-in `User`, or `null` |
| `Token` | JWT issued at login, or `null` |
| `IsLoggedIn` | `true` when `CurrentUser` is not null |
| `Login(user, token)` | Sets session state |
| `Logout()` | Clears session state |
| `HasPermission(permission)` | Role-based permission check — Admin has all; User has View/EditProfile only |

---

## `UI/ConsoleUI.cs`

Static helper class. All rendering goes through here — no direct `Console.*` calls in menu classes.

### Rendering helpers

| Method | Output |
|---|---|
| `Clear(showHeader)` | Clears screen and optionally redraws the app banner |
| `WriteHeader()` | Draws the double-line `╔═╗` banner with the app title |
| `WriteHeading(text)` | Section heading with a coloured `─` underline |
| `WriteSeparator()` | Full-width `─────` horizontal rule |

### Status messages

| Method | Colour | Prefix |
|---|---|---|
| `WriteSuccess(msg)` | Green | `✓` |
| `WriteError(msg)` | Red | `✗` |
| `WriteWarning(msg)` | Yellow | `⚠` |
| `WriteInfo(msg)` | Cyan | `ℹ` |

### Menu rendering

| Method | Description |
|---|---|
| `WriteMenuItem(n, label, desc?)` | Numbered menu item with optional dim description |
| `WriteMenuGroupLabel(label)` | Visual group separator inside a menu |
| `WriteBackItem(label?)` | `[0] Back` / `[0] Exit` item |

### Input helpers

| Method | Description |
|---|---|
| `ReadLine(prompt)` | Required string — loops until non-empty |
| `ReadOptionalLine(prompt, current?)` | Optional — Enter keeps current value |
| `ReadPassword(prompt)` | Masked input — each char replaced with `*` |
| `ReadMenuChoice(max)` | Validates integer `[0..max]` |
| `Confirm(question)` | `[y/N]` confirmation |
| `Pause(message?)` | Press-any-key pause |

### Data display

| Method | Description |
|---|---|
| `WriteUserTable(users)` | Tabular list with index, username, email, role, active; system users highlighted |
| `WriteUserDetail(user)` | Full label-value detail card |

---

## Menus

### `LoginMenu`

Dependency-injected: `IAuthService`, `IUserService`, `UserSession`.

- Shows `[1] Log In` / `[0] Exit`
- Reads username with `ReadLine` and password with `ReadPassword` (masked)
- Calls `IAuthService.LoginAsync(username, password)`
- On success: calls `session.Login(user, token)` and returns `false` (logged in)
- On failure: displays error and loops
- `[0]` returns `true` (exit requested) → `App.RunAsync()` breaks its loop

### `MainMenu`

Dependency-injected: `ProfileMenu`, `AdminMenu`, `UserSession`.

- Shows a logged-in context bar (`username [Role]`)
- Item `[1] My Profile` is always available
- Item `[2] User Management` appears only when `user.Role == UserRole.Admin`
- `[0] Log Out` asks for confirmation, calls `session.Logout()`, returns to login

### `ProfileMenu`

Dependency-injected: `IUserService`, `UserSession`.

| Item | Action |
|---|---|
| `[1] View Profile` | Renders `WriteUserDetail` for the current user |
| `[2] Edit Profile` | `ReadOptionalLine` for each editable field; calls `IUserService.UpdateAsync()` |
| `[3] Change Password` | Three masked password prompts; validates confirmation; calls `IUserService.ChangePasswordAsync()` |

### `AdminMenu`

Dependency-injected: `IUserService`, `UserSession`.

| Item | Action |
|---|---|
| `[1] List All Users` | `IUserService.GetAllAsync()` → `WriteUserTable` |
| `[2] Create New User` | Collects username, email, password, first/last name, role; calls `IUserService.CreateAsync()` |
| `[3] Edit a User` | Lists users, selects by number; edits email, names, role, active flag; protects system users |
| `[4] Delete a User` | Lists users, selects by number; `Confirm` prompt; calls `IUserService.DeleteAsync()` |

System users (`IsSystem = true`) are protected: edit and delete operations are rejected with an error message.

---

## DI registration (`Program.cs`)

```csharp
services
    .AddAppLogging(opts =>
    {
        opts.ApplicationName = "AppSimple.UserCLI";
        opts.EnableConsole   = false;   // file-only — avoids mixing log output with CLI UI
        opts.EnableFile      = true;
        opts.LogDirectory    = "logs";
    })
    .AddCoreServices()
    .AddJwtAuthentication(opts => configuration.GetSection("Jwt").Bind(opts))
    .AddDataLibServices(connectionString);

services.AddSingleton<UserSession>();
services.AddTransient<LoginMenu>();
services.AddTransient<ProfileMenu>();
services.AddTransient<AdminMenu>();
services.AddTransient<MainMenu>();
services.AddTransient<App>();
```

> Logging is configured as **file-only** (`EnableConsole = false`) so Serilog output
> does not interleave with the interactive console UI.

---

## Default credentials

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | Admin |

The admin user is seeded on first startup via `DbInitializer.SeedAdminUser()`.

---

## Running the application

```bash
cd src/AppSimple.UserCLI
$HOME/.dotnet/dotnet run
```

Or from the `src/` folder:

```bash
$HOME/.dotnet/dotnet run --project AppSimple.UserCLI/AppSimple.UserCLI.csproj
```
