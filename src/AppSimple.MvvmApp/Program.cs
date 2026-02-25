using Avalonia;

namespace AppSimple.MvvmApp;

/// <summary>Application entry point.</summary>
internal static class Program
{
    /// <summary>Main entry point; builds and starts the Avalonia application.</summary>
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>Creates and configures the <see cref="AppBuilder"/>.</summary>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
