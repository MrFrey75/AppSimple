using AppSimple.Core.Auth;
using AppSimple.Core.Common.Exceptions;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;

namespace AppSimple.Core.Services.Impl;

/// <summary>
/// Provides user management operations backed by <see cref="IUserRepository"/>.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAppLogger<UserService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="UserService"/>.
    /// </summary>
    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAppLogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger         = logger;
    }

    /// <inheritdoc />
    public async Task<User?> GetByUidAsync(Guid uid)
        => await _userRepository.GetByUidAsync(uid);

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(string username)
        => await _userRepository.GetByUsernameAsync(username);

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync()
        => await _userRepository.GetAllAsync();

    /// <inheritdoc />
    /// <exception cref="DuplicateEntityException">Thrown if the username or email is already taken.</exception>
    public async Task<User> CreateAsync(string username, string email, string plainPassword)
    {
        if (await _userRepository.UsernameExistsAsync(username))
            throw new DuplicateEntityException("Username", username);

        if (await _userRepository.EmailExistsAsync(email))
            throw new DuplicateEntityException("Email", email);

        var now = DateTime.UtcNow;
        var user = new User
        {
            Uid          = Guid.CreateVersion7(),
            Username     = username,
            Email        = email,
            PasswordHash = _passwordHasher.Hash(plainPassword),
            CreatedAt    = now,
            UpdatedAt    = now
        };

        await _userRepository.AddAsync(user);
        _logger.Information("User {Username} created with uid {Uid}.", username, user.Uid);

        return user;
    }

    /// <inheritdoc />
    /// <exception cref="EntityNotFoundException">Thrown if the user does not exist.</exception>
    /// <exception cref="SystemEntityException">Thrown if the user is a system entity.</exception>
    public async Task UpdateAsync(User user)
    {
        var existing = await _userRepository.GetByUidAsync(user.Uid)
            ?? throw new EntityNotFoundException(nameof(User), user.Uid);

        if (existing.IsSystem)
            throw new SystemEntityException(nameof(User));

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        _logger.Information("User {Uid} updated.", user.Uid);
    }

    /// <inheritdoc />
    /// <exception cref="EntityNotFoundException">Thrown if the user does not exist.</exception>
    /// <exception cref="SystemEntityException">Thrown if the user is a system entity.</exception>
    public async Task DeleteAsync(Guid uid)
    {
        var existing = await _userRepository.GetByUidAsync(uid)
            ?? throw new EntityNotFoundException(nameof(User), uid);

        if (existing.IsSystem)
            throw new SystemEntityException(nameof(User));

        await _userRepository.DeleteAsync(uid);
        _logger.Information("User {Uid} deleted.", uid);
    }

    /// <inheritdoc />
    /// <exception cref="EntityNotFoundException">Thrown if the user does not exist.</exception>
    /// <exception cref="UnauthorizedException">Thrown if <paramref name="currentPassword"/> is incorrect.</exception>
    public async Task ChangePasswordAsync(Guid uid, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByUidAsync(uid)
            ?? throw new EntityNotFoundException(nameof(User), uid);

        if (!_passwordHasher.Verify(currentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.UpdatedAt    = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        _logger.Information("Password changed for user {Uid}.", uid);
    }
}
