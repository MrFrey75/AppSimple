using AppSimple.Core.Models;

namespace AppSimple.Core.Services;

/// <summary>
/// Defines write-only user command operations.
/// </summary>
public interface IUserCommandService
{
    /// <summary>
    /// Creates a new user with a hashed password.
    /// </summary>
    /// <param name="username">The desired login username.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="plainPassword">The plain-text password to hash and store.</param>
    /// <returns>The newly created <see cref="User"/>.</returns>
    /// <exception cref="AppSimple.Core.Common.Exceptions.DuplicateEntityException">
    /// Thrown if the username or email is already taken.
    /// </exception>
    Task<User> CreateAsync(string username, string email, string plainPassword);

    /// <summary>
    /// Persists changes to an existing user's profile.
    /// </summary>
    /// <exception cref="AppSimple.Core.Common.Exceptions.EntityNotFoundException">
    /// Thrown if the user does not exist.
    /// </exception>
    /// <exception cref="AppSimple.Core.Common.Exceptions.SystemEntityException">
    /// Thrown if the user is a system-reserved record.
    /// </exception>
    Task UpdateAsync(User user);

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <exception cref="AppSimple.Core.Common.Exceptions.EntityNotFoundException">
    /// Thrown if the user does not exist.
    /// </exception>
    /// <exception cref="AppSimple.Core.Common.Exceptions.SystemEntityException">
    /// Thrown if the user is a system-reserved record.
    /// </exception>
    Task DeleteAsync(Guid uid);

    /// <summary>
    /// Changes a user's password after verifying the current one.
    /// </summary>
    /// <exception cref="AppSimple.Core.Common.Exceptions.EntityNotFoundException">
    /// Thrown if the user does not exist.
    /// </exception>
    /// <exception cref="AppSimple.Core.Common.Exceptions.UnauthorizedException">
    /// Thrown if <paramref name="currentPassword"/> is incorrect.
    /// </exception>
    Task ChangePasswordAsync(Guid uid, string currentPassword, string newPassword);
}
