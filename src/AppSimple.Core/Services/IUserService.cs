using AppSimple.Core.Models;

namespace AppSimple.Core.Services;

/// <summary>
/// Defines user management operations available in the application.
/// </summary>
public interface IUserService
{
    /// <summary>Gets a user by their unique identifier.</summary>
    Task<User?> GetByUidAsync(Guid uid);

    /// <summary>Gets a user by their username.</summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>Returns all users.</summary>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Creates a new user with a hashed password.
    /// </summary>
    /// <param name="username">The desired username.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="plainPassword">The plain-text password to hash and store.</param>
    /// <returns>The newly created <see cref="User"/>.</returns>
    Task<User> CreateAsync(string username, string email, string plainPassword);

    /// <summary>Updates a user's profile information.</summary>
    Task UpdateAsync(User user);

    /// <summary>Deletes a user by their unique identifier. System users cannot be deleted.</summary>
    Task DeleteAsync(Guid uid);

    /// <summary>Changes a user's password after verifying the current password.</summary>
    Task ChangePasswordAsync(Guid uid, string currentPassword, string newPassword);
}
