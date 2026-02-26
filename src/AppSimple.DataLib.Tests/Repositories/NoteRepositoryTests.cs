using AppSimple.Core.Models;
using AppSimple.DataLib.Repositories;
using AppSimple.DataLib.Tests.Helpers;

namespace AppSimple.DataLib.Tests.Repositories;

/// <summary>
/// Integration tests for <see cref="NoteRepository"/> and <see cref="TagRepository"/>
/// using an in-memory SQLite database.
/// </summary>
public sealed class NoteRepositoryTests : DatabaseTestBase
{
    private readonly NoteRepository _repo;
    private readonly TagRepository  _tagRepo;
    private readonly UserRepository _userRepo;

    private readonly Guid _userUid;

    public NoteRepositoryTests()
    {
        _repo     = new NoteRepository(ConnectionFactory);
        _tagRepo  = new TagRepository(ConnectionFactory);
        _userRepo = new UserRepository(ConnectionFactory);

        // Notes and Tags have a FK to Users — seed a user up front
        var user = UserFactory.Create();
        _userUid = user.Uid;
        _userRepo.AddAsync(user).GetAwaiter().GetResult();
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_InsertsNote_CanBeRetrievedByUid()
    {
        var note = NoteFactory.Create(userUid: _userUid);
        await _repo.AddAsync(note);

        var result = await _repo.GetByUidAsync(note.Uid);

        Assert.NotNull(result);
        Assert.Equal(note.Uid, result.Uid);
    }

    [Fact]
    public async Task AddAsync_PersistsAllFields()
    {
        var note = NoteFactory.Create(userUid: _userUid, title: "My Title", content: "My Content");
        await _repo.AddAsync(note);

        var result = await _repo.GetByUidAsync(note.Uid);

        Assert.NotNull(result);
        Assert.Equal(_userUid, result.UserUid);
        Assert.Equal("My Title", result.Title);
        Assert.Equal("My Content", result.Content);
    }

    // -------------------------------------------------------------------------
    // GetByUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUidAsync_ReturnsNull_WhenNotFound()
    {
        Assert.Null(await _repo.GetByUidAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByUidAsync_PopulatesTags()
    {
        var note = NoteFactory.Create(userUid: _userUid);
        var tag  = TagFactory.Create(userUid: _userUid, name: "important");
        await _repo.AddAsync(note);
        await _tagRepo.AddAsync(tag);
        await _repo.AddTagAsync(note.Uid, tag.Uid);

        var result = await _repo.GetByUidAsync(note.Uid);

        Assert.NotNull(result);
        Assert.Single(result.Tags);
        Assert.Equal(tag.Uid, result.Tags[0].Uid);
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoNotes()
    {
        Assert.Empty(await _repo.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllInsertedNotes()
    {
        await _repo.AddAsync(NoteFactory.Create(userUid: _userUid));
        await _repo.AddAsync(NoteFactory.Create(userUid: _userUid));
        await _repo.AddAsync(NoteFactory.Create(userUid: _userUid));

        Assert.Equal(3, (await _repo.GetAllAsync()).Count());
    }

    // -------------------------------------------------------------------------
    // GetByUserUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUserUidAsync_ReturnsOnlyNotesForThatUser()
    {
        var otherUser = UserFactory.Create();
        await _userRepo.AddAsync(otherUser);

        await _repo.AddAsync(NoteFactory.Create(userUid: _userUid));
        await _repo.AddAsync(NoteFactory.Create(userUid: _userUid));
        await _repo.AddAsync(NoteFactory.Create(userUid: otherUser.Uid));

        var result = await _repo.GetByUserUidAsync(_userUid);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByUserUidAsync_ReturnsEmpty_WhenNoNotesForUser()
    {
        Assert.Empty(await _repo.GetByUserUidAsync(Guid.NewGuid()));
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var note = NoteFactory.Create(userUid: _userUid, title: "Old Title", content: "Old");
        await _repo.AddAsync(note);

        note.Title   = "New Title";
        note.Content = "New Content";
        note.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(note);

        var result = await _repo.GetByUidAsync(note.Uid);
        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Content", result.Content);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_RemovesNote()
    {
        var note = NoteFactory.Create(userUid: _userUid);
        await _repo.AddAsync(note);

        await _repo.DeleteAsync(note.Uid);

        Assert.Null(await _repo.GetByUidAsync(note.Uid));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenNoteNotFound()
    {
        var ex = await Record.ExceptionAsync(() => _repo.DeleteAsync(Guid.NewGuid()));
        Assert.Null(ex);
    }

    // -------------------------------------------------------------------------
    // AddTagAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddTagAsync_AssociatesTagWithNote()
    {
        var note = NoteFactory.Create(userUid: _userUid);
        var tag  = TagFactory.Create(userUid: _userUid);
        await _repo.AddAsync(note);
        await _tagRepo.AddAsync(tag);

        await _repo.AddTagAsync(note.Uid, tag.Uid);

        var result = await _repo.GetByUidAsync(note.Uid);
        Assert.NotNull(result);
        Assert.Contains(result.Tags, t => t.Uid == tag.Uid);
    }

    [Fact]
    public async Task AddTagAsync_IsIdempotent_WhenCalledTwice()
    {
        var note = NoteFactory.Create(userUid: _userUid);
        var tag  = TagFactory.Create(userUid: _userUid);
        await _repo.AddAsync(note);
        await _tagRepo.AddAsync(tag);

        await _repo.AddTagAsync(note.Uid, tag.Uid);
        await _repo.AddTagAsync(note.Uid, tag.Uid); // second call should not throw or duplicate

        var result = await _repo.GetByUidAsync(note.Uid);
        Assert.Single(result!.Tags);
    }

    // -------------------------------------------------------------------------
    // RemoveTagAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RemoveTagAsync_DisassociatesTagFromNote()
    {
        var note = NoteFactory.Create(userUid: _userUid);
        var tag  = TagFactory.Create(userUid: _userUid);
        await _repo.AddAsync(note);
        await _tagRepo.AddAsync(tag);
        await _repo.AddTagAsync(note.Uid, tag.Uid);

        await _repo.RemoveTagAsync(note.Uid, tag.Uid);

        var result = await _repo.GetByUidAsync(note.Uid);
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task RemoveTagAsync_DoesNotThrow_WhenAssociationDoesNotExist()
    {
        var ex = await Record.ExceptionAsync(
            () => _repo.RemoveTagAsync(Guid.NewGuid(), Guid.NewGuid()));
        Assert.Null(ex);
    }
}
