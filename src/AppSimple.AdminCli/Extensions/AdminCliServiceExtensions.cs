using AppSimple.AdminCli.Menus;
using AppSimple.AdminCli.Services;
using AppSimple.AdminCli.Services.Impl;
using AppSimple.AdminCli.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AppSimple.AdminCli.Extensions;

/// <summary>Extension methods for registering AdminCli services with the DI container.</summary>
public static class AdminCliServiceExtensions
{
    /// <summary>
    /// Registers all services required by the Admin CLI application.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="config">The application configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAdminCliServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ── Serilog file logging ───────────────────────────────────────────
        var logDir     = LogPath.Resolve(config["AppLogging:LogDirectory"]);
        var enableFile = config.GetValue("AppLogging:EnableFile", true);

        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext();

        if (enableFile)
            logConfig.WriteTo.File(
                Path.Combine(logDir, "admincli-.log"),
                rollingInterval: RollingInterval.Day);

        Log.Logger = logConfig.CreateLogger();

        services.AddLogging(lb => lb.AddSerilog(Log.Logger, dispose: true));

        // ── Typed HttpClient ───────────────────────────────────────────────
        var baseUrl = config["WebApi:BaseUrl"] ?? "http://localhost:5157";
        services.AddHttpClient<IApiClient, ApiClient>(c =>
            c.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"));

        // ── Application services ───────────────────────────────────────────
        services.AddSingleton<AdminSession>();
        services.AddTransient<LoginMenu>();
        services.AddTransient<MainMenu>();
        services.AddTransient<UsersMenu>();
        services.AddTransient<SystemMenu>();
        services.AddTransient<App>();

        return services;
    }
}
