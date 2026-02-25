namespace AppSimple.Core.Logging;

/// <summary>
/// Typed logger abstraction used throughout the application.
/// Implementations are backed by Serilog and enriched with context from the type parameter <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The class that owns this logger; used to set the <c>SourceContext</c> property.</typeparam>
public interface IAppLogger<T>
{
    /// <summary>Writes a verbose/debug-level message.</summary>
    /// <param name="messageTemplate">Serilog message template.</param>
    /// <param name="args">Template property values.</param>
    void Debug(string messageTemplate, params object?[] args);

    /// <summary>Writes an informational message.</summary>
    void Information(string messageTemplate, params object?[] args);

    /// <summary>Writes a warning message.</summary>
    void Warning(string messageTemplate, params object?[] args);

    /// <summary>Writes an error message.</summary>
    void Error(string messageTemplate, params object?[] args);

    /// <summary>Writes an error message with an associated exception.</summary>
    /// <param name="ex">The exception to attach to the log event.</param>
    void Error(Exception ex, string messageTemplate, params object?[] args);

    /// <summary>Writes a fatal message indicating an unrecoverable failure.</summary>
    void Fatal(string messageTemplate, params object?[] args);

    /// <summary>Writes a fatal message with an associated exception.</summary>
    void Fatal(Exception ex, string messageTemplate, params object?[] args);

    /// <summary>Checks whether logging at the given level is currently enabled.</summary>
    /// <param name="level">The <see cref="Serilog.Events.LogEventLevel"/> to test.</param>
    bool IsEnabled(Serilog.Events.LogEventLevel level);
}
