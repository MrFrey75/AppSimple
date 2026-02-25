using AppSimple.Core.Common.Exceptions;
using AppSimple.Core.Constants;
using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using AppSimple.Core.Services;
using AppSimple.UserCLI.Session;
using AppSimple.UserCLI.UI;

namespace AppSimple.UserCLI.Menus;

/// <summary>
/// Admin-only menu for managing all application users:
/// list, create, edit, delete, and database reset.
/// </summary>
public class AdminMenu
{
    private readonly IUserService _users;
    private readonly IDatabaseResetService _resetService;
    private readonly UserSession _session;

    /// <summary>Initializes a new instance of <see cref="AdminMenu"/>.</summary>
    public AdminMenu(IUserService users, IDatabaseResetService resetService, UserSession session)
    {
        _users        = users;
        _resetService = resetService;
        _session      = session;
    }

    /// <summary>Displays the admin user-management menu and loops until Back is selected.</summary>
    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.WriteHeading("User Management");

            ConsoleUI.WriteMenuItem(1, "List All Users");
            ConsoleUI.WriteMenuItem(2, "Create New User");
            ConsoleUI.WriteMenuItem(3, "Edit a User");
            ConsoleUI.WriteMenuItem(4, "Delete a User");
            ConsoleUI.WriteMenuGroupLabel("Danger Zone");
            ConsoleUI.WriteMenuItem(5, "Reset & Reseed Database", "⚠ erases ALL data");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(5);

            switch (choice)
            {
                case 0: return;
                case 1: await ListUsersAsync(); break;
                case 2: await CreateUserAsync(); break;
                case 3: await EditUserAsync(); break;
                case 4: await DeleteUserAsync(); break;
                case 5: await ResetDatabaseAsync(); break;
            }
        }
    }

    // ─── List ───────────────────────────────────────────────────────────────

    private async Task ListUsersAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("All Users");

        var users = await _users.GetAllAsync();
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

        ConsoleUI.WriteInfo("Assign role:");
        ConsoleUI.WriteMenuItem(1, "User");
        ConsoleUI.WriteMenuItem(2, "Admin");
        ConsoleUI.WriteLine();
        int roleChoice = ConsoleUI.ReadMenuChoice(2);
        UserRole role = roleChoice == 2 ? UserRole.Admin : UserRole.User;
        ConsoleUI.WriteLine();

        try
        {
            var user = await _users.CreateAsync(username, email, password);

            // Apply role if not the default
            if (role != UserRole.User)
            {
                user.Role = role;
                await _users.UpdateAsync(user);
            }

            ConsoleUI.WriteSuccess($"User '{username}' created successfully.");
        }
        catch (DuplicateEntityException ex)
        {
            ConsoleUI.WriteError($"Conflict: {ex.Message}");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Creation failed: {ex.Message}");
        }

        ConsoleUI.Pause();
    }

    // ─── Edit ───────────────────────────────────────────────────────────────

    private async Task EditUserAsync()
    {
        var users = (await _users.GetAllAsync()).ToList();
        if (users.Count == 0)
        {
            ConsoleUI.WriteInfo("No users available to edit.");
            ConsoleUI.Pause();
            return;
        }

        var selected = await PickUserAsync(users, "Edit a User");
        if (selected is null) return;

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading($"Editing: {selected.Username}");
        ConsoleUI.WriteInfo("Press Enter to keep the current value.");
        ConsoleUI.WriteLine();

        selected.FirstName   = ConsoleUI.ReadOptionalLine("First Name",    selected.FirstName);
        selected.LastName    = ConsoleUI.ReadOptionalLine("Last Name",     selected.LastName);
        selected.PhoneNumber = ConsoleUI.ReadOptionalLine("Phone Number",  selected.PhoneNumber);
        selected.Bio         = ConsoleUI.ReadOptionalLine("Bio",           selected.Bio);

        string? dobInput = ConsoleUI.ReadOptionalLine(
            "Date of Birth (yyyy-MM-dd)", selected.DateOfBirth?.ToString("yyyy-MM-dd"));

        if (dobInput is not null && DateTime.TryParse(dobInput, out DateTime dob))
            selected.DateOfBirth = dob;

        ConsoleUI.WriteLine();

        ConsoleUI.WriteInfo("Set active status:");
        ConsoleUI.WriteMenuItem(1, "Active");
        ConsoleUI.WriteMenuItem(2, "Inactive");
        ConsoleUI.WriteLine();
        int activeChoice = ConsoleUI.ReadMenuChoice(2);
        if (activeChoice != 0) selected.IsActive = activeChoice == 1;
        ConsoleUI.WriteLine();

        ConsoleUI.WriteInfo("Set role:");
        ConsoleUI.WriteMenuItem(1, "User");
        ConsoleUI.WriteMenuItem(2, "Admin");
        ConsoleUI.WriteLine();
        int roleChoice = ConsoleUI.ReadMenuChoice(2);
        if (roleChoice != 0) selected.Role = roleChoice == 2 ? UserRole.Admin : UserRole.User;
        ConsoleUI.WriteLine();

        try
        {
            await _users.UpdateAsync(selected);
            ConsoleUI.WriteSuccess($"User '{selected.Username}' updated.");
        }
        catch (SystemEntityException)
        {
            ConsoleUI.WriteError("Cannot modify a system user.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Update failed: {ex.Message}");
        }

        ConsoleUI.Pause();
    }

    // ─── Delete ─────────────────────────────────────────────────────────────

    private async Task DeleteUserAsync()
    {
        var users = (await _users.GetAllAsync()).ToList();
        if (users.Count == 0)
        {
            ConsoleUI.WriteInfo("No users available to delete.");
            ConsoleUI.Pause();
            return;
        }

        var selected = await PickUserAsync(users, "Delete a User");
        if (selected is null) return;

        ConsoleUI.WriteLine();
        ConsoleUI.WriteWarning($"You are about to delete user '{selected.Username}'. This cannot be undone.");

        if (!ConsoleUI.Confirm("Are you sure?"))
        {
            ConsoleUI.WriteInfo("Deletion cancelled.");
            ConsoleUI.Pause();
            return;
        }

        ConsoleUI.WriteLine();

        try
        {
            await _users.DeleteAsync(selected.Uid);
            ConsoleUI.WriteSuccess($"User '{selected.Username}' deleted.");
        }
        catch (SystemEntityException)
        {
            ConsoleUI.WriteError("Cannot delete a system user.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Deletion failed: {ex.Message}");
        }

        ConsoleUI.Pause();
    }

    // ─── Reset & Reseed ─────────────────────────────────────────────────────

    private async Task ResetDatabaseAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Reset & Reseed Database");
        ConsoleUI.WriteLine();
        ConsoleUI.WriteWarning("⚠  WARNING: This operation is IRREVERSIBLE.");
        ConsoleUI.WriteWarning("   ALL user accounts will be permanently deleted.");
        ConsoleUI.WriteWarning("   The database will be reseeded with default data.");
        ConsoleUI.WriteWarning($"  You will be logged out. Log back in as '{AppConstants.DefaultAdminUsername}' / '{AppConstants.DefaultAdminPassword}'.");
        ConsoleUI.WriteLine();

        if (!ConsoleUI.Confirm("Type YES to confirm the reset"))
        {
            ConsoleUI.WriteInfo("Reset cancelled.");
            ConsoleUI.Pause();
            return;
        }

        ConsoleUI.WriteLine();
        ConsoleUI.WriteInfo("Resetting database...");

        try
        {
            await _resetService.ResetAndReseedAsync();
            ConsoleUI.WriteSuccess("Database reset and reseeded successfully.");
            ConsoleUI.WriteInfo($"Default admin: '{AppConstants.DefaultAdminUsername}' / '{AppConstants.DefaultAdminPassword}'");
            ConsoleUI.WriteInfo($"Sample users: alice, bob, carol (password: {AppConstants.DefaultSamplePassword})");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Reset failed: {ex.Message}");
            ConsoleUI.Pause();
            return;
        }

        ConsoleUI.WriteLine();
        ConsoleUI.WriteWarning("You have been logged out. Please log in again.");
        ConsoleUI.Pause();

        // Force logout — session is now invalid
        _session.Logout();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Displays the user table, prompts for a selection, and returns the chosen user,
    /// or <c>null</c> if the user selected Back.
    /// </summary>
    private static Task<User?> PickUserAsync(List<User> users, string heading)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading(heading);
        ConsoleUI.WriteUserTable(users);
        ConsoleUI.WriteBackItem();
        ConsoleUI.WriteLine();

        int choice = ConsoleUI.ReadMenuChoice(users.Count);
        return Task.FromResult(choice == 0 ? null : users[choice - 1]);
    }
}
