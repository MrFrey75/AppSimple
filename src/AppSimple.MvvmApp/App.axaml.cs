using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AppSimple.Core.Auth;
using AppSimple.Core.Extensions;
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

        var connectionString = config["ConnectionString"] ?? "Data Source=appsimple.db";

        var services = new ServiceCollection();
        services.AddCoreServices();
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
