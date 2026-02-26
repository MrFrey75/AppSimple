using AppSimple.Core.Interfaces;
using AppSimple.DataLib.Db;
using AppSimple.DataLib.Repositories;
using AppSimple.DataLib.Services;
using AppSimple.DataLib.Services.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace AppSimple.DataLib.Extensions;

/// <summary>
/// Extension methods for registering <c>AppSimple.DataLib</c> services with the DI container.
/// </summary>
public static class DataLibServiceExtensions
{
    /// <summary>
    /// Registers all data access services (repositories, DB connection factory, initializer) into the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="connectionString">The SQLite connection string (e.g., <c>Data Source=app.db</c>).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddDataLibServices(this IServiceCollection services, string connectionString)
    {
        DapperConfig.Register();

        services.Configure<DatabaseOptions>(opts => opts.ConnectionString = connectionString);
        services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
        services.AddSingleton<DbInitializer>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IDatabaseResetService, DatabaseResetService>();

        return services;
    }
}
