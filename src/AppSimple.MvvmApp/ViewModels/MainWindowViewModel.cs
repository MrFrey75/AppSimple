using AppSimple.Core.Enums;
using AppSimple.Core.Services;
using AppSimple.MvvmApp.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSimple.MvvmApp.ViewModels;

/// <summary>
/// Root ViewModel bound to <see cref="AppSimple.MvvmApp.MainWindow"/>.
/// Owns navigation state, login/logout logic, and the current page.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IAuthService    _auth;
    private readonly IUserService    _users;
    private readonly UserSession     _session;
    private readonly HomeViewModel   _homeVm;
    private readonly ProfileViewModel _profileVm;
    private readonly UsersViewModel  _usersVm;

    // ─── Observable state ──────────────────────────────────────────────────

    /// <summary>Gets or sets the page ViewModel currently displayed in the content area.</summary>
    [ObservableProperty] private BaseViewModel _currentPage = null!;

    /// <summary>Gets or sets the username text entered in the nav-bar login form.</summary>
    [ObservableProperty] private string _loginUsername = string.Empty;

    /// <summary>Gets or sets the password text entered in the nav-bar login form.</summary>
    [ObservableProperty] private string _loginPassword = string.Empty;

    /// <summary>Gets or sets the error message shown when login fails.</summary>
    [ObservableProperty] private string _loginError = string.Empty;

    /// <summary>Gets or sets a value indicating whether a login operation is in progress.</summary>
    [ObservableProperty] private bool _isLoggingIn;

    // ─── Computed ──────────────────────────────────────────────────────────

    /// <summary>Gets a value indicating whether a user is logged in.</summary>
    public bool IsLoggedIn => _session.IsLoggedIn;

    /// <summary>Gets a value indicating whether the current user has Admin role.</summary>
    public bool IsAdmin => _session.IsLoggedIn && _session.CurrentUser!.Role == UserRole.Admin;

    /// <summary>Gets the welcome string displayed in the nav bar when logged in.</summary>
    public string WelcomeText => _session.IsLoggedIn
        ? $"Logged in as  {_session.CurrentUser!.Username}"
        : string.Empty;

    // ─── Constructor ───────────────────────────────────────────────────────

    /// <summary>Initializes a new instance of <see cref="MainWindowViewModel"/>.</summary>
    public MainWindowViewModel(
        IAuthService auth,
        IUserService users,
        UserSession session,
        HomeViewModel homeVm,
        ProfileViewModel profileVm,
        UsersViewModel usersVm)
    {
        _auth      = auth;
        _users     = users;
        _session   = session;
        _homeVm    = homeVm;
        _profileVm = profileVm;
        _usersVm   = usersVm;
        _currentPage = homeVm;
    }

    // ─── Navigation commands ───────────────────────────────────────────────

    /// <summary>Navigates to the Home page.</summary>
    [RelayCommand]
    private void NavigateToHome()
    {
        _homeVm.Refresh();
        CurrentPage = _homeVm;
    }

    /// <summary>Navigates to the Profile page (requires login).</summary>
    [RelayCommand(CanExecute = nameof(IsLoggedIn))]
    private async Task NavigateToProfile()
    {
        await _profileVm.LoadAsync();
        CurrentPage = _profileVm;
    }

    /// <summary>Navigates to the User Management page (requires Admin role).</summary>
    [RelayCommand(CanExecute = nameof(IsAdmin))]
    private async Task NavigateToUsers()
    {
        await _usersVm.LoadAsync();
        CurrentPage = _usersVm;
    }

    // ─── Auth ──────────────────────────────────────────────────────────────

    /// <summary>Logs the current user out and returns to the home page.</summary>
    [RelayCommand(CanExecute = nameof(IsLoggedIn))]
    private void Logout()
    {
        _session.Logout();
        LoginUsername = string.Empty;
        LoginPassword = string.Empty;
        LoginError    = string.Empty;
        NotifySessionChanged();
        _homeVm.Refresh();
        CurrentPage = _homeVm;
    }

    /// <summary>
    /// Authenticates the user using <see cref="LoginUsername"/> and <see cref="LoginPassword"/>.
    /// </summary>
    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(LoginUsername) || string.IsNullOrWhiteSpace(LoginPassword))
        {
            LoginError = "Username and password are required.";
            return;
        }

        IsLoggingIn = true;
        LoginError  = string.Empty;

        try
        {
            var result = await _auth.LoginAsync(LoginUsername, LoginPassword);

            if (result.Succeeded && result.Token is not null)
            {
                var user = await _users.GetByUsernameAsync(LoginUsername);
                if (user is not null)
                {
                    _session.Login(user, result.Token);
                    LoginUsername = string.Empty;
                    LoginPassword = string.Empty;
                    LoginError    = string.Empty;
                    NotifySessionChanged();
                    _homeVm.Refresh();
                    NavigateToHomeCommand.Execute(null);
                    return;
                }
            }

            LoginError = result.Message;
        }
        catch (Exception ex)
        {
            LoginError = ex.Message;
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    /// <summary>
    /// Authenticates the user. Kept for backward-compatibility with code-behind callers.
    /// </summary>
    public Task LoginAsync(string password)
    {
        LoginPassword = password;
        return Login();
    }

    // ─── Private helpers ───────────────────────────────────────────────────

    private void NotifySessionChanged()
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(WelcomeText));
        NavigateToProfileCommand.NotifyCanExecuteChanged();
        NavigateToUsersCommand.NotifyCanExecuteChanged();
        LogoutCommand.NotifyCanExecuteChanged();
    }
}
