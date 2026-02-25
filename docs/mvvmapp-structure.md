# AppSimple.MvvmApp — Source Map

Cross-platform MVVM desktop application built with **Avalonia UI 11.3.12** targeting `net10.0`.
References `AppSimple.Core` and `AppSimple.DataLib` **directly** — no WebApi layer required.

> **Platform**: Windows · macOS · Linux (via Avalonia UI)

## Project file

`AppSimple.MvvmApp.csproj`

### NuGet dependencies

| Package | Purpose |
|---|---|
| `Avalonia` 11.3.12 | Core Avalonia UI framework |
| `Avalonia.Desktop` 11.3.12 | Desktop platform support (Windows/macOS/Linux) |
| `Avalonia.Themes.Fluent` 11.3.12 | Fluent Design theme |
| `Avalonia.Controls.DataGrid` 11.3.12 | DataGrid control |
| `Avalonia.Fonts.Inter` 11.3.12 | Inter font family |
| `CommunityToolkit.Mvvm` 8.x | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` |
| `Microsoft.Extensions.Hosting` 10.x | DI container, IConfiguration |

---

## Directory layout

```
AppSimple.MvvmApp/
├── Program.cs                      Avalonia AppBuilder entry point
├── App.axaml / App.axaml.cs        Application — DI wiring, DB init, DataTemplates, styles
├── MainWindow.axaml / .axaml.cs    Shell window — NavBar (top) + sidebar + ContentControl
├── appsettings.json                DB connection string config
├── Extensions/
│   └── MvvmAppServiceExtensions.cs AddMvvmAppServices() DI extension
├── Session/
│   └── UserSession.cs              Singleton session state (CurrentUser, Token, HasPermission)
├── Converters/
│   └── InverseBoolConverter.cs     bool → !bool (for IsVisible bindings)
├── ViewModels/
│   ├── BaseViewModel.cs            ObservableObject + IsBusy/Error/Success helpers
│   ├── HomeViewModel.cs            Landing page — IsLoggedIn, WelcomeText, Refresh()
│   ├── ProfileViewModel.cs         Own-profile view/edit + ChangePasswordCommand
│   ├── UsersViewModel.cs           Admin user management — ObservableCollection, CRUD
│   └── MainWindowViewModel.cs      Navigation + login/logout + LoginCommand (bound)
├── Views/
│   ├── HomeView.axaml / .axaml.cs  Public landing page
│   ├── ProfileView.axaml / .cs     Profile form — password fields bind directly (no code-behind)
│   └── UsersView.axaml / .cs       DataGrid + right-panel form — password field bound via MVVM
└── Controls/
    ├── NavBar.axaml                Top navigation bar UserControl
    └── NavBar.axaml.cs             Code-behind — Enter-key handling only
```

---

## Application layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│  NavBar (56 px, pinned top)                                             │
│  AppSimple               [username] [password •••] [Log In]             │
│                          OR:  👤 admin  Admin       [Log Out]           │
├─────────────┬───────────────────────────────────────────────────────────┤
│  Left       │  ContentControl — current page (swaps via DataTemplate)   │
│  Sidebar    │  ┌──────────────────────────────────────────────────────┐ │
│  (logged-in │  │  HomeView / ProfileView / UsersView                  │ │
│   only)     │  └──────────────────────────────────────────────────────┘ │
│  🏠 Home    │                                                            │
│  👤 Profile │                                                            │
│  👥 Users*  │                                                            │
└─────────────┴───────────────────────────────────────────────────────────┘
* Users nav item visible to Admin role only
```

---

## Navigation

Page routing is handled by Avalonia's `DataTemplate` system in `App.axaml`. The
`ContentControl` in `MainWindow` binds to `MainWindowViewModel.CurrentPage`
(a `BaseViewModel`). When the current page changes, Avalonia automatically selects
the matching `DataTemplate`:

```xml
<!-- App.axaml -->
<Application.DataTemplates>
    <DataTemplate DataType="{x:Type vm:HomeViewModel}">
        <v:HomeView />
    </DataTemplate>
    <!-- ... etc. -->
</Application.DataTemplates>
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
| Right — login form | Username TextBox + Password TextBox (`PasswordChar="•"`) + Log In button | `IsLoggedIn = false` |
| Right — user chip | Username + role badge + Log Out button | `IsLoggedIn = true` |

> **Avalonia vs WPF**: Avalonia's `TextBox` with `PasswordChar="•"` supports full MVVM
> binding (unlike WPF's `PasswordBox`). No code-behind password handling is needed.
> `LoginPassword` is an `[ObservableProperty]` on `MainWindowViewModel`.

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
| `StatusMessage` / `HasMessage` | Active success text |
| `SetError(msg)` | Sets error, clears success |
| `SetSuccess(msg)` | Sets success, clears error |
| `ClearMessages()` | Clears both |

### `HomeViewModel`
Stateless landing page. Exposes `IsLoggedIn` and `WelcomeText` read from
`UserSession`. `Refresh()` must be called after login/logout to propagate
changes to the UI.

### `ProfileViewModel`
- `LoadAsync()` — fetches current user from DB, refreshes session + form fields
- `SaveProfileCommand` — persists editable fields
- `ChangePasswordCommand` — uses `CurrentPassword`, `NewPassword`, `ConfirmPassword` properties (all bound via `TextBox PasswordChar`)

### `UsersViewModel`
- `Users` — `ObservableCollection<User>` bound to DataGrid
- `LoadAsync()` — reloads all users from DB
- `ShowCreateFormCommand` — shows right panel in Create mode
- `EditSelectedUserCommand` — populates form from selected user
- `DeleteSelectedUserCommand` — deletes selected user (guards system users)
- `CancelFormCommand` — hides right panel
- `SaveFormCommand` — uses `FormPassword` property (bound via `TextBox PasswordChar`); dispatches to create/update depending on `FormMode`

### `MainWindowViewModel`
- Holds references to all page VMs (singletons)
- `IsLoggedIn` / `IsAdmin` — derived from `UserSession`
- `LoginUsername` / `LoginPassword` — observable properties bound to NavBar TextBoxes
- `LoginCommand` — parameterless RelayCommand; reads `LoginUsername`/`LoginPassword`
- `NotifySessionChanged()` — raises PropertyChanged for all session-dependent properties

---

## Themes (App.axaml + Themes/)

Supports 5 runtime-switchable themes. The selected theme is persisted to `~/.local/share/AppSimple/config.json` (shared with all AppSimple apps). Users switch via a `ComboBox` in the NavBar.

| Theme | Style |
|-------|-------|
| Catppuccin Mocha | Dark, blue accent — default |
| Catppuccin Latte | Light, Catppuccin palette |
| Dracula | Dark, purple accent |
| Nord | Dark, cool cyan accent |
| Solarized Light | Light, warm background |

Each theme is a `ResourceDictionary` in `Themes/ThemeName.axaml` providing 17 `ThemeXxxBrush` keys. `ThemeManager` swaps `Application.Resources.MergedDictionaries[0]` at runtime — `DynamicResource` bindings in `App.axaml` styles and individual views update automatically.

| Resource Key | Usage |
|---|---|
| `ThemeWindowBrush` | Window / page background |
| `ThemeSurfaceBrush` | Card backgrounds |
| `ThemeNavBarBrush` | Nav bar background |
| `ThemeOverlayBrush` | Input/control backgrounds |
| `ThemeBorderBrush` | Borders |
| `ThemeTextBrush` | Primary text |
| `ThemeSubtextBrush` | Labels, secondary text |
| `ThemeAccentBrush` | Accent colour |
| `ThemeErrorBrush` | Error / danger |
| `ThemeSuccessBrush` | Success / green |
| `ThemeButtonBrush` / `ThemeButtonHoverBrush` | Default button states |
| `ThemeDataGridBgBrush` / `ThemeDataGridSelBrush` | DataGrid |
| `ThemeNavButtonHoverBrush` | Sidebar nav buttons |

Theme-related files:
- `Themes/` — 5 AXAML ResourceDictionary files
- `Services/ThemeManager.cs` — applies saved theme on startup; switches at runtime
- Core: `IAppConfigService` — reads/writes `config.json`

---

## DI registration (App.axaml.cs → `AddMvvmAppServices`)

```csharp
services.AddCoreServices();                // validators, hasher, user/auth services
services.AddDataLibServices(connStr);      // Dapper + SQLite repositories
services.AddMvvmAppServices();             // session, ViewModels, MainWindow
```

ViewModels are registered as singletons so navigation state (e.g. loaded user
list) is retained when switching pages.

---

## Default credentials

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | Admin |

The admin user is seeded on first startup via `DbInitializer.SeedAdminUser()`.


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
