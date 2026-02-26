using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;
using AppSimple.Core.Services.Impl;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AppSimple.Core.Tests.Services;

/// <summary>Unit tests for <see cref="NoteService"/>.</summary>
public sealed class NoteServiceTests
{
    private readonly INoteRepository _repo = Substitute.For<INoteRepository>();
    private readonly IAppLogger<NoteService> _log = Substitute.For<IAppLogger<NoteService>>();
    private readonly NoteService _svc;

    public NoteServiceTests()
    {
        _svc = new NoteService(_repo, _log);
    }

    private static readonly Guid _userUid = Guid.CreateVersion7();

    private static Note MakeNote(Guid? userUid = null) => new()
    {
        Uid       = Guid.CreateVersion7(),
        UserUid   = userUid ?? _userUid,
        Title     = "Test Note",
        Content   = "Some content",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    // -------------------------------------------------------------------------
    // GetByUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUidAsync_ReturnsNote_WhenFound()
    {
        var note = MakeNote();
        _repo.GetByUidAsync(note.Uid).Returns(note);

        var result = await _svc.GetByUidAsync(note.Uid);
        Assert.Same(note, result);
    }

    [Fact]
    public async Task GetByUidAsync_ReturnsNull_WhenNotFound()
    {
        _repo.GetByUidAsync(Arg.Any<Guid>()).ReturnsNull();
        Assert.Null(await _svc.GetByUidAsync(Guid.NewGuid()));
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var notes = new[] { MakeNote(), MakeNote() };
        _repo.GetAllAsync().Returns(notes);

        var result = await _svc.GetAllAsync();
        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------------------
    // GetByUserUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUserUidAsync_ReturnsNotesForUser()
    {
        var notes = new[] { MakeNote(_userUid), MakeNote(_userUid) };
        _repo.GetByUserUidAsync(_userUid).Returns(notes);

        var result = await _svc.GetByUserUidAsync(_userUid);
        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_CallsRepositoryAdd()
    {
        await _svc.CreateAsync(_userUid, "Title", "Body");
        await _repo.Received(1).AddAsync(Arg.Any<Note>());
    }

    [Fact]
    public async Task CreateAsync_SetsCorrectFields()
    {
        var note = await _svc.CreateAsync(_userUid, "My Title", "My Content");

        Assert.Equal(_userUid, note.UserUid);
        Assert.Equal("My Title", note.Title);
        Assert.Equal("My Content", note.Content);
        Assert.NotEqual(Guid.Empty, note.Uid);
    }

    [Fact]
    public async Task CreateAsync_SetsTimestamps()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var note   = await _svc.CreateAsync(_userUid, "T", "C");
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(note.CreatedAt, before, after);
        Assert.InRange(note.UpdatedAt, before, after);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt_AndCallsRepository()
    {
        var note   = MakeNote();
        var before = DateTime.UtcNow.AddSeconds(-1);
        await _svc.UpdateAsync(note);
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(note.UpdatedAt, before, after);
        await _repo.Received(1).UpdateAsync(note);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete()
    {
        var uid = Guid.NewGuid();
        await _svc.DeleteAsync(uid);
        await _repo.Received(1).DeleteAsync(uid);
    }

    // -------------------------------------------------------------------------
    // AddTagAsync / RemoveTagAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddTagAsync_DelegatesToRepository()
    {
        var noteUid = Guid.NewGuid();
        var tagUid  = Guid.NewGuid();

        await _svc.AddTagAsync(noteUid, tagUid);
        await _repo.Received(1).AddTagAsync(noteUid, tagUid);
    }

    [Fact]
    public async Task RemoveTagAsync_DelegatesToRepository()
    {
        var noteUid = Guid.NewGuid();
        var tagUid  = Guid.NewGuid();

        await _svc.RemoveTagAsync(noteUid, tagUid);
        await _repo.Received(1).RemoveTagAsync(noteUid, tagUid);
    }
}
