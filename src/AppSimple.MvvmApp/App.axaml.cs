using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AppSimple.Core.Auth;
using AppSimple.Core.Extensions;
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

        var connectionString = DatabasePath.Resolve(config["Database:ConnectionString"]);

        var services = new ServiceCollection();
        services.AddAppLogging(opts =>
        {
            opts.EnableFile    = config.GetSection("AppLogging")["EnableFile"]    == "true";
            opts.LogDirectory  = config.GetSection("AppLogging")["LogDirectory"]  ?? "logs";
        });
        services.AddCoreServices();
        services.AddJwtAuthentication(opts =>
        {
            opts.Secret            = config["Jwt:Secret"]   ?? "change-this-secret-in-production-32chars!!";
            opts.Issuer            = config["Jwt:Issuer"]   ?? "AppSimple";
            opts.Audience          = config["Jwt:Audience"] ?? "AppSimple";
            opts.ExpirationMinutes = int.TryParse(config["Jwt:ExpirationMinutes"], out var exp) ? exp : 480;
        });
        services.AddDataLibServices(connectionString);
        services.AddMvvmAppServices();
        _serviceProvider = services.BuildServiceProvider();

        // Ensure DB schema + admin seed
        var initializer = _serviceProvider.GetRequiredService<AppSimple.DataLib.Db.DbInitializer>();
        var hasher       = _serviceProvider.GetRequiredService<IPasswordHasher>();
        initializer.Initialize();
        initializer.SeedAdminUser(hasher.Hash("Admin123!"));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        base.OnFrameworkInitializationCompleted();

        
    }
}
