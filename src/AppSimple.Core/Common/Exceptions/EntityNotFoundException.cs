namespace AppSimple.Core.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be found in the data store.
/// </summary>
public sealed class EntityNotFoundException : AppException
{
    /// <summary>Gets the type name of the entity that was not found.</summary>
    public string EntityType { get; }

    /// <summary>Gets the identifier used in the lookup.</summary>
    public object EntityId { get; }

    /// <summary>
    /// Initializes a new instance for the given entity type and identifier.
    /// </summary>
    /// <param name="entityType">The entity type name (e.g. <c>"User"</c>).</param>
    /// <param name="entityId">The identifier that was searched for.</param>
    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with id '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId   = entityId;
    }
}
