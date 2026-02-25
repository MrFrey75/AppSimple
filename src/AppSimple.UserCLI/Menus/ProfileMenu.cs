using AppSimple.Core.Common.Exceptions;
using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using AppSimple.Core.Services;
using AppSimple.UserCLI.Session;
using AppSimple.UserCLI.UI;

namespace AppSimple.UserCLI.Menus;

/// <summary>
/// Menu for viewing and editing the currently logged-in user's own profile,
/// and changing their password.
/// </summary>
public class ProfileMenu
{
    private readonly IUserService _users;
    private readonly UserSession _session;

    /// <summary>Initializes a new instance of <see cref="ProfileMenu"/>.</summary>
    public ProfileMenu(IUserService users, UserSession session)
    {
        _users   = users;
        _session = session;
    }

    /// <summary>Displays the profile menu and loops until the user selects Back.</summary>
    public async Task ShowAsync()
    {
        while (true)
        {
            var user = _session.CurrentUser!;

            ConsoleUI.Clear();
            ConsoleUI.WriteHeading($"My Profile  ({user.Username})");

            ConsoleUI.WriteMenuItem(1, "View Profile");
            ConsoleUI.WriteMenuItem(2, "Edit Profile");
            ConsoleUI.WriteMenuItem(3, "Change Password");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(3);

            switch (choice)
            {
                case 0: return;
                case 1: await ViewProfileAsync(user); break;
                case 2: await EditProfileAsync(user); break;
                case 3: await ChangePasswordAsync(user); break;
            }
        }
    }

    // ─── View ───────────────────────────────────────────────────────────────

    private static Task ViewProfileAsync(User user)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Profile Details");
        ConsoleUI.WriteUserDetail(user);
        ConsoleUI.Pause();
        return Task.CompletedTask;
    }

    // ─── Edit ───────────────────────────────────────────────────────────────

    private async Task EditProfileAsync(User user)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Edit Profile");
        ConsoleUI.WriteInfo("Press Enter to keep the current value.");
        ConsoleUI.WriteLine();

        user.FirstName   = ConsoleUI.ReadOptionalLine("First Name",    user.FirstName);
        user.LastName    = ConsoleUI.ReadOptionalLine("Last Name",     user.LastName);
        user.PhoneNumber = ConsoleUI.ReadOptionalLine("Phone Number",  user.PhoneNumber);
        user.Bio         = ConsoleUI.ReadOptionalLine("Bio",           user.Bio);

        string? dobInput = ConsoleUI.ReadOptionalLine(
            "Date of Birth (yyyy-MM-dd)", user.DateOfBirth?.ToString("yyyy-MM-dd"));

        if (dobInput is not null &&
            DateTime.TryParse(dobInput, out DateTime dob))
        {
            user.DateOfBirth = dob;
        }

        ConsoleUI.WriteLine();

        try
        {
            await _users.UpdateAsync(user);

            // Refresh session with updated user
            var refreshed = await _users.GetByUidAsync(user.Uid);
            if (refreshed is not null) _session.Login(refreshed, _session.Token!);

            ConsoleUI.WriteSuccess("Profile updated successfully.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Update failed: {ex.Message}");
        }

        ConsoleUI.Pause();
    }

    // ─── Change Password ────────────────────────────────────────────────────

    private async Task ChangePasswordAsync(User user)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Change Password");

        string current = ConsoleUI.ReadPassword("Current Password");
        string newPwd  = ConsoleUI.ReadPassword("New Password");
        string confirm = ConsoleUI.ReadPassword("Confirm New Password");
        ConsoleUI.WriteLine();

        if (newPwd != confirm)
        {
            ConsoleUI.WriteError("New password and confirmation do not match.");
            ConsoleUI.Pause();
            return;
        }

        try
        {
            await _users.ChangePasswordAsync(user.Uid, current, newPwd);
            ConsoleUI.WriteSuccess("Password changed successfully.");
        }
        catch (UnauthorizedException)
        {
            ConsoleUI.WriteError("Current password is incorrect.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed to change password: {ex.Message}");
        }

        ConsoleUI.Pause();
    }
}
