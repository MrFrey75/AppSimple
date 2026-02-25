namespace AppSimple.Core.Common.Exceptions;

/// <summary>
/// Thrown when an operation is rejected because the caller is not authorized to perform it.
/// </summary>
public sealed class UnauthorizedException : AppException
{
    /// <summary>Initializes a new instance with the given reason.</summary>
    public UnauthorizedException(string message) : base(message) { }
}
