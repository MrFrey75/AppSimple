using AppSimple.AdminCli.Menus;
using AppSimple.AdminCli.Session;
using AppSimple.AdminCli.UI;

namespace AppSimple.AdminCli;

/// <summary>
/// Top-level application controller. Drives the login → main menu → logout loop.
/// </summary>
public sealed class App
{
    private readonly LoginMenu _loginMenu;
    private readonly MainMenu _mainMenu;
    private readonly AdminSession _session;

    /// <summary>Initializes a new instance of <see cref="App"/>.</summary>
    public App(LoginMenu loginMenu, MainMenu mainMenu, AdminSession session)
    {
        _loginMenu = loginMenu;
        _mainMenu  = mainMenu;
        _session   = session;
    }

    /// <summary>
    /// Starts the application loop. Shows login until authenticated,
    /// then shows the main menu until logout. Repeats until the user exits.
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            if (!_session.IsLoggedIn)
            {
                bool exit = await _loginMenu.ShowAsync();
                if (exit) break;
            }
            else
            {
                await _mainMenu.ShowAsync();
            }
        }

        ConsoleUI.Clear(showHeader: false);
        ConsoleUI.WriteInfo("Goodbye!");
        Console.WriteLine();
    }
}
