using AppSimple.AdminCli;
using AppSimple.AdminCli.Extensions;
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

try
{
    var app = provider.GetRequiredService<App>();
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
