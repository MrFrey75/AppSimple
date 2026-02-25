# AppSimple.WebApp — Structure & Documentation

## Overview

`AppSimple.WebApp` is the ASP.NET Core 10 MVC front-end for AppSimple. It connects to
`AppSimple.WebApi` over HTTP — it has **no direct reference** to `AppSimple.Core` or
`AppSimple.DataLib`. Authentication state is stored in a cookie (the JWT token is kept as a
claim and forwarded in every API call).

---

## Project Structure

```
AppSimple.WebApp/
├── Program.cs                         Application entry point
├── AppSimple.WebApp.csproj
├── appsettings.json                   WebApi:BaseUrl, AppLogging config
├── appsettings.Development.json
├── Extensions/
│   ├── WebAppServiceExtensions.cs     AddWebAppServices() DI extension
│   └── LogPath.cs                     Local log-path resolver (mirrors Core.Logging.LogPath)
├── Services/
│   ├── IApiClient.cs                  Interface for all WebApi calls
│   ├── LoginResult.cs                 DTO: token + username + role
│   ├── UserDto.cs                     DTO: safe user representation
│   ├── UpdateProfileRequest.cs        DTO: profile update fields
│   └── Impl/
│       └── ApiClient.cs               Typed HttpClient implementation
├── Models/                            Razor view-models
│   ├── HomeViewModel.cs
│   ├── LoginViewModel.cs
│   ├── ProfileViewModel.cs
│   ├── EditProfileViewModel.cs
│   ├── ChangePasswordViewModel.cs
│   ├── UserListViewModel.cs
│   ├── CreateUserViewModel.cs
│   └── EditUserViewModel.cs
├── Controllers/
│   ├── HomeController.cs              GET /
│   ├── AuthController.cs              GET/POST /login, POST /logout
│   ├── ProfileController.cs           /profile/** [Authorize]
│   └── AdminController.cs             /admin/** [Authorize(Roles="Admin")]
└── Views/
    ├── _ViewImports.cshtml
    ├── _ViewStart.cshtml
    ├── Shared/
    │   ├── _Layout.cshtml             Dark Catppuccin theme + navbar
    │   └── _Notifications.cshtml      TempData alert partial
    ├── Home/Index.cshtml
    ├── Auth/Login.cshtml
    ├── Profile/
    │   ├── Index.cshtml               Read-only profile info
    │   ├── Edit.cshtml                Edit profile fields
    │   └── ChangePassword.cshtml
    └── Admin/
        ├── Index.cshtml               Users table + delete
        ├── Create.cshtml
        └── Edit.cshtml
```

---

## Routes

### Public

| Method | Path | Controller Action | Description |
|---|---|---|---|
| GET | `/` | `HomeController.Index` | Landing page |
| GET | `/login` | `AuthController.Login` | Login form |
| POST | `/login` | `AuthController.Login` | Process login |
| POST | `/logout` | `AuthController.Logout` | Sign out |

### Protected (`[Authorize]`)

| Method | Path | Action | Description |
|---|---|---|---|
| GET | `/profile` | `ProfileController.Index` | View own profile |
| GET | `/profile/edit` | `ProfileController.Edit` | Edit profile form |
| POST | `/profile/edit` | `ProfileController.Edit` | Save profile |
| GET | `/profile/change-password` | `ProfileController.ChangePassword` | Change password form |
| POST | `/profile/change-password` | `ProfileController.ChangePassword` | Process password change |

### Admin (`[Authorize(Roles = "Admin")]`)

| Method | Path | Action | Description |
|---|---|---|---|
| GET | `/admin` | `AdminController.Index` | User list |
| GET | `/admin/create` | `AdminController.Create` | Create user form |
| POST | `/admin/create` | `AdminController.Create` | Create user |
| GET | `/admin/edit/{uid}` | `AdminController.Edit` | Edit user form |
| POST | `/admin/edit/{uid}` | `AdminController.Edit` | Save user |
| POST | `/admin/delete/{uid}` | `AdminController.Delete` | Delete user |

---

## Authentication Flow

```
Browser                          WebApp (MVC)              WebApi (REST)
  │                                   │                         │
  │  POST /login {u, p}               │                         │
  │──────────────────────────────────►│                         │
  │                                   │  POST /api/auth/login   │
  │                                   │────────────────────────►│
  │                                   │◄────────────────────────│
  │                                   │  { token, username, role }
  │                                   │                         │
  │                                   │ SignInAsync(cookie)      │
  │                                   │ claims: Name=username    │
  │                                   │         Role=role        │
  │                                   │         jwt_token=token  │
  │◄──────────────────────────────────│                         │
  │  Set-Cookie: .AspNetCore.Cookies  │                         │
  │                                   │                         │
  │  GET /profile                     │                         │
  │──────────────────────────────────►│                         │
  │                                   │ Read jwt_token claim     │
  │                                   │  GET /api/protected/me  │
  │                                   │  Authorization: Bearer … │
  │                                   │────────────────────────►│
  │                                   │◄────────────────────────│
  │◄──────────────────────────────────│                         │
  │  Profile page HTML                │                         │
```

The JWT token lives only in the server-side auth cookie — it is never sent to the browser directly.

---

## Configuration

Leave `LogDirectory` empty to use the shared OS default (`~/.local/share/AppSimple/logs`).

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

Override `WebApi:BaseUrl` via `appsettings.Development.json` or the `ASPNETCORE_ENVIRONMENT`-specific file when running multiple environments.

---

## IApiClient

```csharp
Task<LoginResult?> LoginAsync(string username, string password);
Task<UserDto?> GetMeAsync(string token);
Task<UserDto?> UpdateMeAsync(string token, UpdateProfileRequest request);
Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword);
Task<IReadOnlyList<UserDto>> GetAllUsersAsync(string token);
Task<UserDto?> GetUserAsync(string token, Guid uid);
Task<UserDto?> CreateUserAsync(string token, string username, string email, string password);
Task<UserDto?> UpdateUserAsync(string token, Guid uid, UpdateProfileRequest request);
Task<bool> DeleteUserAsync(string token, Guid uid);
```

`ApiClient` uses `System.Text.Json` with `PropertyNameCaseInsensitive = true`. All methods return `null` / `false` on non-2xx responses rather than throwing.

---

## Theme

Dark Catppuccin Mocha palette — matches `AppSimple.MvvmApp`. All CSS is inlined in `_Layout.cshtml` (no static file dependency).

| Variable | Value | Usage |
|---|---|---|
| `--bg` | `#1e1e2e` | Window / page background |
| `--surface` | `#181825` | Navbar, cards |
| `--overlay` | `#313244` | Input backgrounds, hover |
| `--text` | `#cdd6f4` | Primary text |
| `--subtext` | `#a6adc8` | Labels, secondary text |
| `--accent` | `#89b4fa` | Links, buttons, focus rings |
| `--green` | `#a6e3a1` | Success state |
| `--red` | `#f38ba8` | Error / danger state |
| `--yellow` | `#f9e2af` | Admin role tag |
| `--border` | `#45475a` | Borders, dividers |

---

## Running Locally

WebApi **must be running first** (default `http://localhost:5157`):

```bash
# Terminal 1 — start WebApi
cd src/AppSimple.WebApi
dotnet run

# Terminal 2 — start WebApp
cd src/AppSimple.WebApp
dotnet run
```

**Default credentials:** `admin` / `Admin123!`

Navigate to `http://localhost:5158` (or whatever port Kestrel assigns).

---

## Tech Stack

| Package | Purpose |
|---|---|
| `Serilog.AspNetCore` | Structured file + console logging |
| `Microsoft.AspNetCore.Authentication.Cookies` | Cookie session auth (built-in) |
| `System.Net.Http.Json` | JSON serialization for HttpClient (built-in) |
