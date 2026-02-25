using AppSimple.AdminCli.Menus;
using AppSimple.AdminCli.Services;
using AppSimple.AdminCli.Services.Impl;
using AppSimple.AdminCli.Session;
using AppSimple.Core.Auth;
using AppSimple.Core.Constants;
using AppSimple.Core.Extensions;
using AppSimple.Core.Logging;
using AppSimple.DataLib.Db;
using AppSimple.DataLib.Extensions;
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
            opts.EnableFile      = config.GetValue(AppConstants.ConfigLoggingEnableFile, true);
            opts.LogDirectory    = LogPath.Resolve(config.GetValue(AppConstants.ConfigLoggingDirectory, ""))!;
        });

        // ── Core + DataLib (for direct DB access — reset/reseed only) ─────
        services.AddCoreServices();
        services.AddDataLibServices(
            DatabasePath.Resolve(config[AppConstants.ConfigDatabaseConnectionString]));

        // ── Typed HttpClient ───────────────────────────────────────────────
        var baseUrl = config[AppConstants.ConfigWebApiBaseUrl] ?? AppConstants.DefaultWebApiBaseUrl;
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
