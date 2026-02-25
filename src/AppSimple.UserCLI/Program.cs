using AppSimple.Core.Auth;
using AppSimple.Core.Constants;
using AppSimple.Core.Extensions;
using AppSimple.Core.Logging;
using AppSimple.DataLib.Db;
using AppSimple.DataLib.Extensions;
using AppSimple.UserCLI;
using AppSimple.UserCLI.Menus;
using AppSimple.UserCLI.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

// ── Configuration ─────────────────────────────────────────────────────────────
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

// ── DI container ──────────────────────────────────────────────────────────────
var services = new ServiceCollection();

// Logging — file-only so log output doesn't interfere with the console UI
services.AddAppLogging(opts =>
{
    opts.ApplicationName = "AppSimple.UserCLI";
    opts.EnableConsole   = false;
    opts.EnableFile      = configuration.GetValue(AppConstants.ConfigLoggingEnableFile, true);
    opts.LogDirectory    = LogPath.Resolve(configuration.GetValue(AppConstants.ConfigLoggingDirectory, ""))!;
});

// Core services — validators, password hasher, user service, auth service
services.AddCoreServices();

// JWT — used by AuthService to issue tokens
services.AddJwtAuthentication(opts =>
    configuration.GetSection("Jwt").Bind(opts));

// Data access — SQLite + Dapper repositories
services.AddDataLibServices(
    DatabasePath.Resolve(configuration[AppConstants.ConfigDatabaseConnectionString]));

// CLI-specific services
services.AddSingleton<UserSession>();
services.AddTransient<ProfileMenu>();
services.AddTransient<AdminMenu>();
services.AddTransient<MainMenu>();
services.AddTransient<LoginMenu>();
services.AddTransient<App>();

var provider = services.BuildServiceProvider();

// ── Database bootstrap ────────────────────────────────────────────────────────
var initializer = provider.GetRequiredService<DbInitializer>();
initializer.Initialize();

// Seed default admin on first run (idempotent — skipped if admin already exists)
var hasher = provider.GetRequiredService<IPasswordHasher>();
initializer.SeedAdminUser(hasher.Hash(AppConstants.DefaultAdminPassword));

// ── Run ───────────────────────────────────────────────────────────────────────
var app = provider.GetRequiredService<App>();
await app.RunAsync();

Log.CloseAndFlush();
