using Serilog;
using Serilog.Events;

namespace AppSimple.Core.Logging.Impl;

/// <summary>
/// Serilog-backed implementation of <see cref="IAppLogger{T}"/>.
/// Obtains a context-enriched <see cref="ILogger"/> via <c>Log.ForContext&lt;T&gt;()</c>.
/// </summary>
/// <typeparam name="T">The owning class; sets the <c>SourceContext</c> enrichment property.</typeparam>
public sealed class SerilogAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SerilogAppLogger{T}"/> bound to the Serilog static context.
    /// </summary>
    public SerilogAppLogger()
    {
        _logger = Log.ForContext<T>();
    }

    /// <inheritdoc />
    public void Debug(string messageTemplate, params object?[] args)
        => _logger.Debug(messageTemplate, args);

    /// <inheritdoc />
    public void Information(string messageTemplate, params object?[] args)
        => _logger.Information(messageTemplate, args);

    /// <inheritdoc />
    public void Warning(string messageTemplate, params object?[] args)
        => _logger.Warning(messageTemplate, args);

    /// <inheritdoc />
    public void Error(string messageTemplate, params object?[] args)
        => _logger.Error(messageTemplate, args);

    /// <inheritdoc />
    public void Error(Exception ex, string messageTemplate, params object?[] args)
        => _logger.Error(ex, messageTemplate, args);

    /// <inheritdoc />
    public void Fatal(string messageTemplate, params object?[] args)
        => _logger.Fatal(messageTemplate, args);

    /// <inheritdoc />
    public void Fatal(Exception ex, string messageTemplate, params object?[] args)
        => _logger.Fatal(ex, messageTemplate, args);

    /// <inheritdoc />
    public bool IsEnabled(LogEventLevel level)
        => _logger.IsEnabled(level);
}
