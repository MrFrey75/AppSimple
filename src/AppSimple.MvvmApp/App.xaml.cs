using System.Windows;
using AppSimple.Core.Auth;
using AppSimple.Core.Extensions;
using AppSimple.DataLib.Db;
using AppSimple.DataLib.Extensions;
using AppSimple.MvvmApp.Session;
using AppSimple.MvvmApp.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AppSimple.MvvmApp;

/// <summary>
/// Application entry point. Bootstraps the DI container, initialises the
/// database, and shows <see cref="MainWindow"/>.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        ConfigureServices(services, configuration);
        _serviceProvider = services.BuildServiceProvider();

        // ── Database bootstrap ────────────────────────────────────────────
        var initializer = _serviceProvider.GetRequiredService<DbInitializer>();
        initializer.Initialize();

        var hasher = _serviceProvider.GetRequiredService<IPasswordHasher>();
        initializer.SeedAdminUser(hasher.Hash("Admin123!"));

        // ── Show main window ──────────────────────────────────────────────
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration cfg)
    {
        // Logging — file only so logs don't appear over the UI
        services.AddAppLogging(opts =>
        {
            opts.ApplicationName = "AppSimple.MvvmApp";
            opts.EnableConsole   = false;
            opts.EnableFile      = cfg.GetValue("AppLogging:EnableFile", true);
            opts.LogDirectory    = cfg.GetValue("AppLogging:LogDirectory", "logs")!;
        });

        // Core services — validators, password hasher, user service, auth service
        services.AddCoreServices();

        // JWT token service
        services.AddJwtAuthentication(opts =>
            cfg.GetSection("Jwt").Bind(opts));

        // Data access — SQLite + Dapper
        services.AddDataLibServices(
            cfg["Database:ConnectionString"] ?? "Data Source=appsimple.db");

        // Session + ViewModels (singletons so navigation retains state)
        services.AddSingleton<UserSession>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<ProfileViewModel>();
        services.AddSingleton<UsersViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        // Shell window
        services.AddTransient<MainWindow>();
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
