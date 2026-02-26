using AppSimple.Core.Models;
using AppSimple.DataLib.Repositories;
using AppSimple.DataLib.Tests.Helpers;

namespace AppSimple.DataLib.Tests.Repositories;

/// <summary>
/// Integration tests for <see cref="TagRepository"/> using an in-memory SQLite database.
/// </summary>
public sealed class TagRepositoryTests : DatabaseTestBase
{
    private readonly TagRepository  _repo;
    private readonly UserRepository _userRepo;

    private readonly Guid _userUid;

    public TagRepositoryTests()
    {
        _repo     = new TagRepository(ConnectionFactory);
        _userRepo = new UserRepository(ConnectionFactory);

        // Tags have a FK to Users — seed a user up front
        var user = UserFactory.Create();
        _userUid = user.Uid;
        _userRepo.AddAsync(user).GetAwaiter().GetResult();
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_InsertsTag_CanBeRetrievedByUid()
    {
        var tag = TagFactory.Create(userUid: _userUid);
        await _repo.AddAsync(tag);

        var result = await _repo.GetByUidAsync(tag.Uid);

        Assert.NotNull(result);
        Assert.Equal(tag.Uid, result.Uid);
    }

    [Fact]
    public async Task AddAsync_PersistsAllFields()
    {
        var tag = TagFactory.Create(
            userUid: _userUid,
            name: "urgent",
            description: "High priority items",
            color: "#FF0000");
        await _repo.AddAsync(tag);

        var result = await _repo.GetByUidAsync(tag.Uid);

        Assert.NotNull(result);
        Assert.Equal(_userUid, result.UserUid);
        Assert.Equal("urgent", result.Name);
        Assert.Equal("High priority items", result.Description);
        Assert.Equal("#FF0000", result.Color);
    }

    [Fact]
    public async Task AddAsync_NullableFields_CanBeNull()
    {
        var tag = TagFactory.Create(userUid: _userUid, description: null);
        await _repo.AddAsync(tag);

        var result = await _repo.GetByUidAsync(tag.Uid);

        Assert.NotNull(result);
        Assert.Null(result.Description);
    }

    // -------------------------------------------------------------------------
    // GetByUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUidAsync_ReturnsNull_WhenNotFound()
    {
        Assert.Null(await _repo.GetByUidAsync(Guid.NewGuid()));
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoTags()
    {
        Assert.Empty(await _repo.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllInsertedTags()
    {
        await _repo.AddAsync(TagFactory.Create(userUid: _userUid));
        await _repo.AddAsync(TagFactory.Create(userUid: _userUid));

        Assert.Equal(2, (await _repo.GetAllAsync()).Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTagsOrderedByName()
    {
        await _repo.AddAsync(TagFactory.Create(userUid: _userUid, name: "zebra"));
        await _repo.AddAsync(TagFactory.Create(userUid: _userUid, name: "alpha"));
        await _repo.AddAsync(TagFactory.Create(userUid: _userUid, name: "mike"));

        var names = (await _repo.GetAllAsync()).Select(t => t.Name).ToList();

        Assert.Equal(["alpha", "mike", "zebra"], names);
    }

    // -------------------------------------------------------------------------
    // GetByUserUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUserUidAsync_ReturnsOnlyTagsForThatUser()
    {
        var otherUser = UserFactory.Create();
        await _userRepo.AddAsync(otherUser);

        await _repo.AddAsync(TagFactory.Create(userUid: _userUid));
        await _repo.AddAsync(TagFactory.Create(userUid: _userUid));
        await _repo.AddAsync(TagFactory.Create(userUid: otherUser.Uid));

        var result = await _repo.GetByUserUidAsync(_userUid);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByUserUidAsync_ReturnsEmpty_WhenNoTagsForUser()
    {
        Assert.Empty(await _repo.GetByUserUidAsync(Guid.NewGuid()));
    }

    // -------------------------------------------------------------------------
    // GetByNameAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByNameAsync_ReturnsTag_WhenExists()
    {
        var tag = TagFactory.Create(userUid: _userUid, name: "findme");
        await _repo.AddAsync(tag);

        var result = await _repo.GetByNameAsync(_userUid, "findme");

        Assert.NotNull(result);
        Assert.Equal(tag.Uid, result.Uid);
    }

    [Fact]
    public async Task GetByNameAsync_IsCaseInsensitive()
    {
        var tag = TagFactory.Create(userUid: _userUid, name: "CasedTag");
        await _repo.AddAsync(tag);

        var result = await _repo.GetByNameAsync(_userUid, "casedtag");

        Assert.NotNull(result);
        Assert.Equal(tag.Uid, result.Uid);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsNull_WhenNotFound()
    {
        Assert.Null(await _repo.GetByNameAsync(_userUid, "nonexistent"));
    }

    [Fact]
    public async Task GetByNameAsync_DoesNotReturnTag_FromDifferentUser()
    {
        var otherUser = UserFactory.Create();
        await _userRepo.AddAsync(otherUser);
        await _repo.AddAsync(TagFactory.Create(userUid: otherUser.Uid, name: "shared-name"));

        Assert.Null(await _repo.GetByNameAsync(_userUid, "shared-name"));
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var tag = TagFactory.Create(userUid: _userUid, name: "old", color: "#000000");
        await _repo.AddAsync(tag);

        tag.Name        = "new";
        tag.Description = "Updated description";
        tag.Color       = "#FFFFFF";
        tag.UpdatedAt   = DateTime.UtcNow;
        await _repo.UpdateAsync(tag);

        var result = await _repo.GetByUidAsync(tag.Uid);
        Assert.NotNull(result);
        Assert.Equal("new", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("#FFFFFF", result.Color);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_RemovesTag()
    {
        var tag = TagFactory.Create(userUid: _userUid);
        await _repo.AddAsync(tag);

        await _repo.DeleteAsync(tag.Uid);

        Assert.Null(await _repo.GetByUidAsync(tag.Uid));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenTagNotFound()
    {
        var ex = await Record.ExceptionAsync(() => _repo.DeleteAsync(Guid.NewGuid()));
        Assert.Null(ex);
    }

    [Fact]
    public async Task DeleteAsync_CascadesRemovalFromNoteTags()
    {
        var noteRepo = new NoteRepository(ConnectionFactory);
        var note     = NoteFactory.Create(userUid: _userUid);
        var tag      = TagFactory.Create(userUid: _userUid);
        await noteRepo.AddAsync(note);
        await _repo.AddAsync(tag);
        await noteRepo.AddTagAsync(note.Uid, tag.Uid);

        await _repo.DeleteAsync(tag.Uid);

        var refreshed = await noteRepo.GetByUidAsync(note.Uid);
        Assert.NotNull(refreshed);
        Assert.Empty(refreshed.Tags); // NoteTag row should be cascade-deleted
    }
}
