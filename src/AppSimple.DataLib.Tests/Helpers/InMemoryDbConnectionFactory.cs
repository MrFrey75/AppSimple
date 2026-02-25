using System.Data;
using AppSimple.DataLib.Db;
using Microsoft.Data.Sqlite;

namespace AppSimple.DataLib.Tests.Helpers;

/// <summary>
/// An in-memory SQLite connection factory used for isolated unit tests.
/// Each instance owns a single persistent in-memory connection so all
/// operations within a test share the same schema and data.
/// </summary>
public sealed class InMemoryDbConnectionFactory : IDbConnectionFactory, IDisposable
{
    private readonly SqliteConnection _connection;

    /// <summary>Initializes a new in-memory SQLite connection and opens it.</summary>
    public InMemoryDbConnectionFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    /// <inheritdoc />
    /// <remarks>Returns the same open connection for every call so the in-memory DB persists.</remarks>
    public IDbConnection CreateConnection() => new NonClosingConnectionWrapper(_connection);

    /// <inheritdoc />
    public void Dispose() => _connection.Dispose();
}
