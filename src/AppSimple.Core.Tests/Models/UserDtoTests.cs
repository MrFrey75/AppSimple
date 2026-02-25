using AppSimple.Core.Enums;
using AppSimple.Core.Models.DTOs;

namespace AppSimple.Core.Tests.Models;

/// <summary>Tests for the <see cref="UserDto"/> DTO.</summary>
public sealed class UserDtoTests
{
    [Fact]
    public void UserDto_DefaultValues_AreExpected()
    {
        var dto = new UserDto();

        Assert.Equal(Guid.Empty,    dto.Uid);
        Assert.Equal(string.Empty,  dto.Username);
        Assert.Equal(string.Empty,  dto.Email);
        Assert.Null(dto.FirstName);
        Assert.Null(dto.LastName);
        Assert.Null(dto.FullName);
        Assert.Null(dto.PhoneNumber);
        Assert.Null(dto.Bio);
        Assert.Null(dto.DateOfBirth);
        Assert.Equal(UserRole.User, dto.Role);
        Assert.False(dto.IsActive);
        Assert.False(dto.IsSystem);
        Assert.Equal(default,       dto.CreatedAt);
    }

    [Fact]
    public void UserDto_Properties_SetAndGet_Correctly()
    {
        var uid = Guid.CreateVersion7();
        var now = DateTime.UtcNow;

        var dto = new UserDto
        {
            Uid         = uid,
            Username    = "bob",
            Email       = "bob@example.com",
            FirstName   = "Bob",
            LastName    = "Smith",
            FullName    = "Bob Smith",
            PhoneNumber = "+1-555-0100",
            Bio         = "Developer",
            DateOfBirth = new DateTime(1990, 1, 1),
            Role        = UserRole.Admin,
            IsActive    = true,
            IsSystem    = false,
            CreatedAt   = now,
        };

        Assert.Equal(uid,                    dto.Uid);
        Assert.Equal("bob",                  dto.Username);
        Assert.Equal("bob@example.com",      dto.Email);
        Assert.Equal("Bob",                  dto.FirstName);
        Assert.Equal("Smith",                dto.LastName);
        Assert.Equal("Bob Smith",            dto.FullName);
        Assert.Equal("+1-555-0100",          dto.PhoneNumber);
        Assert.Equal("Developer",            dto.Bio);
        Assert.Equal(new DateTime(1990,1,1), dto.DateOfBirth);
        Assert.Equal(UserRole.Admin,         dto.Role);
        Assert.True(dto.IsActive);
        Assert.False(dto.IsSystem);
        Assert.Equal(now,                    dto.CreatedAt);
    }

    [Fact]
    public void UserDto_Role_DefaultIsUser()
    {
        Assert.Equal(UserRole.User, new UserDto().Role);
    }

    [Fact]
    public void UserDto_Role_CanBeSetToAdmin()
    {
        var dto = new UserDto { Role = UserRole.Admin };
        Assert.Equal(UserRole.Admin, dto.Role);
    }
}
