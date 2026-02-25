using AppSimple.MvvmApp.Session;

namespace AppSimple.MvvmApp.ViewModels;

/// <summary>
/// ViewModel for the public landing page. Reflects session state but has no auth requirements.
/// </summary>
public partial class HomeViewModel : BaseViewModel
{
    private readonly UserSession _session;

    /// <summary>Initializes a new instance of <see cref="HomeViewModel"/>.</summary>
    public HomeViewModel(UserSession session)
    {
        _session = session;
    }

    /// <summary>Gets a value indicating whether a user is currently logged in.</summary>
    public bool IsLoggedIn => _session.IsLoggedIn;

    /// <summary>Gets a contextual welcome string shown when logged in.</summary>
    public string WelcomeText => _session.IsLoggedIn
        ? $"Welcome back, {_session.CurrentUser!.Username}!"
        : string.Empty;

    /// <summary>
    /// Refreshes computed properties after a login or logout event.
    /// Called by <see cref="MainWindowViewModel"/> when session state changes.
    /// </summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(WelcomeText));
    }
}
