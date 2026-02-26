using AppSimple.Core.Enums;
using AppSimple.UserCLI.Session;
using AppSimple.UserCLI.UI;

namespace AppSimple.UserCLI.Menus;

/// <summary>
/// The post-login main menu. Builds the menu dynamically based on the logged-in
/// user's role and permissions, then dispatches to sub-menus.
/// </summary>
public class MainMenu
{
    private readonly ProfileMenu _profileMenu;
    private readonly AdminMenu _adminMenu;
    private readonly NotesMenu _notesMenu;
    private readonly ContactsMenu _contactsMenu;
    private readonly UserSession _session;

    /// <summary>Initializes a new instance of <see cref="MainMenu"/>.</summary>
    public MainMenu(ProfileMenu profileMenu, AdminMenu adminMenu, NotesMenu notesMenu, ContactsMenu contactsMenu, UserSession session)
    {
        _profileMenu  = profileMenu;
        _adminMenu    = adminMenu;
        _notesMenu    = notesMenu;
        _contactsMenu = contactsMenu;
        _session      = session;
    }

    /// <summary>
    /// Displays the main menu and loops until the user logs out.
    /// </summary>
    public async Task ShowAsync()
    {
        while (true)
        {
            var user = _session.CurrentUser!;
            bool isAdmin = user.Role == UserRole.Admin;

            ConsoleUI.Clear();

            // User context bar
            string roleLabel = isAdmin ? " [Admin]" : " [User]";
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Logged in as: {user.Username}{roleLabel}");
            Console.ResetColor();
            ConsoleUI.WriteSeparator();
            ConsoleUI.WriteLine();

            ConsoleUI.WriteHeading("Main Menu");

            // Items available to all users
            ConsoleUI.WriteMenuItem(1, "My Profile",  "view and edit your profile");
            ConsoleUI.WriteMenuItem(2, "My Notes",    "create and manage notes");
            ConsoleUI.WriteMenuItem(3, "My Contacts", "manage your contact book");

            // Admin-only section
            if (isAdmin)
            {
                ConsoleUI.WriteMenuGroupLabel("Administration");
                ConsoleUI.WriteMenuItem(4, "User Management", "create, edit, delete users");
            }

            ConsoleUI.WriteBackItem("Log Out");
            ConsoleUI.WriteLine();

            int maxChoice = isAdmin ? 4 : 3;
            int choice = ConsoleUI.ReadMenuChoice(maxChoice);

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
                    await _profileMenu.ShowAsync();
                    break;

                case 2:
                    await _notesMenu.ShowAsync();
                    break;

                case 3:
                    await _contactsMenu.ShowAsync();
                    break;

                case 4 when isAdmin:
                    await _adminMenu.ShowAsync();
                    break;
            }
        }
    }
}
