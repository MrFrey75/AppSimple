using AppSimple.Core.Enums;
using AppSimple.Core.Models;

namespace AppSimple.DataLib.Tests.Helpers;

/// <summary>
/// Provides factory methods for building <see cref="User"/> test instances with sensible defaults.
/// </summary>
internal static class UserFactory
{
    private static int _counter;

    /// <summary>
    /// Creates a new <see cref="User"/> with unique username/email and optional overrides.
    /// </summary>
    public static User Create(
        string? username = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null,
        DateTime? dateOfBirth = null,
        string? bio = null,
        string? avatarUrl = null,
        UserRole role = UserRole.User,
        bool isActive = true,
        bool isSystem = false)
    {
        var n = ++_counter;
        var now = DateTime.UtcNow;
        return new User
        {
            Uid = Guid.CreateVersion7(),
            Username = username ?? $"user{n}",
            PasswordHash = "$2a$11$testhashedpassword",
            Email = email ?? $"user{n}@test.com",
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            Bio = bio,
            AvatarUrl = avatarUrl,
            Role = role,
            IsActive = isActive,
            IsSystem = isSystem,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
