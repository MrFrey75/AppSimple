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
    private readonly ITagRepository  _tagRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAppLogger<UserService> _logger;

    private static readonly IReadOnlyList<(string Name, string Color)> _defaultTags =
    [
        ("Default",   "#CCCCCC"),
        ("Personal",  "#A8E6A3"),
        ("Work",      "#4A9EFF"),
        ("Important", "#FF6B6B"),
        ("Later",     "#FFD93D"),
        ("Archive",   "#B0B0B0"),
        ("Shared",    "#96CEB4"),
        ("Private",   "#C7A8FF"),
        ("Urgent",    "#FF4444"),
        ("Follow-up", "#FFB347"),
    ];

    /// <summary>
    /// Initializes a new instance of <see cref="UserService"/>.
    /// </summary>
    public UserService(
        IUserRepository         userRepository,
        ITagRepository          tagRepository,
        IPasswordHasher         passwordHasher,
        IAppLogger<UserService> logger)
    {
        _userRepository = userRepository;
        _tagRepository  = tagRepository;
        _passwordHasher = passwordHasher;
        _logger         = logger;
    }

    /// <inheritdoc />
    public async Task<User?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _userRepository.GetByUidAsync(uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving user {Uid}.", uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            return await _userRepository.GetByUsernameAsync(username);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving user '{Username}'.", username);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _userRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving all users.");
            throw;
        }
    }

    /// <inheritdoc />
    /// <exception cref="DuplicateEntityException">Thrown if the username or email is already taken.</exception>
    public async Task<User> CreateAsync(string username, string email, string plainPassword)
    {
        try
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
                UpdatedAt    = now,
            };

            await _userRepository.AddAsync(user);
            _logger.Information("User '{Username}' created with uid {Uid}.", username, user.Uid);

            // Seed default tags for the new user
            var tagNow = DateTime.UtcNow;
            foreach (var (name, color) in _defaultTags)
            {
                await _tagRepository.AddAsync(new Tag
                {
                    Uid       = Guid.CreateVersion7(),
                    UserUid   = user.Uid,
                    Name      = name,
                    Color     = color,
                    IsSystem  = true,
                    CreatedAt = tagNow,
                    UpdatedAt = tagNow,
                });
            }
            _logger.Information("Default tags seeded for user {Uid}.", user.Uid);

            return user;
        }
        catch (DuplicateEntityException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating user '{Username}'.", username);
            throw;
        }
    }

    /// <inheritdoc />
    /// <exception cref="EntityNotFoundException">Thrown if the user does not exist.</exception>
    /// <exception cref="SystemEntityException">Thrown if the user is a system entity.</exception>
    public async Task UpdateAsync(User user)
    {
        try
        {
            var existing = await _userRepository.GetByUidAsync(user.Uid)
                ?? throw new EntityNotFoundException(nameof(User), user.Uid);

            if (existing.IsSystem)
                throw new SystemEntityException(nameof(User));

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            _logger.Information("User {Uid} updated.", user.Uid);
        }
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (SystemEntityException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating user {Uid}.", user.Uid);
            throw;
        }
    }

    /// <inheritdoc />
    /// <exception cref="EntityNotFoundException">Thrown if the user does not exist.</exception>
    /// <exception cref="SystemEntityException">Thrown if the user is a system entity.</exception>
    public async Task DeleteAsync(Guid uid)
    {
        try
        {
            var existing = await _userRepository.GetByUidAsync(uid)
                ?? throw new EntityNotFoundException(nameof(User), uid);

            if (existing.IsSystem)
                throw new SystemEntityException(nameof(User));

            await _userRepository.DeleteAsync(uid);
            _logger.Information("User {Uid} deleted.", uid);
        }
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (SystemEntityException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting user {Uid}.", uid);
            throw;
        }
    }

    /// <inheritdoc />
    /// <exception cref="EntityNotFoundException">Thrown if the user does not exist.</exception>
    /// <exception cref="UnauthorizedException">Thrown if <paramref name="currentPassword"/> is incorrect.</exception>
    public async Task ChangePasswordAsync(Guid uid, string currentPassword, string newPassword)
    {
        try
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
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error changing password for user {Uid}.", uid);
            throw;
        }
    }
}
