namespace AppSimple.Core.Common.Exceptions;

/// <summary>
/// Thrown when an operation would create a duplicate entry that violates a uniqueness constraint.
/// </summary>
public sealed class DuplicateEntityException : AppException
{
    /// <summary>Gets the field or property that caused the conflict (e.g. <c>"Username"</c>).</summary>
    public string Field { get; }

    /// <summary>Gets the duplicate value.</summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance for the given field and value.
    /// </summary>
    public DuplicateEntityException(string field, string value)
        : base($"A record with {field} '{value}' already exists.")
    {
        Field = field;
        Value = value;
    }
}
