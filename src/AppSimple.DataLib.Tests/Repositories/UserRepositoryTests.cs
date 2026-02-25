using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using AppSimple.DataLib.Repositories;
using AppSimple.DataLib.Tests.Helpers;

namespace AppSimple.DataLib.Tests.Repositories;

/// <summary>
/// Integration tests for <see cref="UserRepository"/> using an in-memory SQLite database.
/// Each test class gets a fresh schema via <see cref="DatabaseTestBase"/>.
/// </summary>
public sealed class UserRepositoryTests : DatabaseTestBase
{
    private readonly UserRepository _repo;

    public UserRepositoryTests()
    {
        _repo = new UserRepository(ConnectionFactory);
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_InsertsUser_CanBeRetrievedByUid()
    {
        var user = UserFactory.Create();
        await _repo.AddAsync(user);

        var result = await _repo.GetByUidAsync(user.Uid);

        Assert.NotNull(result);
        Assert.Equal(user.Uid, result.Uid);
    }

    [Fact]
    public async Task AddAsync_PersistsAllFields()
    {
        var dob = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var user = UserFactory.Create(
            username: "john",
            email: "john@example.com",
            firstName: "John",
            lastName: "Doe",
            phoneNumber: "+1-555-000-0001",
            dateOfBirth: dob,
            bio: "Hello world",
            avatarUrl: "/avatars/john.png",
            role: UserRole.Admin);

        await _repo.AddAsync(user);
        var result = await _repo.GetByUidAsync(user.Uid);

        Assert.NotNull(result);
        Assert.Equal("john", result.Username);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("+1-555-000-0001", result.PhoneNumber);
        Assert.Equal(dob, result.DateOfBirth?.ToUniversalTime());
        Assert.Equal("Hello world", result.Bio);
        Assert.Equal("/avatars/john.png", result.AvatarUrl);
        Assert.Equal(UserRole.Admin, result.Role);
    }

    [Fact]
    public async Task AddAsync_NullableFields_CanBeNull()
    {
        var user = UserFactory.Create();
        await _repo.AddAsync(user);

        var result = await _repo.GetByUidAsync(user.Uid);

        Assert.NotNull(result);
        Assert.Null(result.FirstName);
        Assert.Null(result.LastName);
        Assert.Null(result.PhoneNumber);
        Assert.Null(result.DateOfBirth);
        Assert.Null(result.Bio);
        Assert.Null(result.AvatarUrl);
    }

    // -------------------------------------------------------------------------
    // GetByUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUidAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetByUidAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoUsers()
    {
        var result = await _repo.GetAllAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllInsertedUsers()
    {
        await _repo.AddAsync(UserFactory.Create());
        await _repo.AddAsync(UserFactory.Create());
        await _repo.AddAsync(UserFactory.Create());

        var result = await _repo.GetAllAsync();

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUsersOrderedByUsername()
    {
        await _repo.AddAsync(UserFactory.Create(username: "zara"));
        await _repo.AddAsync(UserFactory.Create(username: "alice"));
        await _repo.AddAsync(UserFactory.Create(username: "mike"));

        var usernames = (await _repo.GetAllAsync()).Select(u => u.Username).ToList();

        Assert.Equal(["alice", "mike", "zara"], usernames);
    }

    // -------------------------------------------------------------------------
    // GetByUsernameAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUsernameAsync_ReturnsUser_WhenExists()
    {
        var user = UserFactory.Create(username: "findme");
        await _repo.AddAsync(user);

        var result = await _repo.GetByUsernameAsync("findme");

        Assert.NotNull(result);
        Assert.Equal(user.Uid, result.Uid);
    }

    [Fact]
    public async Task GetByUsernameAsync_IsCaseInsensitive()
    {
        var user = UserFactory.Create(username: "CasedUser");
        await _repo.AddAsync(user);

        var result = await _repo.GetByUsernameAsync("caseduser");

        Assert.NotNull(result);
        Assert.Equal(user.Uid, result.Uid);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetByUsernameAsync("nobody");
        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // GetByEmailAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenExists()
    {
        var user = UserFactory.Create(email: "exact@test.com");
        await _repo.AddAsync(user);

        var result = await _repo.GetByEmailAsync("exact@test.com");

        Assert.NotNull(result);
        Assert.Equal(user.Uid, result.Uid);
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        var user = UserFactory.Create(email: "Mixed@Test.COM");
        await _repo.AddAsync(user);

        var result = await _repo.GetByEmailAsync("mixed@test.com");

        Assert.NotNull(result);
        Assert.Equal(user.Uid, result.Uid);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetByEmailAsync("ghost@test.com");
        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // UsernameExistsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UsernameExistsAsync_ReturnsTrue_WhenExists()
    {
        await _repo.AddAsync(UserFactory.Create(username: "taken"));

        Assert.True(await _repo.UsernameExistsAsync("taken"));
    }

    [Fact]
    public async Task UsernameExistsAsync_IsCaseInsensitive()
    {
        await _repo.AddAsync(UserFactory.Create(username: "Taken"));

        Assert.True(await _repo.UsernameExistsAsync("TAKEN"));
    }

    [Fact]
    public async Task UsernameExistsAsync_ReturnsFalse_WhenNotExists()
    {
        Assert.False(await _repo.UsernameExistsAsync("free"));
    }

    // -------------------------------------------------------------------------
    // EmailExistsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task EmailExistsAsync_ReturnsTrue_WhenExists()
    {
        await _repo.AddAsync(UserFactory.Create(email: "taken@test.com"));

        Assert.True(await _repo.EmailExistsAsync("taken@test.com"));
    }

    [Fact]
    public async Task EmailExistsAsync_IsCaseInsensitive()
    {
        await _repo.AddAsync(UserFactory.Create(email: "Taken@Test.com"));

        Assert.True(await _repo.EmailExistsAsync("TAKEN@TEST.COM"));
    }

    [Fact]
    public async Task EmailExistsAsync_ReturnsFalse_WhenNotExists()
    {
        Assert.False(await _repo.EmailExistsAsync("free@test.com"));
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var user = UserFactory.Create(firstName: "Old", lastName: "Name");
        await _repo.AddAsync(user);

        user.FirstName = "New";
        user.LastName = "Name2";
        user.Bio = "Updated bio";
        user.PhoneNumber = "+1-555-999-9999";
        user.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(user);

        var updated = await _repo.GetByUidAsync(user.Uid);
        Assert.NotNull(updated);
        Assert.Equal("New", updated.FirstName);
        Assert.Equal("Name2", updated.LastName);
        Assert.Equal("Updated bio", updated.Bio);
        Assert.Equal("+1-555-999-9999", updated.PhoneNumber);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotModify_SystemUser()
    {
        var systemUser = UserFactory.Create(username: "sysuser", isSystem: true);
        await _repo.AddAsync(systemUser);

        systemUser.FirstName = "Hacked";
        await _repo.UpdateAsync(systemUser);

        var result = await _repo.GetByUidAsync(systemUser.Uid);
        Assert.NotNull(result);
        // IsSystem = 0 guard in WHERE clause — original FirstName should remain
        Assert.Null(result.FirstName);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_RemovesUser()
    {
        var user = UserFactory.Create();
        await _repo.AddAsync(user);

        await _repo.DeleteAsync(user.Uid);

        Assert.Null(await _repo.GetByUidAsync(user.Uid));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotRemove_SystemUser()
    {
        var systemUser = UserFactory.Create(isSystem: true);
        await _repo.AddAsync(systemUser);

        await _repo.DeleteAsync(systemUser.Uid);

        Assert.NotNull(await _repo.GetByUidAsync(systemUser.Uid));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenUserNotFound()
    {
        var ex = await Record.ExceptionAsync(() => _repo.DeleteAsync(Guid.NewGuid()));
        Assert.Null(ex);
    }

    // -------------------------------------------------------------------------
    // FullName computed property (model-level)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("Jane", "Doe", "Jane Doe")]
    [InlineData("Jane", null, "Jane")]
    [InlineData(null, "Doe", "Doe")]
    [InlineData(null, null, null)]
    [InlineData("  ", "  ", null)]
    public void User_FullName_ReturnsExpected(string? first, string? last, string? expected)
    {
        var user = UserFactory.Create(firstName: first, lastName: last);
        Assert.Equal(expected, user.FullName);
    }
}
