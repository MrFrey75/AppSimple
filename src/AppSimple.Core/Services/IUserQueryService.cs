using AppSimple.Core.Models;

namespace AppSimple.Core.Services;

/// <summary>
/// Defines read-only user query operations.
/// </summary>
public interface IUserQueryService
{
    /// <summary>Gets a user by their unique identifier, or <c>null</c> if not found.</summary>
    Task<User?> GetByUidAsync(Guid uid);

    /// <summary>Gets a user by their username (case-insensitive), or <c>null</c> if not found.</summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>Returns all users ordered by username.</summary>
    Task<IEnumerable<User>> GetAllAsync();
}
