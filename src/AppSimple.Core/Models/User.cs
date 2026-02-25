using AppSimple.Core.Enums;

namespace AppSimple.Core.Models;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : BaseEntity
{
    /// <summary>Gets or sets the unique username used for login.</summary>
    public required string Username { get; set; }

    /// <summary>Gets or sets the BCrypt-hashed password for the user.</summary>
    public required string PasswordHash { get; set; }

    /// <summary>Gets or sets the email address of the user.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the user's first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the user's last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the user's phone number (e.g., +1-555-000-0000).</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets the user's date of birth in UTC.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets a short biography or description provided by the user.</summary>
    public string? Bio { get; set; }

    /// <summary>Gets or sets the URL or relative path to the user's profile picture.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the role assigned to the user.</summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>Gets or sets a value indicating whether this user account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets the user's full name, combining <see cref="FirstName"/> and <see cref="LastName"/>.</summary>
    public string? FullName =>
        string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? null
            : $"{FirstName} {LastName}".Trim();
}
