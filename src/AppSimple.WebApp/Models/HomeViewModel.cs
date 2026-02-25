namespace AppSimple.WebApp.Models;

/// <summary>View model for the home page.</summary>
public sealed class HomeViewModel
{
    /// <summary>Whether the current user is logged in.</summary>
    public bool IsLoggedIn { get; set; }

    /// <summary>The current user's username, or null if not logged in.</summary>
    public string? Username { get; set; }

    /// <summary>Whether the current user has the Admin role.</summary>
    public bool IsAdmin { get; set; }
}
