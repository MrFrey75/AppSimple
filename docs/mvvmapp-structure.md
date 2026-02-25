# AppSimple.MvvmApp — Source Map

WPF desktop application targeting `net10.0-windows`. References `AppSimple.Core`
and `AppSimple.DataLib` **directly** — no WebApi layer required.

> **Platform**: Windows only (`net10.0-windows`, `UseWPF=true`).

## Project file

`AppSimple.MvvmApp.csproj`

### NuGet dependencies

| Package | Purpose |
|---|---|
| `CommunityToolkit.Mvvm` 8.x | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` |
| `Microsoft.Extensions.Hosting` 10.x | DI container, IConfiguration |

---

## Directory layout

```
AppSimple.MvvmApp/
├── App.xaml / App.xaml.cs          Application entry — DI wiring, DB init
├── MainWindow.xaml / .cs           Shell window — NavBar + ContentControl
├── appsettings.json                DB, JWT, logging config
├── Session/
│   └── UserSession.cs              Singleton session state (CurrentUser, Token, HasPermission)
├── Converters/
│   ├── BoolToVisibilityConverter.cs        bool → Visibility
│   ├── InverseBoolToVisibilityConverter.cs bool → Visibility (inverted)
│   └── FormWidthConverter.cs               bool → column MaxWidth (0 or 320)
├── ViewModels/
│   ├── BaseViewModel.cs            ObservableObject + IsBusy/Error/Success helpers
│   ├── HomeViewModel.cs            Landing page — IsLoggedIn, WelcomeText, Refresh()
│   ├── ProfileViewModel.cs         Own-profile view/edit + ChangePasswordAsync()
│   ├── UsersViewModel.cs           Admin user management — ObservableCollection, CRUD
│   └── MainWindowViewModel.cs      Navigation + login/logout + IsLoggedIn/IsAdmin
├── Views/
│   ├── HomeView.xaml / .cs         Public landing page
│   ├── ProfileView.xaml / .cs      Profile form (code-behind handles PasswordBoxes)
│   └── UsersView.xaml / .cs        DataGrid + right-panel form (code-behind handles PasswordBox)
└── Controls/
    ├── NavBar.xaml                 Top navigation bar UserControl
    └── NavBar.xaml.cs              Code-behind — PasswordBox login, Enter-key handling
```

---

## Application layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│  NavBar (56 px, pinned top)                                             │
│  ╔════════╗  [Home] [My Profile] [Users*]    [username] [password] [Log In] │
│  ║AppSimple║                                 OR:  Logged in as admin  [Log Out] │
│  ╚════════╝                                                             │
├─────────────────────────────────────────────────────────────────────────┤
│  ContentControl — current page (swaps via DataTemplate)                 │
│  ┌──────────────────────────────────────────────────────────────┐       │
│  │  HomeView / ProfileView / UsersView                          │       │
│  └──────────────────────────────────────────────────────────────┘       │
└─────────────────────────────────────────────────────────────────────────┘
* Users nav item visible to Admin role only
```

---

## Navigation

Page routing is handled by WPF's `DataTemplate` system in `App.xaml`. The
`ContentControl` in `MainWindow` binds to `MainWindowViewModel.CurrentPage`
(a `BaseViewModel`). When the current page changes, WPF automatically selects
the matching `DataTemplate`:

```xml
<!-- App.xaml -->
<DataTemplate DataType="{x:Type vm:HomeViewModel}">
    <views:HomeView/>
</DataTemplate>
<!-- ... etc. -->
```

`MainWindowViewModel` exposes three navigation RelayCommands:

| Command | Guard | Navigates to |
|---|---|---|
| `NavigateToHomeCommand` | always | `HomeViewModel` |
| `NavigateToProfileCommand` | `IsLoggedIn` | `ProfileViewModel` (loads user) |
| `NavigateToUsersCommand` | `IsAdmin` | `UsersViewModel` (loads all users) |

---

## NavBar

The NavBar is a `UserControl` that inherits `DataContext` from `MainWindow`
(which is `MainWindowViewModel`). Its layout has three columns:

| Column | Content | Condition |
|---|---|---|
| Left — app name | "AppSimple" logo text | Always |
| Centre — nav items | Home · My Profile · Users | My Profile: logged in; Users: admin |
| Right — login form | Username TextBox + PasswordBox + Log In button | `IsLoggedIn = false` |
| Right — user info | "Logged in as …" + Log Out button | `IsLoggedIn = true` |

The PasswordBox cannot bind its `Password` property via MVVM for security
reasons. `NavBar.xaml.cs` code-behind reads it and calls
`MainWindowViewModel.LoginAsync(password)`.

---

## MVVM Pattern

All ViewModels extend `BaseViewModel` which extends `CommunityToolkit.Mvvm.ObservableObject`.

| Pattern | Usage |
|---|---|
| `[ObservableProperty]` | Auto-generates property + `PropertyChanged` notification |
| `[NotifyPropertyChangedFor]` | Also notifies dependent computed properties |
| `[RelayCommand]` | Auto-generates `IRelayCommand` property + `CanExecute` support |
| `[RelayCommand(CanExecute = "...")]` | Guards commands — nav buttons auto-disable |

---

## ViewModels

### `BaseViewModel`

| Member | Description |
|---|---|
| `IsBusy` | True during async operations |
| `ErrorMessage` / `HasError` | Active error text |
| `SuccessMessage` / `HasSuccess` | Active success text |
| `SetError(msg)` | Sets error, clears success |
| `SetSuccess(msg)` | Sets success, clears error |
| `ClearMessages()` | Clears both |

### `HomeViewModel`
Stateless landing page. Exposes `IsLoggedIn` and `WelcomeText` read from
`UserSession`. `Refresh()` must be called after login/logout to propagate
changes to the UI.

### `ProfileViewModel`
- `LoadAsync()` — fetches current user from DB, refreshes session + form fields
- `SaveProfileCommand` — persists editable fields; keeps read-only fields unchanged
- `ChangePasswordAsync(current, new, confirm)` — called from code-behind

### `UsersViewModel`
- `Users` — `ObservableCollection<User>` bound to DataGrid
- `LoadAsync()` — reloads all users from DB
- `ShowCreateFormCommand` — shows right panel in Create mode
- `EditSelectedUserCommand` — populates form from selected user
- `DeleteSelectedUserCommand` — deletes selected user (guards system users)
- `CancelFormCommand` — hides right panel
- `SaveFormAsync(password)` — dispatches to create/update depending on `FormMode`

### `MainWindowViewModel`
- Holds references to all page VMs (singletons)
- `IsLoggedIn` / `IsAdmin` — derived from `UserSession`
- `LoginAsync(password)` — called from NavBar code-behind
- `NotifySessionChanged()` — raises PropertyChanged for all session-dependent properties and refreshes CanExecute on guarded commands

---

## Styles (App.xaml)

| Resource key | Type | Description |
|---|---|---|
| `BoolToVisibility` | Converter | `true` → Visible |
| `InverseBoolToVisibility` | Converter | `true` → Collapsed |
| `FormWidthConverter` | Converter | `true` → 320, `false` → 0 (column width) |
| `BackgroundBrush` | Brush | Window background `#1E1E2E` |
| `NavBarBrush` | Brush | Nav bar background `#181825` |
| `CardBrush` | Brush | Card/panel background `#313244` |
| `AccentBrush` | Brush | Accent blue `#89B4FA` |
| `TextBrush` | Brush | Primary text `#CDD6F4` |
| `SuccessBrush` / `ErrorBrush` | Brush | Status colours |
| `NavButtonStyle` | Button | Flat, transparent, hover highlight |
| `PrimaryButtonStyle` | Button | Accent-filled |
| `SecondaryButtonStyle` | Button | Outlined |
| `DangerButtonStyle` | Button | Red, for destructive actions |
| `PageTitleStyle` | TextBlock | 28 px bold heading |
| `SectionHeadingStyle` | TextBlock | 16 px accent-coloured sub-heading |
| `FormLabelStyle` | TextBlock | 13 px muted form label |
| `FormValueStyle` | TextBlock | 14 px form value display |
| `CardStyle` | Border | Dark card with rounded corners |

Implicit styles for `TextBox`, `PasswordBox`, `ComboBox`, `CheckBox`, and
`DataGrid`/`DataGridRow`/`DataGridCell`/`DataGridColumnHeader` provide a consistent
dark-theme appearance without needing third-party libraries.

---

## DI registration (App.xaml.cs)

```csharp
services
    .AddAppLogging(opts => { ... })          // file-only logging
    .AddCoreServices()                        // validators, hasher, user/auth services
    .AddJwtAuthentication(opts => { ... })    // IJwtTokenService
    .AddDataLibServices(connectionString);    // Dapper + SQLite repositories

services.AddSingleton<UserSession>();
services.AddSingleton<HomeViewModel>();
services.AddSingleton<ProfileViewModel>();
services.AddSingleton<UsersViewModel>();
services.AddSingleton<MainWindowViewModel>();
services.AddTransient<MainWindow>();
```

ViewModels are registered as singletons so navigation state (e.g. loaded user
list) is retained when switching pages.

---

## Default credentials

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | Admin |

The admin user is seeded on first startup via `DbInitializer.SeedAdminUser()`.
