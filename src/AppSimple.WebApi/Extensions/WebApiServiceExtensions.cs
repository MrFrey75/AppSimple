using AppSimple.Core.Auth;
using AppSimple.Core.Extensions;
using AppSimple.Core.Logging;
using AppSimple.DataLib.Db;
using AppSimple.DataLib.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AppSimple.WebApi.Extensions;

/// <summary>
/// Registers all AppSimple.WebApi services with the DI container.
/// </summary>
public static class WebApiServiceExtensions
{
    /// <summary>
    /// Configures Core, DataLib, JWT authentication, Swagger, and logging services.
    /// </summary>
    public static WebApplicationBuilder AddWebApiServices(this WebApplicationBuilder builder)
    {
        var config           = builder.Configuration;
        var connectionString = DatabasePath.Resolve(config["Database:ConnectionString"]);

        // Core + DataLib
        builder.Services.AddAppLogging(opts =>
        {
            opts.EnableFile   = config.GetValue("AppLogging:EnableFile", true);
            opts.LogDirectory = LogPath.Resolve(config["AppLogging:LogDirectory"]);
        });
        builder.Services.AddCoreServices();
        builder.Services.AddJwtAuthentication(opts =>
        {
            opts.Secret            = config["Jwt:Secret"]   ?? throw new InvalidOperationException("Jwt:Secret is required.");
            opts.Issuer            = config["Jwt:Issuer"]   ?? "AppSimple";
            opts.Audience          = config["Jwt:Audience"] ?? "AppSimple";
            opts.ExpirationMinutes = config.GetValue("Jwt:ExpirationMinutes", 480);
        });
        builder.Services.AddDataLibServices(connectionString);

        // ASP.NET Core authentication with JWT bearer
        var jwtSecret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is required.");
        var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,
                    ValidateIssuer           = true,
                    ValidIssuer              = config["Jwt:Issuer"] ?? "AppSimple",
                    ValidateAudience         = true,
                    ValidAudience            = config["Jwt:Audience"] ?? "AppSimple",
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero,
                };
            });

        builder.Services.AddAuthorization(opts =>
        {
            opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
        });

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        return builder;
    }
}
