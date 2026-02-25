using AppSimple.Core.Enums;
using AppSimple.Core.Models;

namespace AppSimple.Core.Tests.Models;

/// <summary>Tests for the <see cref="User"/> model defaults, required fields, and computed properties.</summary>
public sealed class UserTests
{
    // -------------------------------------------------------------------------
    // Defaults
    // -------------------------------------------------------------------------

    [Fact]
    public void Role_DefaultsTo_User()
    {
        var user = MakeUser();
        Assert.Equal(UserRole.User, user.Role);
    }

    [Fact]
    public void IsActive_DefaultsTo_True()
    {
        var user = MakeUser();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void IsSystem_DefaultsTo_False()
    {
        var user = MakeUser();
        Assert.False(user.IsSystem);
    }

    [Fact]
    public void NullableProfileFields_DefaultTo_Null()
    {
        var user = MakeUser();
        Assert.Null(user.FirstName);
        Assert.Null(user.LastName);
        Assert.Null(user.PhoneNumber);
        Assert.Null(user.DateOfBirth);
        Assert.Null(user.Bio);
        Assert.Null(user.AvatarUrl);
    }

    [Fact]
    public void RequiredFields_CanBeSetAndRead()
    {
        var user = MakeUser(username: "alice", email: "alice@example.com", passwordHash: "hash123");
        Assert.Equal("alice", user.Username);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("hash123", user.PasswordHash);
    }

    // -------------------------------------------------------------------------
    // FullName computed property
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("Jane", "Doe", "Jane Doe")]
    [InlineData("Jane", null, "Jane")]
    [InlineData(null, "Doe", "Doe")]
    [InlineData(null, null, null)]
    [InlineData("", "", null)]
    [InlineData("  ", "  ", null)]
    [InlineData("Jane", "  ", "Jane")]
    [InlineData("  ", "Doe", "Doe")]
    public void FullName_ReturnsExpected(string? first, string? last, string? expected)
    {
        var user = MakeUser(firstName: first, lastName: last);
        Assert.Equal(expected, user.FullName);
    }

    [Fact]
    public void FullName_IsReadOnly_DoesNotHaveSetter()
    {
        var prop = typeof(User).GetProperty(nameof(User.FullName));
        Assert.NotNull(prop);
        Assert.Null(prop!.GetSetMethod());
    }

    // -------------------------------------------------------------------------
    // Inheritance from BaseEntity
    // -------------------------------------------------------------------------

    [Fact]
    public void User_InheritsFrom_BaseEntity()
    {
        Assert.True(typeof(User).IsSubclassOf(typeof(BaseEntity)));
    }

    [Fact]
    public void User_Uid_IsVersion7Guid()
    {
        var user = MakeUser();
        var bytes = user.Uid.ToByteArray();
        var version = (bytes[7] >> 4) & 0xF;
        Assert.Equal(7, version);
    }

    // -------------------------------------------------------------------------
    // Profile field round-trips
    // -------------------------------------------------------------------------

    [Fact]
    public void ProfileFields_CanBeSetAndRead()
    {
        var dob = new DateTime(1990, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        var user = MakeUser();
        user.FirstName = "Alice";
        user.LastName = "Smith";
        user.PhoneNumber = "+1-555-123-4567";
        user.DateOfBirth = dob;
        user.Bio = "Software engineer";
        user.AvatarUrl = "/avatars/alice.png";

        Assert.Equal("Alice", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.Equal("+1-555-123-4567", user.PhoneNumber);
        Assert.Equal(dob, user.DateOfBirth);
        Assert.Equal("Software engineer", user.Bio);
        Assert.Equal("/avatars/alice.png", user.AvatarUrl);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static User MakeUser(
        string username = "testuser",
        string email = "test@example.com",
        string passwordHash = "$2a$11$hash",
        string? firstName = null,
        string? lastName = null) => new()
    {
        Username = username,
        Email = email,
        PasswordHash = passwordHash,
        FirstName = firstName,
        LastName = lastName
    };
}
