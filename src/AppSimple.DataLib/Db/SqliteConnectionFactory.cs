using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace AppSimple.DataLib.Db;

/// <summary>
/// Creates SQLite database connections using the configured connection string.
/// </summary>
public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of <see cref="SqliteConnectionFactory"/>.
    /// </summary>
    /// <param name="options">The database options containing the connection string.</param>
    public SqliteConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
