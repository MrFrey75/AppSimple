using AppSimple.Core.Constants;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;

namespace AppSimple.Core.Services.Impl;

/// <summary>
/// Core implementation of <see cref="ITagService"/> backed by <see cref="ITagRepository"/>.
/// </summary>
public sealed class TagService : ITagService
{
    private readonly ITagRepository _tags;
    private readonly IAppLogger<TagService> _logger;

    /// <summary>Initializes a new instance of <see cref="TagService"/>.</summary>
    public TagService(ITagRepository tags, IAppLogger<TagService> logger)
    {
        _tags   = tags;
        _logger = logger;
        _logger.Debug("TagService initialized.");
    }

    /// <inheritdoc />
    public Task<Tag?> GetByUidAsync(Guid uid)
    {
        _logger.Debug("GetByUid: {Uid}", uid);
        return _tags.GetByUidAsync(uid);
    }

    /// <inheritdoc />
    public Task<IEnumerable<Tag>> GetAllAsync()
    {
        _logger.Debug("GetAll tags requested.");
        return _tags.GetAllAsync();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Tag>> GetByUserUidAsync(Guid userUid)
    {
        _logger.Debug("GetByUserUid: {UserUid}", userUid);
        return _tags.GetByUserUidAsync(userUid);
    }

    /// <inheritdoc />
    public async Task<Tag> CreateAsync(Guid userUid, string name, string? description = null, string? color = null)
    {
        var now = DateTime.UtcNow;
        var tag = new Tag
        {
            Uid         = Guid.CreateVersion7(),
            UserUid     = userUid,
            Name        = name,
            Description = description,
            Color       = color ?? "#CCCCCC",
            CreatedAt   = now,
            UpdatedAt   = now,
        };

        await _tags.AddAsync(tag);
        _logger.Information("Tag '{Name}' ({Uid}) created for user {UserUid}.", name, tag.Uid, userUid);
        return tag;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Tag tag)
    {
        tag.UpdatedAt = DateTime.UtcNow;
        await _tags.UpdateAsync(tag);
        _logger.Information("Tag {Uid} updated.", tag.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        await _tags.DeleteAsync(uid);
        _logger.Information("Tag {Uid} deleted.", uid);
    }

    /// <inheritdoc />
    public async Task SeedDefaultTagsAsync(Guid userUid)
    {
        var existing = await _tags.GetByUserUidAsync(userUid);
        if (existing.Any()) return;

        var now = DateTime.UtcNow;
        foreach (var (name, color) in AppConstants.DefaultTags)
        {
            var tag = new Tag
            {
                Uid       = Guid.CreateVersion7(),
                UserUid   = userUid,
                Name      = name,
                Color     = color,
                IsSystem  = true,
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _tags.AddAsync(tag);
        }
        _logger.Information("Seeded {Count} default tags for user {UserUid}.",
            AppConstants.DefaultTags.Count, userUid);
    }
}
