using AppSimple.Core.Enums;
using AppSimple.Core.Models;

namespace AppSimple.MvvmApp.Session;

/// <summary>
/// Singleton that holds the authenticated user's session state for the lifetime
/// of the WPF application.
/// </summary>
public class UserSession
{
    private User? _currentUser;
    private string? _token;

    /// <summary>Gets the currently logged-in user, or <c>null</c> if not authenticated.</summary>
    public User? CurrentUser => _currentUser;

    /// <summary>Gets the JWT token issued at login, or <c>null</c> if not authenticated.</summary>
    public string? Token => _token;

    /// <summary>Gets a value indicating whether a user is currently logged in.</summary>
    public bool IsLoggedIn => _currentUser is not null;

    /// <summary>
    /// Stores the authenticated user and JWT token, marking the session as active.
    /// </summary>
    public void Login(User user, string token)
    {
        _currentUser = user;
        _token = token;
    }

    /// <summary>Clears the session, effectively logging the user out.</summary>
    public void Logout()
    {
        _currentUser = null;
        _token = null;
    }

    /// <summary>
    /// Determines whether the currently logged-in user has the specified permission.
    /// </summary>
    public bool HasPermission(Permission permission)
    {
        if (_currentUser is null) return false;

        return permission switch
        {
            Permission.ViewProfile or
            Permission.EditProfile => true,

            Permission.ViewUsers or
            Permission.CreateUser or
            Permission.EditUser or
            Permission.DeleteUser => _currentUser.Role == UserRole.Admin,

            _ => false
        };
    }
}
