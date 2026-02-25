using AppSimple.UserCLI.Menus;
using AppSimple.UserCLI.Session;
using AppSimple.UserCLI.UI;

namespace AppSimple.UserCLI;

/// <summary>
/// Top-level application controller. Drives the login → main menu → logout loop.
/// </summary>
public class App
{
    private readonly LoginMenu _loginMenu;
    private readonly MainMenu _mainMenu;
    private readonly UserSession _session;

    /// <summary>Initializes a new instance of <see cref="App"/>.</summary>
    public App(LoginMenu loginMenu, MainMenu mainMenu, UserSession session)
    {
        _loginMenu = loginMenu;
        _mainMenu  = mainMenu;
        _session   = session;
    }

    /// <summary>
    /// Starts the application loop. Presents login until authenticated,
    /// then shows the main menu until logout. Repeats until the user exits.
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            if (!_session.IsLoggedIn)
            {
                bool exitRequested = await _loginMenu.ShowAsync();
                if (exitRequested) break;
            }
            else
            {
                await _mainMenu.ShowAsync();
                // MainMenu returns when the user logs out — loop back to login
            }
        }

        ConsoleUI.Clear(showHeader: false);
        ConsoleUI.WriteInfo("Thank you for using AppSimple. Goodbye!");
        Console.WriteLine();
    }
}
