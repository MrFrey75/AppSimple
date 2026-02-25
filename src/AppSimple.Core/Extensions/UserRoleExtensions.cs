using AppSimple.Core.Enums;

namespace AppSimple.Core.Extensions;

/// <summary>
/// Extension methods for the <see cref="UserRole"/> enum.
/// </summary>
public static class UserRoleExtensions
{
    /// <summary>
    /// Determines whether the given <paramref name="role"/> is granted the specified <paramref name="permission"/>.
    /// </summary>
    /// <param name="role">The user's role.</param>
    /// <param name="permission">The permission to check.</param>
    /// <returns><c>true</c> if the role is permitted; otherwise <c>false</c>.</returns>
    public static bool HasPermission(this UserRole role, Permission permission) =>
        permission switch
        {
            Permission.ViewProfile or
            Permission.EditProfile  => true,

            Permission.ViewUsers  or
            Permission.CreateUser or
            Permission.EditUser   or
            Permission.DeleteUser => role == UserRole.Admin,

            _ => false
        };
}
