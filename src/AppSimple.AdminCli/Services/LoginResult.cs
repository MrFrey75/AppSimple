namespace AppSimple.AdminCli.Services;

/// <summary>Result returned by the WebApi login endpoint.</summary>
public sealed class LoginResult
{
    /// <summary>Gets the JWT bearer token.</summary>
    public required string Token { get; set; }

    /// <summary>Gets the authenticated username.</summary>
    public required string Username { get; set; }

    /// <summary>Gets the user role string ("Admin" or "User").</summary>
    public required string Role { get; set; }
}
