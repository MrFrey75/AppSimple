using AppSimple.AdminCli.Services;
using AppSimple.AdminCli.Session;
using AppSimple.AdminCli.UI;
using AppSimple.Core.Logging;

namespace AppSimple.AdminCli.Menus;

/// <summary>
/// The first screen presented. Handles login and application exit.
/// Only allows users with the "Admin" role to proceed.
/// </summary>
public sealed class LoginMenu
{
    private readonly IApiClient _api;
    private readonly AdminSession _session;
    private readonly IAppLogger<LoginMenu> _logger;

    /// <summary>Initializes a new instance of <see cref="LoginMenu"/>.</summary>
    public LoginMenu(IApiClient api, AdminSession session, IAppLogger<LoginMenu> logger)
    {
        _api     = api;
        _session = session;
        _logger  = logger;
        _logger.Debug("LoginMenu initialized") ;
    }

    /// <summary>
    /// Displays the login screen in a loop until the admin successfully logs in or exits.
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
            if (choice == 0) return true;

            ConsoleUI.WriteLine();
            string username = ConsoleUI.ReadLine("Username");
            string password = ConsoleUI.ReadPassword("Password");
            ConsoleUI.WriteLine();

            _logger.Debug("Login attempt for user '{Username}'", username);

            var result = await _api.LoginAsync(username, password);

            if (result is null)
            {
                _logger.Warning("Login failed for user '{Username}' — invalid credentials", username);
                ConsoleUI.WriteError("Invalid credentials. Please try again.");
                ConsoleUI.Pause();
                continue;
            }

            if (!string.Equals(result.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warning("Login denied for user '{Username}' — role is '{Role}', Admin required", username, result.Role);
                ConsoleUI.WriteError("Access denied. This tool is for administrators only.");
                ConsoleUI.Pause();
                continue;
            }

            _session.Login(result.Token, result.Username);
            _logger.Information("Admin '{Username}' logged in successfully", result.Username);
            ConsoleUI.WriteSuccess($"Welcome, {result.Username}!");
            ConsoleUI.Pause();
            return false;
        }
    }
}
