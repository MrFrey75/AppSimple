namespace AppSimple.Core.Common.Exceptions;

/// <summary>
/// Thrown when an operation attempts to modify or delete a system-protected entity.
/// </summary>
public sealed class SystemEntityException : AppException
{
    /// <summary>Gets the type name of the protected entity.</summary>
    public string EntityType { get; }

    /// <summary>
    /// Initializes a new instance for the given entity type.
    /// </summary>
    public SystemEntityException(string entityType)
        : base($"{entityType} is a system entity and cannot be modified or deleted.")
    {
        EntityType = entityType;
    }
}
