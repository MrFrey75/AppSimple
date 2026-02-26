using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;
using AppSimple.Core.Services.Impl;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AppSimple.Core.Tests.Services;

/// <summary>Unit tests for <see cref="TagService"/>.</summary>
public sealed class TagServiceTests
{
    private readonly ITagRepository _repo = Substitute.For<ITagRepository>();
    private readonly IAppLogger<TagService> _log = Substitute.For<IAppLogger<TagService>>();
    private readonly TagService _svc;

    public TagServiceTests()
    {
        _svc = new TagService(_repo, _log);
    }

    private static readonly Guid _userUid = Guid.CreateVersion7();

    private static Tag MakeTag(Guid? userUid = null) => new()
    {
        Uid       = Guid.CreateVersion7(),
        UserUid   = userUid ?? _userUid,
        Name      = "work",
        Color     = "#FF0000",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    // -------------------------------------------------------------------------
    // GetByUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUidAsync_ReturnsTag_WhenFound()
    {
        var tag = MakeTag();
        _repo.GetByUidAsync(tag.Uid).Returns(tag);

        var result = await _svc.GetByUidAsync(tag.Uid);
        Assert.Same(tag, result);
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
        var tags = new[] { MakeTag(), MakeTag() };
        _repo.GetAllAsync().Returns(tags);

        var result = await _svc.GetAllAsync();
        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------------------
    // GetByUserUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUserUidAsync_ReturnTagsForUser()
    {
        var tags = new[] { MakeTag(_userUid), MakeTag(_userUid) };
        _repo.GetByUserUidAsync(_userUid).Returns(tags);

        var result = await _svc.GetByUserUidAsync(_userUid);
        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_CallsRepositoryAdd()
    {
        await _svc.CreateAsync(_userUid, "work");
        await _repo.Received(1).AddAsync(Arg.Any<Tag>());
    }

    [Fact]
    public async Task CreateAsync_SetsCorrectFields()
    {
        var tag = await _svc.CreateAsync(_userUid, "work", "My work tag", "#00FF00");

        Assert.Equal(_userUid, tag.UserUid);
        Assert.Equal("work", tag.Name);
        Assert.Equal("My work tag", tag.Description);
        Assert.Equal("#00FF00", tag.Color);
        Assert.NotEqual(Guid.Empty, tag.Uid);
    }

    [Fact]
    public async Task CreateAsync_UsesDefaultColor_WhenColorNotProvided()
    {
        var tag = await _svc.CreateAsync(_userUid, "personal");
        Assert.Equal("#CCCCCC", tag.Color);
    }

    [Fact]
    public async Task CreateAsync_SetsTimestamps()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var tag    = await _svc.CreateAsync(_userUid, "urgent");
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(tag.CreatedAt, before, after);
        Assert.InRange(tag.UpdatedAt, before, after);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt_AndCallsRepository()
    {
        var tag    = MakeTag();
        var before = DateTime.UtcNow.AddSeconds(-1);
        await _svc.UpdateAsync(tag);
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(tag.UpdatedAt, before, after);
        await _repo.Received(1).UpdateAsync(tag);
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
}
