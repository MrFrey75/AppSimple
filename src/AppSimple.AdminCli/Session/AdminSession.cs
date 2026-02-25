namespace AppSimple.AdminCli.Session;

/// <summary>Holds the current admin session state (token and username).</summary>
public sealed class AdminSession
{
    private string? _token;
    private string? _username;

    /// <summary>Gets the bearer token for authenticated API calls.</summary>
    public string? Token => _token;

    /// <summary>Gets the username of the logged-in admin.</summary>
    public string? Username => _username;

    /// <summary>Gets a value indicating whether an admin is currently logged in.</summary>
    public bool IsLoggedIn => _token is not null;

    /// <summary>Stores the session credentials after a successful login.</summary>
    public void Login(string token, string username)
    {
        _token    = token;
        _username = username;
    }

    /// <summary>Clears the session credentials.</summary>
    public void Logout()
    {
        _token    = null;
        _username = null;
    }
}
