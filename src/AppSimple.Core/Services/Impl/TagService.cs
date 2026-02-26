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
    }

    /// <inheritdoc />
    public async Task<Tag?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _tags.GetByUidAsync(uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving tag {Uid}.", uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        try
        {
            return await _tags.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving all tags.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tag>> GetByUserUidAsync(Guid userUid)
    {
        try
        {
            return await _tags.GetByUserUidAsync(userUid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving tags for user {UserUid}.", userUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Tag> CreateAsync(Guid userUid, string name, string? description = null, string? color = null)
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating tag '{Name}' for user {UserUid}.", name, userUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Tag tag)
    {
        try
        {
            tag.UpdatedAt = DateTime.UtcNow;
            await _tags.UpdateAsync(tag);
            _logger.Information("Tag {Uid} updated.", tag.Uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating tag {Uid}.", tag.Uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        try
        {
            await _tags.DeleteAsync(uid);
            _logger.Information("Tag {Uid} deleted.", uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting tag {Uid}.", uid);
            throw;
        }
    }
}
