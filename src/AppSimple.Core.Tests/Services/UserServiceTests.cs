using AppSimple.Core.Auth;
using AppSimple.Core.Common.Exceptions;
using AppSimple.Core.Enums;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;
using AppSimple.Core.Services;
using AppSimple.Core.Services.Impl;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AppSimple.Core.Tests.Services;

/// <summary>Unit tests for <see cref="UserService"/>.</summary>
public sealed class UserServiceTests
{
    private readonly IUserRepository _repo       = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher     = Substitute.For<IPasswordHasher>();
    private readonly ITagService     _tagService = Substitute.For<ITagService>();
    private readonly IAppLogger<UserService> _log = Substitute.For<IAppLogger<UserService>>();
    private readonly UserService _svc;

    public UserServiceTests()
    {
        _svc = new UserService(_repo, _hasher, _tagService, _log);
        _hasher.Hash(Arg.Any<string>()).Returns("$2a$hashed");
    }

    private static User MakeUser(bool isSystem = false) => new()
    {
        Uid          = Guid.CreateVersion7(),
        Username     = "testuser",
        Email        = "test@example.com",
        PasswordHash = "$2a$11$hash",
        IsSystem     = isSystem
    };

    // -------------------------------------------------------------------------
    // GetByUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUidAsync_ReturnsUser_WhenFound()
    {
        var user = MakeUser();
        _repo.GetByUidAsync(user.Uid).Returns(user);

        var result = await _svc.GetByUidAsync(user.Uid);
        Assert.Same(user, result);
    }

    [Fact]
    public async Task GetByUidAsync_ReturnsNull_WhenNotFound()
    {
        _repo.GetByUidAsync(Arg.Any<Guid>()).ReturnsNull();
        Assert.Null(await _svc.GetByUidAsync(Guid.NewGuid()));
    }

    // -------------------------------------------------------------------------
    // GetByUsernameAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUsernameAsync_DelegatesToRepository()
    {
        var user = MakeUser();
        _repo.GetByUsernameAsync("testuser").Returns(user);

        var result = await _svc.GetByUsernameAsync("testuser");
        Assert.Same(user, result);
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        var users = new[] { MakeUser(), MakeUser() };
        _repo.GetAllAsync().Returns(users);

        var result = await _svc.GetAllAsync();
        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ThrowsDuplicateEntityException_WhenUsernameExists()
    {
        _repo.UsernameExistsAsync("existing").Returns(true);

        await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _svc.CreateAsync("existing", "new@test.com", "Pass1234!"));
    }

    [Fact]
    public async Task CreateAsync_ThrowsDuplicateEntityException_WhenEmailExists()
    {
        _repo.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        _repo.EmailExistsAsync("used@test.com").Returns(true);

        await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _svc.CreateAsync("newuser", "used@test.com", "Pass1234!"));
    }

    [Fact]
    public async Task CreateAsync_HashesPassword()
    {
        _repo.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        _repo.EmailExistsAsync(Arg.Any<string>()).Returns(false);

        await _svc.CreateAsync("alice", "alice@test.com", "PlainPass1");

        _hasher.Received(1).Hash("PlainPass1");
    }

    [Fact]
    public async Task CreateAsync_SetsTimestamps()
    {
        _repo.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        _repo.EmailExistsAsync(Arg.Any<string>()).Returns(false);

        var before = DateTime.UtcNow.AddSeconds(-1);
        var user   = await _svc.CreateAsync("alice", "alice@test.com", "PlainPass1");
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(user.CreatedAt, before, after);
        Assert.InRange(user.UpdatedAt, before, after);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryAdd()
    {
        _repo.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        _repo.EmailExistsAsync(Arg.Any<string>()).Returns(false);

        await _svc.CreateAsync("alice", "alice@test.com", "PlainPass1");

        await _repo.Received(1).AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsUserWithCorrectFields()
    {
        _repo.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
        _repo.EmailExistsAsync(Arg.Any<string>()).Returns(false);

        var user = await _svc.CreateAsync("alice", "alice@test.com", "PlainPass1");

        Assert.Equal("alice", user.Username);
        Assert.Equal("alice@test.com", user.Email);
        Assert.Equal("$2a$hashed", user.PasswordHash);
        Assert.NotEqual(Guid.Empty, user.Uid);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ThrowsEntityNotFoundException_WhenUserNotFound()
    {
        _repo.GetByUidAsync(Arg.Any<Guid>()).ReturnsNull();

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _svc.UpdateAsync(MakeUser()));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsSystemEntityException_ForSystemUser()
    {
        var user = MakeUser(isSystem: true);
        _repo.GetByUidAsync(user.Uid).Returns(user);

        await Assert.ThrowsAsync<SystemEntityException>(
            () => _svc.UpdateAsync(user));
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt_AndCallsRepository()
    {
        var user = MakeUser();
        _repo.GetByUidAsync(user.Uid).Returns(user);

        var before = DateTime.UtcNow.AddSeconds(-1);
        await _svc.UpdateAsync(user);
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(user.UpdatedAt, before, after);
        await _repo.Received(1).UpdateAsync(user);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ThrowsEntityNotFoundException_WhenNotFound()
    {
        _repo.GetByUidAsync(Arg.Any<Guid>()).ReturnsNull();

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _svc.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsSystemEntityException_ForSystemUser()
    {
        var user = MakeUser(isSystem: true);
        _repo.GetByUidAsync(user.Uid).Returns(user);

        await Assert.ThrowsAsync<SystemEntityException>(
            () => _svc.DeleteAsync(user.Uid));
    }

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete()
    {
        var user = MakeUser();
        _repo.GetByUidAsync(user.Uid).Returns(user);

        await _svc.DeleteAsync(user.Uid);

        await _repo.Received(1).DeleteAsync(user.Uid);
    }

    // -------------------------------------------------------------------------
    // ChangePasswordAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ChangePasswordAsync_ThrowsEntityNotFoundException_WhenNotFound()
    {
        _repo.GetByUidAsync(Arg.Any<Guid>()).ReturnsNull();

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _svc.ChangePasswordAsync(Guid.NewGuid(), "old", "New1"));
    }

    [Fact]
    public async Task ChangePasswordAsync_ThrowsUnauthorizedException_WhenPasswordWrong()
    {
        var user = MakeUser();
        _repo.GetByUidAsync(user.Uid).Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _svc.ChangePasswordAsync(user.Uid, "wrongpass", "NewSecure1"));
    }

    [Fact]
    public async Task ChangePasswordAsync_Success_UpdatesHash()
    {
        var user = MakeUser();
        _repo.GetByUidAsync(user.Uid).Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _hasher.Hash("NewSecure1").Returns("$2a$newhash");

        await _svc.ChangePasswordAsync(user.Uid, "OldPass1", "NewSecure1");

        Assert.Equal("$2a$newhash", user.PasswordHash);
        await _repo.Received(1).UpdateAsync(user);
    }
}
