using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AppSimple.Core.Auth;
using AppSimple.Core.Constants;
using AppSimple.Core.Extensions;
using AppSimple.Core.Logging;
using AppSimple.DataLib.Db;
using AppSimple.DataLib.Extensions;
using AppSimple.MvvmApp.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppSimple.MvvmApp;

/// <summary>Avalonia <see cref="Application"/> host. Configures DI and creates the main window.</summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <inheritdoc />
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = DatabasePath.Resolve(config[AppConstants.ConfigDatabaseConnectionString]);

        var services = new ServiceCollection();
        services.AddAppLogging(opts =>
        {
            opts.EnableFile    = config.GetValue(AppConstants.ConfigLoggingEnableFile, true);
            opts.LogDirectory  = LogPath.Resolve(config[AppConstants.ConfigLoggingDirectory]);
            opts.ApplicationName = "AppSimple.MvvmApp";
        });
        services.AddCoreServices();
        services.AddJwtAuthentication(opts =>
        {
            opts.Secret            = config[AppConstants.ConfigJwtSecret]   ?? "change-this-secret-in-production-32chars!!";
            opts.Issuer            = config[AppConstants.ConfigJwtIssuer]   ?? AppConstants.AppName;
            opts.Audience          = config[AppConstants.ConfigJwtAudience] ?? AppConstants.AppName;
            opts.ExpirationMinutes = int.TryParse(config[AppConstants.ConfigJwtExpiration], out var exp) ? exp : AppConstants.DefaultJwtExpirationMinutes;
        });
        services.AddDataLibServices(connectionString);
        services.AddMvvmAppServices();
        _serviceProvider = services.BuildServiceProvider();

        // Ensure DB schema + admin seed
        var initializer = _serviceProvider.GetRequiredService<AppSimple.DataLib.Db.DbInitializer>();
        var hasher       = _serviceProvider.GetRequiredService<IPasswordHasher>();
        initializer.Initialize();
        initializer.SeedAdminUser(hasher.Hash(AppConstants.DefaultAdminPassword));

        var _logger = _serviceProvider.GetRequiredService<IAppLogger<App>>();
        _logger.Information("Application initialized with connection string: {ConnectionString}", connectionString);

        // Apply saved theme before creating the main window
        var themeManager = _serviceProvider.GetRequiredService<AppSimple.MvvmApp.Services.ThemeManager>();
        themeManager.ApplySavedTheme();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        base.OnFrameworkInitializationCompleted();

        
    }
}
