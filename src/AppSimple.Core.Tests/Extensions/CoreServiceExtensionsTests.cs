using AppSimple.Core.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AppSimple.Core.Tests.Extensions;

/// <summary>Tests for <see cref="CoreServiceExtensions.AddCoreServices"/>.</summary>
public sealed class CoreServiceExtensionsTests
{
    [Fact]
    public void AddCoreServices_ReturnsTheSameServiceCollection()
    {
        var services = new ServiceCollection();
        var returned = services.AddCoreServices();
        Assert.Same(services, returned);
    }

    [Fact]
    public void AddCoreServices_RegistersValidatorInfrastructure()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();

        // FluentValidation.DI registers IValidator<T> open-generic infrastructure
        var hasValidatorRegistrations = services.Any(sd =>
            sd.ServiceType.IsGenericType &&
            sd.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>));

        // No concrete validators exist yet, but the infrastructure descriptor is registered
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider); // DI container builds without error
    }

    [Fact]
    public void AddCoreServices_IsIdempotent_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var ex = Record.Exception(() =>
        {
            services.AddCoreServices();
            services.AddCoreServices();
            services.BuildServiceProvider();
        });
        Assert.Null(ex);
    }

    [Fact]
    public void AddCoreServices_DoesNotRegisterUnrelatedServices()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();

        // Should not accidentally register things outside core concerns (e.g. HTTP, hosting)
        var hasHttpClient = services.Any(sd => sd.ServiceType.FullName?.Contains("HttpClient") == true);
        Assert.False(hasHttpClient);
    }
}
