using AppSimple.AdminCli.Services;
using AppSimple.AdminCli.Session;
using AppSimple.AdminCli.UI;

namespace AppSimple.AdminCli.Menus;

/// <summary>
/// User management sub-menu. Allows listing, creating, viewing, editing,
/// deleting users, and changing their roles via the WebApi.
/// </summary>
public sealed class UsersMenu
{
    private readonly IApiClient _api;
    private readonly AdminSession _session;

    /// <summary>Initializes a new instance of <see cref="UsersMenu"/>.</summary>
    public UsersMenu(IApiClient api, AdminSession session)
    {
        _api     = api;
        _session = session;
    }

    /// <summary>Displays the user management menu and loops until Back is selected.</summary>
    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.WriteHeading("User Management");

            ConsoleUI.WriteMenuItem(1, "List All Users");
            ConsoleUI.WriteMenuItem(2, "Create New User");
            ConsoleUI.WriteMenuItem(3, "View User Details");
            ConsoleUI.WriteMenuItem(4, "Edit User");
            ConsoleUI.WriteMenuItem(5, "Delete User");
            ConsoleUI.WriteMenuItem(6, "Change User Role");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(6);

            switch (choice)
            {
                case 0: return;
                case 1: await ListUsersAsync(); break;
                case 2: await CreateUserAsync(); break;
                case 3: await ViewUserDetailAsync(); break;
                case 4: await EditUserAsync(); break;
                case 5: await DeleteUserAsync(); break;
                case 6: await ChangeUserRoleAsync(); break;
            }
        }
    }

    // ─── List ───────────────────────────────────────────────────────────────

    private async Task ListUsersAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("All Users");

        var users = await _api.GetAllUsersAsync(_session.Token!);
        ConsoleUI.WriteUserTable(users);
        ConsoleUI.Pause();
    }

    // ─── Create ─────────────────────────────────────────────────────────────

    private async Task CreateUserAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Create New User");

        string username = ConsoleUI.ReadLine("Username");
        string email    = ConsoleUI.ReadLine("Email");
        string password = ConsoleUI.ReadPassword("Password");
        string confirm  = ConsoleUI.ReadPassword("Confirm Password");
        ConsoleUI.WriteLine();

        if (password != confirm)
        {
            ConsoleUI.WriteError("Passwords do not match.");
            ConsoleUI.Pause();
            return;
        }

        var user = await _api.CreateUserAsync(_session.Token!, username, email, password);

        if (user is not null)
            ConsoleUI.WriteSuccess($"User '{user.Username}' created successfully (UID: {user.Uid}).");
        else
            ConsoleUI.WriteError("Failed to create user. The username or email may already be taken.");

        ConsoleUI.Pause();
    }

    // ─── View Details ────────────────────────────────────────────────────────

    private async Task ViewUserDetailAsync()
    {
        var users = await FetchAndPickAsync("View User Details");
        if (users.Selected is null) return;

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading($"User Detail: {users.Selected.Username}");
        ConsoleUI.WriteUserDetail(users.Selected);
        ConsoleUI.Pause();
    }

    // ─── Edit ───────────────────────────────────────────────────────────────

    private async Task EditUserAsync()
    {
        var pick = await FetchAndPickAsync("Edit User");
        if (pick.Selected is null) return;

        var u = pick.Selected;

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading($"Editing: {u.Username}");
        ConsoleUI.WriteInfo("Press Enter to keep the current value.");
        ConsoleUI.WriteLine();

        string? firstName   = ConsoleUI.ReadOptionalLine("First Name",    u.FirstName);
        string? lastName    = ConsoleUI.ReadOptionalLine("Last Name",     u.LastName);
        string? phone       = ConsoleUI.ReadOptionalLine("Phone Number",  u.PhoneNumber);
        string? bio         = ConsoleUI.ReadOptionalLine("Bio",           u.Bio);

        string? dobInput = ConsoleUI.ReadOptionalLine(
            "Date of Birth (yyyy-MM-dd)", u.DateOfBirth?.ToString("yyyy-MM-dd"));

        DateTime? dob = null;
        if (dobInput is not null && DateTime.TryParse(dobInput, out var parsed))
            dob = parsed;
        else if (u.DateOfBirth.HasValue)
            dob = u.DateOfBirth;

        ConsoleUI.WriteLine();

        var req = new UpdateUserRequest
        {
            FirstName   = firstName,
            LastName    = lastName,
            PhoneNumber = phone,
            Bio         = bio,
            DateOfBirth = dob
        };

        var updated = await _api.UpdateUserAsync(_session.Token!, u.Uid, req);

        if (updated is not null)
            ConsoleUI.WriteSuccess($"User '{updated.Username}' updated successfully.");
        else
            ConsoleUI.WriteError("Update failed.");

        ConsoleUI.Pause();
    }

    // ─── Delete ─────────────────────────────────────────────────────────────

    private async Task DeleteUserAsync()
    {
        var pick = await FetchAndPickAsync("Delete User");
        if (pick.Selected is null) return;

        var u = pick.Selected;

        if (u.IsSystem)
        {
            ConsoleUI.WriteError($"Cannot delete system user '{u.Username}'.");
            ConsoleUI.Pause();
            return;
        }

        ConsoleUI.WriteLine();
        ConsoleUI.WriteWarning($"You are about to permanently delete '{u.Username}'. This cannot be undone.");

        if (!ConsoleUI.Confirm($"Delete user '{u.Username}'?"))
        {
            ConsoleUI.WriteInfo("Deletion cancelled.");
            ConsoleUI.Pause();
            return;
        }

        bool ok = await _api.DeleteUserAsync(_session.Token!, u.Uid);

        if (ok)
            ConsoleUI.WriteSuccess($"User '{u.Username}' deleted.");
        else
            ConsoleUI.WriteError("Deletion failed.");

        ConsoleUI.Pause();
    }

    // ─── Change Role ─────────────────────────────────────────────────────────

    private async Task ChangeUserRoleAsync()
    {
        var pick = await FetchAndPickAsync("Change User Role");
        if (pick.Selected is null) return;

        var u = pick.Selected;

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading($"Change Role: {u.Username}");
        ConsoleUI.WriteInfo($"Current role: {(u.Role == 1 ? "Admin" : "User")}");
        ConsoleUI.WriteLine();
        ConsoleUI.WriteMenuItem(1, "Admin");
        ConsoleUI.WriteMenuItem(2, "User");
        ConsoleUI.WriteBackItem();
        ConsoleUI.WriteLine();

        int choice = ConsoleUI.ReadMenuChoice(2);
        if (choice == 0) return;

        int newRole = choice == 1 ? 1 : 0;
        bool ok = await _api.SetUserRoleAsync(_session.Token!, u.Uid, newRole);

        if (ok)
            ConsoleUI.WriteSuccess($"Role for '{u.Username}' set to {(newRole == 1 ? "Admin" : "User")}.");
        else
            ConsoleUI.WriteError("Role change failed.");

        ConsoleUI.Pause();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private async Task<(List<UserDto> All, UserDto? Selected)> FetchAndPickAsync(string heading)
    {
        var users = (await _api.GetAllUsersAsync(_session.Token!)).ToList();

        if (users.Count == 0)
        {
            ConsoleUI.Clear();
            ConsoleUI.WriteHeading(heading);
            ConsoleUI.WriteInfo("No users available.");
            ConsoleUI.Pause();
            return (users, null);
        }

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading(heading);
        ConsoleUI.WriteUserTable(users);
        ConsoleUI.WriteBackItem();
        ConsoleUI.WriteLine();

        int choice = ConsoleUI.ReadMenuChoice(users.Count);
        return (users, choice == 0 ? null : users[choice - 1]);
    }
}
