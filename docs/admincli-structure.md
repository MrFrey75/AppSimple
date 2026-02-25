# AppSimple.AdminCli — Structure & Documentation

## Overview

`AppSimple.AdminCli` is the administrator command-line tool for AppSimple. It connects to
`AppSimple.WebApi` over HTTP — it has **no direct reference** to `AppSimple.Core` or
`AppSimple.DataLib`. Access is restricted to users with the `Admin` role; non-admin logins
are rejected before a session is established.

---

## Project Structure

```
AppSimple.AdminCli/
├── Program.cs                         Application entry point — DI → App.RunAsync
├── App.cs                             Login/session loop controller
├── AppSimple.AdminCli.csproj
├── appsettings.json                   WebApi:BaseUrl, AppLogging config
├── Extensions/
│   ├── AdminCliServiceExtensions.cs   AddAdminCliServices() DI extension
│   └── LogPath.cs                     Local log-path resolver (mirrors Core.Logging.LogPath)
├── Services/
│   ├── IApiClient.cs                  Interface for all WebApi calls
│   ├── LoginResult.cs                 DTO: token + username + role
│   ├── UserDto.cs                     DTO: safe user representation from API
│   ├── UpdateUserRequest.cs           DTO: partial user update fields
│   ├── HealthResult.cs                DTO: API health check response
│   └── Impl/
│       └── ApiClient.cs               Typed HttpClient implementation
├── Session/
│   └── AdminSession.cs                Holds token + username, IsLoggedIn
├── UI/
│   └── ConsoleUI.cs                   Console rendering helpers — table, detail, menus
└── Menus/
    ├── LoginMenu.cs                   Prompts credentials, enforces Admin-role gate
    ├── MainMenu.cs                    Top-level: Users | System | Logout
    ├── UsersMenu.cs                   Full user management (list, create, edit, delete, role)
    └── SystemMenu.cs                  Health check, smoke test, seed test users
```

---

## Configuration (`appsettings.json`)

```json
{
  "WebApi": {
    "BaseUrl": "http://localhost:5157"
  },
  "AppLogging": {
    "EnableFile": true,
    "LogDirectory": ""
  }
}
```

| Key | Description | Default |
|-----|-------------|---------|
| `WebApi:BaseUrl` | Base URL of the running WebApi | `http://localhost:5157` |
| `AppLogging:EnableFile` | Write log to rolling daily file | `true` |
| `AppLogging:LogDirectory` | Override log output directory | *(resolved via LogPath)* |

Log directory resolution order: `AppLogging:LogDirectory` config → `APPSIMPLE_LOGS` env var →
`~/.local/share/AppSimple/logs` (Linux) / `%LOCALAPPDATA%\AppSimple\logs` (Windows).

---

## DI Registration (`AddAdminCliServices`)

```csharp
services.AddAdminCliServices(config);
```

Registers:
- **Serilog** file sink (rolling daily, using resolved `LogPath`)
- **Typed `HttpClient<IApiClient>`** — base address from `WebApi:BaseUrl`
- `AdminSession` (singleton)
- `LoginMenu`, `MainMenu`, `UsersMenu`, `SystemMenu`, `App` (all transient)

---

## Authentication & Session

1. `LoginMenu.ShowAsync()` calls `IApiClient.LoginAsync(username, password)`
2. If the WebApi returns a `LoginResult` where `Role != "Admin"` → access denied, no session
3. On success, `AdminSession.Login(token, username)` stores the bearer token in memory
4. All subsequent `ApiClient` calls pass the token as a per-request `Authorization: Bearer` header
5. `Logout()` clears the in-memory session — the JWT is not persisted to disk

---

## Menu Overview

### Login Menu
```
╔══════════════════════════════════════════════════════════════════════╗
║                      AppSimple Admin CLI                            ║
╚══════════════════════════════════════════════════════════════════════╝

  [1] Log In
  [0] Exit
```

### Main Menu (post-login)
```
  Logged in as: admin

  [1] User Management
  [2] System & Health
  [0] Log Out
```

### Users Menu
```
  [1] List All Users
  [2] Create New User
  [3] View User Details
  [4] Edit User
  [5] Delete User
  [6] Change User Role
  [0] Back
```

- **Delete** blocks system users (`IsSystem = true`)
- **Change Role** shows current role and prompts: `[1] Admin  [2] User`

### System Menu
```
  [1] Health Check
  [2] Smoke Test
  [3] Seed Test Users
  [0] Back
```

- **Health Check** → `GET /api/health` — displays Status, Timestamp, Uptime
- **Smoke Test** → three sequential checks:
  - ✓ `GET /api/health` — API reachable
  - ✓ `GET /api/protected` with token — Auth works
  - ✓ `GET /api/admin/users` with token — Admin access works
- **Seed Test Users** → creates `testuser1–3` at `test1@appsimple.dev` (skips if already exists)

---

## WebApi Endpoints Used

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `POST` | `/api/auth/login` | None | Login, returns JWT + role |
| `GET` | `/api/health` | None | Service health check |
| `GET` | `/api/protected` | Bearer | Verify token validity |
| `GET` | `/api/admin/users` | Bearer (Admin) | List all users |
| `GET` | `/api/admin/users/{uid}` | Bearer (Admin) | Get single user |
| `POST` | `/api/admin/users` | Bearer (Admin) | Create new user |
| `PUT` | `/api/admin/users/{uid}` | Bearer (Admin) | Update user fields |
| `DELETE` | `/api/admin/users/{uid}` | Bearer (Admin) | Delete user |
| `PATCH` | `/api/admin/users/{uid}/role` | Bearer (Admin) | Set user role (0=User, 1=Admin) |

---

## Running

Ensure `AppSimple.WebApi` is running first, then:

```bash
cd src/AppSimple.AdminCli
dotnet run
```

Or override the WebApi URL at runtime:

```bash
APPSIMPLE_WEBAPI_URL=http://192.168.1.10:5157 dotnet run
```

---

## Packages

| Package | Purpose |
|---------|---------|
| `Serilog.Sinks.File` | Rolling daily log files |
| `Serilog.Enrichers.Environment` | Enrich log events with machine/env data |
| `Microsoft.Extensions.DependencyInjection` | DI container |
| `Microsoft.Extensions.Configuration.Json` | JSON config from appsettings.json |
| `Microsoft.Extensions.Http` | `IHttpClientFactory` / typed `HttpClient` |
| `Microsoft.Extensions.Options.ConfigurationExtensions` | `GetValue<T>` helpers |
