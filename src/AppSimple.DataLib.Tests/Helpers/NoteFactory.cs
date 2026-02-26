using AppSimple.Core.Models;

namespace AppSimple.DataLib.Tests.Helpers;

/// <summary>
/// Provides factory methods for building <see cref="Note"/> test instances with sensible defaults.
/// </summary>
internal static class NoteFactory
{
    private static int _counter;

    /// <summary>
    /// Creates a new <see cref="Note"/> with a unique UID and optional overrides.
    /// </summary>
    public static Note Create(
        Guid? userUid = null,
        string? title = null,
        string? content = null)
    {
        var n   = ++_counter;
        var now = DateTime.UtcNow;
        return new Note
        {
            Uid       = Guid.CreateVersion7(),
            UserUid   = userUid ?? Guid.CreateVersion7(),
            Title     = title   ?? $"Note {n}",
            Content   = content ?? $"Content for note {n}",
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}
