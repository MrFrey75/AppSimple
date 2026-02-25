using AppSimple.Core.Enums;
using AppSimple.Core.Extensions;

namespace AppSimple.Core.Tests.Extensions;

/// <summary>Tests for <see cref="UserRoleExtensions"/>.</summary>
public sealed class UserRoleExtensionsTests
{
    // ── ViewProfile / EditProfile ─────────────────────────────────────────────

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.User)]
    public void ViewProfile_IsGrantedToAllRoles(UserRole role)
    {
        Assert.True(role.HasPermission(Permission.ViewProfile));
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.User)]
    public void EditProfile_IsGrantedToAllRoles(UserRole role)
    {
        Assert.True(role.HasPermission(Permission.EditProfile));
    }

    // ── Admin-only permissions ────────────────────────────────────────────────

    [Theory]
    [InlineData(Permission.ViewUsers)]
    [InlineData(Permission.CreateUser)]
    [InlineData(Permission.EditUser)]
    [InlineData(Permission.DeleteUser)]
    public void AdminPermissions_GrantedToAdmin(Permission permission)
    {
        Assert.True(UserRole.Admin.HasPermission(permission));
    }

    [Theory]
    [InlineData(Permission.ViewUsers)]
    [InlineData(Permission.CreateUser)]
    [InlineData(Permission.EditUser)]
    [InlineData(Permission.DeleteUser)]
    public void AdminPermissions_DeniedToUser(Permission permission)
    {
        Assert.False(UserRole.User.HasPermission(permission));
    }

    // ── Unknown permission ────────────────────────────────────────────────────

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.User)]
    public void UnknownPermission_ReturnsFalse(UserRole role)
    {
        Assert.False(role.HasPermission((Permission)999));
    }
}
