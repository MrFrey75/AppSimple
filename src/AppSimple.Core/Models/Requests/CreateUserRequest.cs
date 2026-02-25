namespace AppSimple.Core.Models.Requests;

/// <summary>
/// Request model for creating a new user account.
/// Validated by <c>CreateUserRequestValidator</c>.
/// </summary>
public sealed class CreateUserRequest
{
    /// <summary>Gets or sets the desired login username.</summary>
    public required string Username { get; set; }

    /// <summary>Gets or sets the user's email address.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the plain-text password to hash and store.</summary>
    public required string Password { get; set; }

    /// <summary>Gets or sets the user's optional first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the user's optional last name.</summary>
    public string? LastName { get; set; }
}
