using AppSimple.Core.Logging;
using AppSimple.Core.Logging.Impl;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;

namespace AppSimple.Core.Tests.Logging;

/// <summary>
/// Tests for <see cref="SerilogAppLogger{T}"/> and its interaction with the Serilog pipeline.
/// Uses an in-memory sink so no I/O is performed.
/// </summary>
public sealed class SerilogAppLoggerTests : IDisposable
{
    private readonly InMemorySink _sink = new();
    private sealed class TestContext { }

    public SerilogAppLoggerTests()
    {
        // Configure a fresh static logger backed by the in-memory sink for each test class
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Sink(_sink)
            .CreateLogger();
    }

    private SerilogAppLogger<TestContext> CreateLogger() => new();

    // -------------------------------------------------------------------------
    // IsEnabled
    // -------------------------------------------------------------------------

    [Fact]
    public void IsEnabled_ReturnsTrue_ForEnabledLevel()
    {
        var logger = CreateLogger();
        Assert.True(logger.IsEnabled(LogEventLevel.Information));
    }

    [Fact]
    public void IsEnabled_ReturnsFalse_ForLevelBelowMinimum()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.Sink(_sink)
            .CreateLogger();

        var logger = CreateLogger();
        Assert.False(logger.IsEnabled(LogEventLevel.Debug));
    }

    // -------------------------------------------------------------------------
    // Debug
    // -------------------------------------------------------------------------

    [Fact]
    public void Debug_WritesEvent_AtDebugLevel()
    {
        var logger = CreateLogger();
        logger.Debug("Debug message {Value}", 42);

        var evt = Assert.Single(_sink.LogEvents);
        Assert.Equal(LogEventLevel.Debug, evt.Level);
    }

    // -------------------------------------------------------------------------
    // Information
    // -------------------------------------------------------------------------

    [Fact]
    public void Information_WritesEvent_AtInformationLevel()
    {
        var logger = CreateLogger();
        logger.Information("Info {User}", "alice");

        var evt = Assert.Single(_sink.LogEvents);
        Assert.Equal(LogEventLevel.Information, evt.Level);
    }

    [Fact]
    public void Information_RenderedMessage_ContainsPropertyValue()
    {
        var logger = CreateLogger();
        logger.Information("Hello {Name}", "World");

        var evt = Assert.Single(_sink.LogEvents);
        Assert.Contains("World", evt.RenderMessage());
    }

    // -------------------------------------------------------------------------
    // Warning
    // -------------------------------------------------------------------------

    [Fact]
    public void Warning_WritesEvent_AtWarningLevel()
    {
        var logger = CreateLogger();
        logger.Warning("Something suspicious");

        var evt = Assert.Single(_sink.LogEvents);
        Assert.Equal(LogEventLevel.Warning, evt.Level);
    }

    // -------------------------------------------------------------------------
    // Error (no exception)
    // -------------------------------------------------------------------------

    [Fact]
    public void Error_WritesEvent_AtErrorLevel()
    {
        var logger = CreateLogger();
        logger.Error("Boom");

        var evt = Assert.Single(_sink.LogEvents);
        Assert.Equal(LogEventLevel.Error, evt.Level);
    }

    // -------------------------------------------------------------------------
    // Error (with exception)
    // -------------------------------------------------------------------------

    [Fact]
    public void Error_WithException_AttachesException()
    {
        var logger = CreateLogger();
        var ex = new InvalidOperationException("test error");
        logger.Error(ex, "Something went wrong");

        var evt = Assert.Single(_sink.LogEvents);
        Assert.Equal(LogEventLevel.Error, evt.Level);
        Assert.NotNull(evt.Exception);
        Assert.IsType<InvalidOperationException>(evt.Exception);
    }

    // -------------------------------------------------------------------------
    // Fatal (no exception)
    // -------------------------------------------------------------------------

    [Fact]
    public void Fatal_WritesEvent_AtFatalLevel()
    {
        var logger = CreateLogger();
        logger.Fatal("Critical failure");

        var evt = Assert.Single(_sink.LogEvents);
        Assert.Equal(LogEventLevel.Fatal, evt.Level);
    }

    // -------------------------------------------------------------------------
    // Fatal (with exception)
    // -------------------------------------------------------------------------

    [Fact]
    public void Fatal_WithException_AttachesException()
    {
        var logger = CreateLogger();
        var ex = new Exception("fatal");
        logger.Fatal(ex, "Unrecoverable");

        var evt = Assert.Single(_sink.LogEvents);
        Assert.Equal(LogEventLevel.Fatal, evt.Level);
        Assert.Same(ex, evt.Exception);
    }

    // -------------------------------------------------------------------------
    // SourceContext enrichment
    // -------------------------------------------------------------------------

    [Fact]
    public void Logger_EnrichesEvents_WithSourceContext()
    {
        var logger = CreateLogger();
        logger.Information("context test");

        var evt = Assert.Single(_sink.LogEvents);
        Assert.True(evt.Properties.ContainsKey("SourceContext"));
        var ctx = evt.Properties["SourceContext"].ToString();
        Assert.Contains(nameof(TestContext), ctx);
    }

    // -------------------------------------------------------------------------
    // Multiple events
    // -------------------------------------------------------------------------

    [Fact]
    public void MultipleLogCalls_ProduceMultipleEvents()
    {
        var logger = CreateLogger();
        logger.Debug("one");
        logger.Information("two");
        logger.Warning("three");

        Assert.Equal(3, _sink.LogEvents.Count());
    }

    public void Dispose() => Log.CloseAndFlush();
}
