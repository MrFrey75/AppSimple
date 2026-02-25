namespace AppSimple.WebApi.DTOs;

/// <summary>Response body returned after a successful login.</summary>
public sealed class LoginResponse
{
    /// <summary>Gets or sets the JWT bearer token.</summary>
    public required string Token { get; set; }

    /// <summary>Gets or sets the authenticated username.</summary>
    public required string Username { get; set; }

    /// <summary>Gets or sets the role of the authenticated user.</summary>
    public required string Role { get; set; }
}
