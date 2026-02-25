using Serilog.Events;

namespace AppSimple.Core.Logging;

/// <summary>
/// Configuration options for the application logger.
/// Pass to <c>AddAppLogging</c> during DI setup.
/// </summary>
public sealed class LoggingOptions
{
    /// <summary>
    /// Gets or sets the minimum log level written to any sink.
    /// Defaults to <see cref="LogEventLevel.Information"/>.
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Gets or sets a value indicating whether log output is written to the console.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether log output is written to a rolling file.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool EnableFile { get; set; } = false;

    /// <summary>
    /// Gets or sets the directory where rolling log files are written.
    /// Defaults to <c>logs/</c> relative to the working directory.
    /// Only used when <see cref="EnableFile"/> is <c>true</c>.
    /// </summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>
    /// Gets or sets the Serilog output template used by console and file sinks.
    /// </summary>
    public string OutputTemplate { get; set; } =
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Gets or sets the application name written to every log event as the <c>Application</c> property.
    /// Defaults to <see cref="Constants.AppConstants.AppName"/>.
    /// </summary>
    public string ApplicationName { get; set; } = Constants.AppConstants.AppName;

    /// <summary>
    /// Gets or sets the rolling interval for file logs.
    /// Defaults to <see cref="Serilog.RollingInterval.Day"/>.
    /// </summary>
    public Serilog.RollingInterval RollingInterval { get; set; } = Serilog.RollingInterval.Day;

    /// <summary>
    /// Gets or sets the maximum number of retained log files.
    /// Defaults to <c>7</c>.
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 7;
}
