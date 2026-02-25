using AppSimple.AdminCli;
using AppSimple.AdminCli.Extensions;
using AppSimple.Core.Auth;
using AppSimple.Core.Constants;
using AppSimple.DataLib.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddAdminCliServices(config);

var provider = services.BuildServiceProvider();

// ── Database bootstrap ────────────────────────────────────────────────────────
var initializer = provider.GetRequiredService<DbInitializer>();
initializer.Initialize();
var hasher = provider.GetRequiredService<IPasswordHasher>();
initializer.SeedAdminUser(hasher.Hash(AppConstants.DefaultAdminPassword));

try
{
    var app = provider.GetRequiredService<App>();
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
