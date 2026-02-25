using AppSimple.AdminCli.Menus;
using AppSimple.AdminCli.Session;
using AppSimple.AdminCli.UI;
using AppSimple.Core.Logging;

namespace AppSimple.AdminCli;

/// <summary>
/// Top-level application controller. Drives the login → main menu → logout loop.
/// </summary>
public sealed class App
{
    private readonly LoginMenu _loginMenu;
    private readonly MainMenu _mainMenu;
    private readonly AdminSession _session;
    private readonly IAppLogger<App> _logger;

    /// <summary>Initializes a new instance of <see cref="App"/>.</summary>
    public App(LoginMenu loginMenu, MainMenu mainMenu, AdminSession session, IAppLogger<App> logger)
    {
        _loginMenu = loginMenu;
        _mainMenu  = mainMenu;
        _session   = session;
        _logger    = logger;
    }

    /// <summary>
    /// Starts the application loop. Shows login until authenticated,
    /// then shows the main menu until logout. Repeats until the user exits.
    /// </summary>
    public async Task RunAsync()
    {
        _logger.Information("AdminCli started");

        while (true)
        {
            if (!_session.IsLoggedIn)
            {
                bool exit = await _loginMenu.ShowAsync();
                if (exit)
                {
                    _logger.Information("User exited from login screen");
                    break;
                }
            }
            else
            {
                await _mainMenu.ShowAsync();
            }
        }

        _logger.Information("AdminCli shutting down");
        ConsoleUI.Clear(showHeader: false);
        ConsoleUI.WriteInfo("Goodbye!");
        Console.WriteLine();
    }
}
