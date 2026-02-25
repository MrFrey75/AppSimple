using AppSimple.Core.Extensions;
using AppSimple.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;

namespace AppSimple.Core.Tests.Logging;

/// <summary>
/// Tests for <see cref="CoreServiceExtensions.AddAppLogging"/>.
/// </summary>
public sealed class AddAppLoggingTests
{
    private sealed class ServiceA { }
    private sealed class ServiceB { }

    [Fact]
    public void AddAppLogging_Returns_SameServiceCollection()
    {
        var services = new ServiceCollection();
        var returned = services.AddAppLogging();
        Assert.Same(services, returned);
    }

    [Fact]
    public void AddAppLogging_RegistersIAppLogger_AsOpenGeneric()
    {
        var services = new ServiceCollection();
        services.AddAppLogging();

        var registration = services.FirstOrDefault(sd =>
            sd.ServiceType.IsGenericTypeDefinition &&
            sd.ServiceType == typeof(IAppLogger<>));

        Assert.NotNull(registration);
    }

    [Fact]
    public void AddAppLogging_CanResolve_TypedLogger()
    {
        var services = new ServiceCollection();
        services.AddAppLogging();
        var provider = services.BuildServiceProvider();

        var logger = provider.GetService<IAppLogger<AddAppLoggingTests>>();
        Assert.NotNull(logger);
    }

    [Fact]
    public void AddAppLogging_DifferentGenericTypes_ResolveDistinctInstances()
    {
        var services = new ServiceCollection();
        services.AddAppLogging();
        var provider = services.BuildServiceProvider();

        var loggerA = provider.GetRequiredService<IAppLogger<ServiceA>>();
        var loggerB = provider.GetRequiredService<IAppLogger<ServiceB>>();

        Assert.NotSame(loggerA, loggerB);
    }

    [Fact]
    public void AddAppLogging_IsTransient_ReturnsNewInstanceEachTime()
    {
        var services = new ServiceCollection();
        services.AddAppLogging();
        var provider = services.BuildServiceProvider();

        var first  = provider.GetRequiredService<IAppLogger<ServiceA>>();
        var second = provider.GetRequiredService<IAppLogger<ServiceA>>();

        Assert.NotSame(first, second);
    }

    [Fact]
    public void AddAppLogging_DefaultOptions_DoNotThrow()
    {
        var services = new ServiceCollection();
        var ex = Record.Exception(() => services.AddAppLogging());
        Assert.Null(ex);
    }

    [Fact]
    public void AddAppLogging_CustomOptions_AreApplied_WithoutThrowing()
    {
        var services = new ServiceCollection();
        var ex = Record.Exception(() => services.AddAppLogging(opts =>
        {
            opts.EnableConsole = false;
            opts.EnableFile    = false;
            opts.MinimumLevel  = LogEventLevel.Warning;
            opts.ApplicationName = "TestApp";
        }));
        Assert.Null(ex);
    }
}
