using AppSimple.Core.Services;
using AppSimple.UserCLI.Session;
using AppSimple.UserCLI.UI;

namespace AppSimple.UserCLI.Menus;

/// <summary>
/// The first screen the user sees. Handles login and application exit.
/// </summary>
public class LoginMenu
{
    private readonly IAuthService _auth;
    private readonly IUserService _users;
    private readonly UserSession _session;

    /// <summary>Initializes a new instance of <see cref="LoginMenu"/>.</summary>
    public LoginMenu(IAuthService auth, IUserService users, UserSession session)
    {
        _auth    = auth;
        _users   = users;
        _session = session;
    }

    /// <summary>
    /// Displays the login screen in a loop until the user successfully logs in
    /// or chooses to exit.
    /// </summary>
    /// <returns><c>true</c> if the user requested exit; <c>false</c> if login succeeded.</returns>
    public async Task<bool> ShowAsync()
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.WriteHeading("Welcome");
            ConsoleUI.WriteMenuItem(1, "Log In");
            ConsoleUI.WriteBackItem("Exit");
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(1);

            if (choice == 0) return true;   // exit requested

            ConsoleUI.WriteLine();
            string username = ConsoleUI.ReadLine("Username");
            string password = ConsoleUI.ReadPassword("Password");
            ConsoleUI.WriteLine();

            var result = await _auth.LoginAsync(username, password);

            if (result.Succeeded && result.Token is not null)
            {
                var user = await _users.GetByUsernameAsync(username);
                if (user is not null)
                {
                    _session.Login(user, result.Token);
                    ConsoleUI.WriteSuccess($"Welcome back, {user.Username}!");
                    ConsoleUI.Pause();
                    return false;   // logged in — hand control to MainMenu
                }
            }

            ConsoleUI.WriteError(result.Message);
            ConsoleUI.Pause();
        }
    }
}
