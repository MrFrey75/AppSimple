using AppSimple.Core.Models;

namespace AppSimple.Core.Interfaces;

/// <summary>
/// Repository interface for <see cref="User"/> entities, extending the generic repository with user-specific queries.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>Finds a user by their username (case-insensitive).</summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>Finds a user by their email address (case-insensitive).</summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Checks whether a username is already taken.</summary>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>Checks whether an email address is already registered.</summary>
    Task<bool> EmailExistsAsync(string email);
}
