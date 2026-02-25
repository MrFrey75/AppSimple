using AppSimple.Core.Enums;
using AppSimple.Core.Models;

namespace AppSimple.Core.Models.DTOs;

/// <summary>
/// Safe user representation returned by the WebApi and deserialized by HTTP clients.
/// Contains no sensitive fields (e.g. no password hash).
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

    /// <summary>Gets or sets the assigned role.</summary>
    public UserRole Role { get; set; }

    /// <summary>Gets or sets a value indicating whether the account is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a system-reserved user.</summary>
    public bool IsSystem { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the account was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Maps a <see cref="User"/> domain model to a <see cref="UserDto"/>.</summary>
    public static UserDto From(User u) => new()
    {
        Uid         = u.Uid,
        Username    = u.Username,
        Email       = u.Email,
        FirstName   = u.FirstName,
        LastName    = u.LastName,
        FullName    = u.FullName,
        PhoneNumber = u.PhoneNumber,
        Bio         = u.Bio,
        DateOfBirth = u.DateOfBirth,
        Role        = u.Role,
        IsActive    = u.IsActive,
        IsSystem    = u.IsSystem,
        CreatedAt   = u.CreatedAt,
    };
}
