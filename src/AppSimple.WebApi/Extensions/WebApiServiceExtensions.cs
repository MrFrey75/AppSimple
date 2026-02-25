using AppSimple.Core.Auth;
using AppSimple.Core.Constants;
using AppSimple.Core.Extensions;
using AppSimple.Core.Logging;
using AppSimple.DataLib.Db;
using AppSimple.DataLib.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
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
        var connectionString = DatabasePath.Resolve(config[AppConstants.ConfigDatabaseConnectionString]);

        // Core + DataLib
        builder.Services.AddAppLogging(opts =>
        {
            opts.EnableFile   = config.GetValue(AppConstants.ConfigLoggingEnableFile, true);
            opts.LogDirectory = LogPath.Resolve(config[AppConstants.ConfigLoggingDirectory]);
        });

        // Wire Serilog into ASP.NET Core's ILogger<T> pipeline (e.g. ExceptionMiddleware)
        builder.Host.UseSerilog();
        builder.Services.AddCoreServices();
        builder.Services.AddJwtAuthentication(opts =>
        {
            opts.Secret            = config[AppConstants.ConfigJwtSecret]   ?? throw new InvalidOperationException("Jwt:Secret is required.");
            opts.Issuer            = config[AppConstants.ConfigJwtIssuer]   ?? AppConstants.AppName;
            opts.Audience          = config[AppConstants.ConfigJwtAudience] ?? AppConstants.AppName;
            opts.ExpirationMinutes = config.GetValue(AppConstants.ConfigJwtExpiration, AppConstants.DefaultJwtExpirationMinutes);
        });
        builder.Services.AddDataLibServices(connectionString);

        // ASP.NET Core authentication with JWT bearer
        var jwtSecret = config[AppConstants.ConfigJwtSecret] ?? throw new InvalidOperationException("Jwt:Secret is required.");
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
                    ValidIssuer              = config[AppConstants.ConfigJwtIssuer] ?? AppConstants.AppName,
                    ValidateAudience         = true,
                    ValidAudience            = config[AppConstants.ConfigJwtAudience] ?? AppConstants.AppName,
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
