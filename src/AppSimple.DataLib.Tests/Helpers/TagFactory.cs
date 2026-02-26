using AppSimple.Core.Models;

namespace AppSimple.DataLib.Tests.Helpers;

/// <summary>
/// Provides factory methods for building <see cref="Tag"/> test instances with sensible defaults.
/// </summary>
internal static class TagFactory
{
    private static int _counter;

    /// <summary>
    /// Creates a new <see cref="Tag"/> with a unique UID and optional overrides.
    /// </summary>
    public static Tag Create(
        Guid? userUid = null,
        string? name = null,
        string? description = null,
        string? color = null)
    {
        var n   = ++_counter;
        var now = DateTime.UtcNow;
        return new Tag
        {
            Uid         = Guid.CreateVersion7(),
            UserUid     = userUid      ?? Guid.CreateVersion7(),
            Name        = name         ?? $"tag{n}",
            Description = description,
            Color       = color        ?? "#CCCCCC",
            CreatedAt   = now,
            UpdatedAt   = now,
        };
    }
}
