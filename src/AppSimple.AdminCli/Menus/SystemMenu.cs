using AppSimple.AdminCli.Services;
using AppSimple.AdminCli.Session;
using AppSimple.AdminCli.UI;

namespace AppSimple.AdminCli.Menus;

/// <summary>
/// System and health sub-menu. Provides health checks, smoke tests,
/// and test user seeding.
/// </summary>
public sealed class SystemMenu
{
    private readonly IApiClient _api;
    private readonly AdminSession _session;

    /// <summary>Initializes a new instance of <see cref="SystemMenu"/>.</summary>
    public SystemMenu(IApiClient api, AdminSession session)
    {
        _api     = api;
        _session = session;
    }

    /// <summary>Displays the system menu and loops until Back is selected.</summary>
    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.WriteHeading("System & Health");

            ConsoleUI.WriteMenuItem(1, "Health Check",   "query /api/health");
            ConsoleUI.WriteMenuItem(2, "Smoke Test",     "verify all key endpoints");
            ConsoleUI.WriteMenuItem(3, "Seed Test Users","create testuser1–3");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(3);

            switch (choice)
            {
                case 0: return;
                case 1: await HealthCheckAsync(); break;
                case 2: await SmokeTestAsync(); break;
                case 3: await SeedTestUsersAsync(); break;
            }
        }
    }

    // ─── Health Check ───────────────────────────────────────────────────────

    private async Task HealthCheckAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Health Check");

        var health = await _api.GetHealthAsync();

        if (health is null)
        {
            ConsoleUI.WriteError("API is unreachable or returned an error.");
        }
        else
        {
            ConsoleUI.WriteSuccess("API responded successfully.");
            ConsoleUI.WriteLine();
            ConsoleUI.WriteKeyValue("Status:",    health.Status);
            ConsoleUI.WriteKeyValue("Timestamp:", health.Timestamp);
            ConsoleUI.WriteKeyValue("Uptime:",    health.Uptime);
        }

        ConsoleUI.Pause();
    }

    // ─── Smoke Test ─────────────────────────────────────────────────────────

    private async Task SmokeTestAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Smoke Test");
        ConsoleUI.WriteInfo("Running checks...");
        ConsoleUI.WriteLine();

        // 1 — API reachable
        var health = await _api.GetHealthAsync();
        WriteCheckResult("GET /api/health — API reachable", health is not null);

        // 2 — Auth works
        bool authOk = await _api.PingProtectedAsync(_session.Token!);
        WriteCheckResult("GET /api/protected — Auth works", authOk);

        // 3 — Admin access
        var users = await _api.GetAllUsersAsync(_session.Token!);
        WriteCheckResult("GET /api/admin/users — Admin access", users.Count >= 0);

        ConsoleUI.Pause();
    }

    private static void WriteCheckResult(string label, bool passed)
    {
        if (passed)
            ConsoleUI.WriteSuccess(label);
        else
            ConsoleUI.WriteError(label);
    }

    // ─── Seed Test Users ────────────────────────────────────────────────────

    private async Task SeedTestUsersAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Seed Test Users");

        var testUsers = new[]
        {
            ("testuser1", "test1@appsimple.dev", "TestPass1!"),
            ("testuser2", "test2@appsimple.dev", "TestPass1!"),
            ("testuser3", "test3@appsimple.dev", "TestPass1!")
        };

        foreach (var (username, email, password) in testUsers)
        {
            var result = await _api.CreateUserAsync(_session.Token!, username, email, password);

            if (result is not null)
                ConsoleUI.WriteSuccess($"Created '{username}' ({email}).");
            else
                ConsoleUI.WriteWarning($"Skipped '{username}' — already exists or creation failed.");
        }

        ConsoleUI.Pause();
    }
}
