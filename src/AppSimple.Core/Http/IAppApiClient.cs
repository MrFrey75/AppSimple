using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;

namespace AppSimple.Core.Http;

/// <summary>
/// Base interface for typed HTTP clients that communicate with the AppSimple WebApi.
/// Defines the common methods shared by all client projects.
/// Project-specific interfaces should extend this with additional methods.
/// </summary>
public interface IAppApiClient
{
    /// <summary>Authenticates a user and returns a <see cref="LoginResult"/>, or <c>null</c> on failure.</summary>
    Task<LoginResult?> LoginAsync(string username, string password);

    /// <summary>Returns all users. Requires an admin token.</summary>
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(string token);

    /// <summary>Returns a single user by UID, or <c>null</c> if not found.</summary>
    Task<UserDto?> GetUserAsync(string token, Guid uid);

    /// <summary>Creates a new user. Returns the created <see cref="UserDto"/>, or <c>null</c> on failure.</summary>
    Task<UserDto?> CreateUserAsync(string token, string username, string email, string password);

    /// <summary>Updates an existing user. Returns the updated <see cref="UserDto"/>, or <c>null</c> on failure.</summary>
    Task<UserDto?> UpdateUserAsync(string token, Guid uid, UpdateUserRequest request);

    /// <summary>Deletes a user by UID. Returns <c>true</c> on success.</summary>
    Task<bool> DeleteUserAsync(string token, Guid uid);
}
