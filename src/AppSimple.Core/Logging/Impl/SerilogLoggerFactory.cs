using Serilog;
using Serilog.Events;

namespace AppSimple.Core.Logging.Impl;

/// <summary>
/// Builds a fully configured <see cref="LoggerConfiguration"/> from <see cref="LoggingOptions"/>,
/// applying enrichers, sinks, and destructuring policies.
/// </summary>
internal static class SerilogLoggerFactory
{
    /// <summary>
    /// Creates and returns a configured <see cref="Serilog.Core.Logger"/> from the provided options.
    /// The caller is responsible for assigning this to <see cref="Log.Logger"/> and disposing it on shutdown.
    /// </summary>
    /// <param name="options">The logging options to apply.</param>
    internal static Serilog.Core.Logger Create(LoggingOptions options)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Is(options.MinimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("Application", options.ApplicationName);

        if (options.EnableConsole)
        {
            config.WriteTo.Console(
                restrictedToMinimumLevel: options.MinimumLevel,
                outputTemplate: options.OutputTemplate);
        }

        if (options.EnableFile)
        {
            var logPath = Path.Combine(options.LogDirectory, "app-.log");
            config.WriteTo.File(
                path: logPath,
                restrictedToMinimumLevel: options.MinimumLevel,
                outputTemplate: options.OutputTemplate,
                rollingInterval: options.RollingInterval,
                retainedFileCountLimit: options.RetainedFileCountLimit,
                shared: true);
        }

        return config.CreateLogger();
    }
}
