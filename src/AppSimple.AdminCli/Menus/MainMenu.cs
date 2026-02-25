using AppSimple.AdminCli.Session;
using AppSimple.AdminCli.UI;

namespace AppSimple.AdminCli.Menus;

/// <summary>
/// The post-login main menu. Dispatches to user management or system sub-menus
/// and handles logout.
/// </summary>
public sealed class MainMenu
{
    private readonly UsersMenu _usersMenu;
    private readonly SystemMenu _systemMenu;
    private readonly AdminSession _session;

    /// <summary>Initializes a new instance of <see cref="MainMenu"/>.</summary>
    public MainMenu(UsersMenu usersMenu, SystemMenu systemMenu, AdminSession session)
    {
        _usersMenu  = usersMenu;
        _systemMenu = systemMenu;
        _session    = session;
    }

    /// <summary>Displays the main menu and loops until the admin logs out.</summary>
    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUI.Clear();

            if (!Console.IsOutputRedirected)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Logged in as: {_session.Username} [Admin]");
            if (!Console.IsOutputRedirected)
                Console.ResetColor();

            ConsoleUI.WriteSeparator();
            ConsoleUI.WriteLine();
            ConsoleUI.WriteHeading("Main Menu");

            ConsoleUI.WriteMenuItem(1, "User Management",   "create, edit, delete and manage users");
            ConsoleUI.WriteMenuItem(2, "System & Health",   "health check, smoke test, seed users");
            ConsoleUI.WriteBackItem("Log Out");
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(2);

            switch (choice)
            {
                case 0:
                    if (ConsoleUI.Confirm("Are you sure you want to log out?"))
                    {
                        _session.Logout();
                        return;
                    }
                    break;

                case 1:
                    await _usersMenu.ShowAsync();
                    break;

                case 2:
                    await _systemMenu.ShowAsync();
                    break;
            }
        }
    }
}
