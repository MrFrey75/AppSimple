namespace AppSimple.Core.Enums;

/// <summary>
/// Defines the granular permissions available in the system.
/// </summary>
public enum Permission
{
    /// <summary>View own profile.</summary>
    ViewProfile = 10,

    /// <summary>Edit own profile.</summary>
    EditProfile = 11,

    /// <summary>View all users (Admin).</summary>
    ViewUsers = 20,

    /// <summary>Create users (Admin).</summary>
    CreateUser = 21,

    /// <summary>Edit any user (Admin).</summary>
    EditUser = 22,

    /// <summary>Delete any user (Admin).</summary>
    DeleteUser = 23
}
