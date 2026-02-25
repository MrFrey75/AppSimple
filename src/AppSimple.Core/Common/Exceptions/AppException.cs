namespace AppSimple.Core.Common.Exceptions;

/// <summary>
/// Base exception for all application-specific errors. Catch this to handle any domain exception uniformly.
/// </summary>
public class AppException : Exception
{
    /// <summary>Initializes a new instance with a message.</summary>
    public AppException(string message) : base(message) { }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    public AppException(string message, Exception innerException) : base(message, innerException) { }
}
