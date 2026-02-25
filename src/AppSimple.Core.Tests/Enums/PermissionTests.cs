using AppSimple.Core.Enums;

namespace AppSimple.Core.Tests.Enums;

/// <summary>Tests for <see cref="Permission"/> enum values and membership.</summary>
public sealed class PermissionTests
{
    [Theory]
    [InlineData(Permission.ViewProfile, 10)]
    [InlineData(Permission.EditProfile, 11)]
    [InlineData(Permission.ViewUsers,   20)]
    [InlineData(Permission.CreateUser,  21)]
    [InlineData(Permission.EditUser,    22)]
    [InlineData(Permission.DeleteUser,  23)]
    public void Permission_HasExpectedIntValue(Permission permission, int expected)
    {
        Assert.Equal(expected, (int)permission);
    }

    [Fact]
    public void AllExpectedMembers_AreDefined()
    {
        var defined = Enum.GetNames<Permission>();
        Assert.Contains("ViewProfile", defined);
        Assert.Contains("EditProfile", defined);
        Assert.Contains("ViewUsers",   defined);
        Assert.Contains("CreateUser",  defined);
        Assert.Contains("EditUser",    defined);
        Assert.Contains("DeleteUser",  defined);
    }

    [Fact]
    public void ExactlySixMembers_AreDefined()
    {
        Assert.Equal(6, Enum.GetValues<Permission>().Length);
    }

    [Fact]
    public void UserPermissions_HaveLowerValues_ThanAdminPermissions()
    {
        var userPerms   = new[] { Permission.ViewProfile, Permission.EditProfile };
        var adminPerms  = new[] { Permission.ViewUsers, Permission.CreateUser, Permission.EditUser, Permission.DeleteUser };

        var maxUser  = userPerms.Max(p => (int)p);
        var minAdmin = adminPerms.Min(p => (int)p);

        Assert.True(maxUser < minAdmin,
            "User-scoped permissions should have lower integer values than admin-scoped permissions");
    }
}
