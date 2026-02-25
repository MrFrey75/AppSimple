namespace AppSimple.Core.Models;

/// <summary>
/// Base class for all domain entities. Provides common fields for identity, auditing, and system protection.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity. Generated using <see cref="Guid.CreateVersion7()"/> for database performance.
    /// </summary>
    public Guid Uid { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Gets or sets the UTC date and time when the entity was created. Set automatically in the service layer.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the entity was last updated. Set automatically in the service layer.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this entity is a system entity that should not be modified by users.
    /// </summary>
    public bool IsSystem { get; set; }
}
