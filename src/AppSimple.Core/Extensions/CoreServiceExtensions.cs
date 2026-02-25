using AppSimple.Core.Auth;
using AppSimple.Core.Auth.Impl;
using AppSimple.Core.Config;
using AppSimple.Core.Config.Impl;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Logging.Impl;
using AppSimple.Core.Models;
using AppSimple.Core.Services;
using AppSimple.Core.Services.Impl;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AppSimple.Core.Extensions;

/// <summary>
/// Extension methods for registering <c>AppSimple.Core</c> services with the DI container.
/// </summary>
public static class CoreServiceExtensions
{
    /// <summary>
    /// Registers all core services: validators, password hasher, and application services.
    /// </summary>
    /// <remarks>
    /// Call <see cref="AddJwtAuthentication"/> separately if JWT support is required,
    /// and <see cref="AddAppLogging"/> to configure structured logging.
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Config
        services.AddSingleton<IAppConfigService>(_ =>
            new AppConfigService(AppConfigPath.Resolve()));

        // Validators
        services.AddValidatorsFromAssemblyContaining<User>();

        // Auth
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // Application services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserQueryService>(sp => sp.GetRequiredService<IUserService>());
        services.AddScoped<IUserCommandService>(sp => sp.GetRequiredService<IUserService>());
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>
    /// Registers JWT authentication services and configures <see cref="JwtOptions"/>.
    /// Must be called after <see cref="AddCoreServices"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Action to configure <see cref="JwtOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddJwtAuthentication(opts => {
    ///     opts.Secret            = "your-32-char-minimum-secret-key!!";
    ///     opts.ExpirationMinutes = 60;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        Action<JwtOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        return services;
    }

    /// <summary>
    /// Configures the Serilog global logger and registers <see cref="IAppLogger{T}"/> for DI.
    /// Call this once at application startup before any logging occurs.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional action to customise <see cref="LoggingOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddAppLogging(opts => {
    ///     opts.EnableConsole = true;
    ///     opts.EnableFile    = true;
    ///     opts.MinimumLevel  = LogEventLevel.Debug;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAppLogging(
        this IServiceCollection services,
        Action<LoggingOptions>? configure = null)
    {
        var options = new LoggingOptions();
        configure?.Invoke(options);

        // Build and assign the global Serilog logger
        Log.Logger = SerilogLoggerFactory.Create(options);

        // Register IAppLogger<T> as open-generic transient so every class gets its own SourceContext
        services.AddTransient(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));

        return services;
    }
}
