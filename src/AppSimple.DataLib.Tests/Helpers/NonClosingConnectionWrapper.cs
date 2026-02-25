using System.Data;

namespace AppSimple.DataLib.Tests.Helpers;

/// <summary>
/// Wraps an <see cref="IDbConnection"/> and suppresses <see cref="Dispose"/> and <see cref="Close"/>
/// so that an in-memory SQLite connection is not destroyed between repository calls.
/// </summary>
internal sealed class NonClosingConnectionWrapper : IDbConnection
{
    private readonly IDbConnection _inner;

    public NonClosingConnectionWrapper(IDbConnection inner) => _inner = inner;

    // Suppress close / dispose so the in-memory DB survives across repository calls
    public void Close() { }
    public void Dispose() { }

#pragma warning disable CS8767 // IDbConnection.ConnectionString nullability mismatch (interface annotation varies by runtime)
    public string ConnectionString
    {
        get => _inner.ConnectionString ?? string.Empty;
        set => _inner.ConnectionString = value;
    }
#pragma warning restore CS8767

    public int ConnectionTimeout => _inner.ConnectionTimeout;
    public string Database => _inner.Database;
    public ConnectionState State => _inner.State;

    public IDbTransaction BeginTransaction() => _inner.BeginTransaction();
    public IDbTransaction BeginTransaction(IsolationLevel il) => _inner.BeginTransaction(il);
    public void ChangeDatabase(string databaseName) => _inner.ChangeDatabase(databaseName);
    public IDbCommand CreateCommand() => _inner.CreateCommand();
    public void Open() => _inner.Open();
}
