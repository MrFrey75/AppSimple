namespace AppSimple.WebApp.Services;

/// <summary>Result returned from the WebApi login endpoint.</summary>
public sealed class LoginResult
{
    /// <summary>JWT bearer token.</summary>
    public required string Token { get; set; }

    /// <summary>Authenticated username.</summary>
    public required string Username { get; set; }

    /// <summary>Role string (e.g. "Admin" or "User").</summary>
    public required string Role { get; set; }
}
