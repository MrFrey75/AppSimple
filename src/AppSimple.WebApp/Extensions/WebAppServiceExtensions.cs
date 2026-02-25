using AppSimple.WebApp.Services;
using AppSimple.WebApp.Services.Impl;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

namespace AppSimple.WebApp.Extensions;

/// <summary>Extension methods for registering WebApp services.</summary>
public static class WebAppServiceExtensions
{
    /// <summary>Registers all services required by the WebApp.</summary>
    public static WebApplicationBuilder AddWebAppServices(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Host.UseSerilog((ctx, lc) =>
        {
            var logDir = LogPath.Resolve(config["AppLogging:LogDirectory"]);
            var enableFile = config.GetValue("AppLogging:EnableFile", true);
            lc.MinimumLevel.Information()
              .Enrich.FromLogContext();
            if (enableFile)
                lc.WriteTo.File(Path.Combine(logDir, "webapp-.log"), rollingInterval: Serilog.RollingInterval.Day);
            lc.WriteTo.Console();
        });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opts =>
            {
                opts.LoginPath = "/login";
                opts.LogoutPath = "/logout";
                opts.AccessDeniedPath = "/";
                opts.ExpireTimeSpan = TimeSpan.FromHours(8);
                opts.SlidingExpiration = true;
            });

        builder.Services.AddAuthorization();

        var baseUrl = config["WebApi:BaseUrl"] ?? "http://localhost:5157";
        builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpContextAccessor();

        return builder;
    }
}
