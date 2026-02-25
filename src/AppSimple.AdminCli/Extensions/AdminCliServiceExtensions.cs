using AppSimple.AdminCli.Menus;
using AppSimple.AdminCli.Services;
using AppSimple.AdminCli.Services.Impl;
using AppSimple.AdminCli.Session;
using AppSimple.Core.Extensions;
using AppSimple.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        // ── Logging (via Core — file-only so output doesn't pollute the console UI) ──
        services.AddAppLogging(opts =>
        {
            opts.ApplicationName = "AppSimple.AdminCli";
            opts.EnableConsole   = false;
            opts.EnableFile      = config.GetValue("AppLogging:EnableFile", true);
            opts.LogDirectory    = LogPath.Resolve(config.GetValue("AppLogging:LogDirectory", ""))!;
        });

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
