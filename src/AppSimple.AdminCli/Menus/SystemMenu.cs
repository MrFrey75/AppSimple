using AppSimple.AdminCli.Services;
using AppSimple.AdminCli.Session;
using AppSimple.AdminCli.UI;
using AppSimple.Core.Logging;

namespace AppSimple.AdminCli.Menus;

/// <summary>
/// System and health sub-menu. Provides health checks, smoke tests,
/// and test user seeding.
/// </summary>
public sealed class SystemMenu
{
    private readonly IApiClient _api;
    private readonly AdminSession _session;
    private readonly IAppLogger<SystemMenu> _logger;

    /// <summary>Initializes a new instance of <see cref="SystemMenu"/>.</summary>
    public SystemMenu(IApiClient api, AdminSession session, IAppLogger<SystemMenu> logger)
    {
        _api     = api;
        _session = session;
        _logger  = logger;
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

        _logger.Debug("Admin '{Admin}' requested health check", _session.Username);
        var health = await _api.GetHealthAsync();

        if (health is null)
        {
            _logger.Warning("Health check failed — API unreachable");
            ConsoleUI.WriteError("API is unreachable or returned an error.");
        }
        else
        {
            _logger.Information("Health check OK — Status: {Status}, Uptime: {Uptime}", health.Status, health.Uptime);
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

        _logger.Information("Admin '{Admin}' started smoke test", _session.Username);

        // 1 — API reachable
        var health = await _api.GetHealthAsync();
        bool healthOk = health is not null;
        WriteCheckResult("GET /api/health — API reachable", healthOk);
        _logger.Information("Smoke test: /api/health — {Result}", healthOk ? "PASS" : "FAIL");

        // 2 — Auth works
        bool authOk = await _api.PingProtectedAsync(_session.Token!);
        WriteCheckResult("GET /api/protected — Auth works", authOk);
        _logger.Information("Smoke test: /api/protected — {Result}", authOk ? "PASS" : "FAIL");

        // 3 — Admin access
        var users = await _api.GetAllUsersAsync(_session.Token!);
        bool adminOk = users.Count >= 0;
        WriteCheckResult("GET /api/admin/users — Admin access", adminOk);
        _logger.Information("Smoke test: /api/admin/users — {Result}", adminOk ? "PASS" : "FAIL");

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
            {
                _logger.Information("Seeded test user '{Username}' ({Email})", username, email);
                ConsoleUI.WriteSuccess($"Created '{username}' ({email}).");
            }
            else
            {
                _logger.Warning("Seed skipped for '{Username}' — already exists or creation failed", username);
                ConsoleUI.WriteWarning($"Skipped '{username}' — already exists or creation failed.");
            }
        }

        ConsoleUI.Pause();
    }
}
