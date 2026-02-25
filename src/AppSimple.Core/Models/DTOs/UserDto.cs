namespace AppSimple.Core.Models.DTOs;

/// <summary>
/// Safe user representation returned by the WebApi.
/// Used by client-side projects (WebApp, AdminCli) to deserialize API responses.
/// </summary>
public sealed class UserDto
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Uid { get; set; }

    /// <summary>Gets or sets the username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the full name (computed by the API).</summary>
    public string? FullName { get; set; }

    /// <summary>Gets or sets the phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets the biography.</summary>
    public string? Bio { get; set; }

    /// <summary>Gets or sets the date of birth.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets the role (0 = User, 1 = Admin).</summary>
    public int Role { get; set; }

    /// <summary>Gets or sets a value indicating whether the account is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a system-reserved user.</summary>
    public bool IsSystem { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the account was created.</summary>
    public DateTime CreatedAt { get; set; }
}
