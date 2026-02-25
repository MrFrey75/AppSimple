using AppSimple.Core.Enums;
using AppSimple.Core.Models;

namespace AppSimple.WebApi.DTOs;

/// <summary>Safe user representation returned by the API (no password hash).</summary>
public sealed class UserDto
{
    /// <summary>Gets the unique identifier of the user.</summary>
    public Guid Uid { get; set; }

    /// <summary>Gets the username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets the email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets the first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets the last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets the full name.</summary>
    public string? FullName { get; set; }

    /// <summary>Gets the phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets the biography.</summary>
    public string? Bio { get; set; }

    /// <summary>Gets the date of birth.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gets the assigned role.</summary>
    public UserRole Role { get; set; }

    /// <summary>Gets a value indicating whether the account is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets the UTC timestamp when the account was created.</summary>
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
        CreatedAt   = u.CreatedAt,
    };
}
