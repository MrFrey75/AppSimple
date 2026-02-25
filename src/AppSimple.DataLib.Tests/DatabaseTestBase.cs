using AppSimple.DataLib.Db;
using AppSimple.DataLib.Tests.Helpers;

namespace AppSimple.DataLib.Tests;

/// <summary>
/// Base class for integration-style tests that need a freshly initialized in-memory SQLite database.
/// Derives from <see cref="IDisposable"/> so xUnit disposes the connection after each test class.
/// </summary>
public abstract class DatabaseTestBase : IDisposable
{
    /// <summary>Gets the in-memory connection factory shared by all tests in the class.</summary>
    protected InMemoryDbConnectionFactory ConnectionFactory { get; } = new();

    /// <summary>Gets the <see cref="DbInitializer"/> backed by the in-memory database.</summary>
    protected DbInitializer Initializer { get; }

    /// <summary>
    /// Initializes a new instance and runs schema creation so each test starts with a clean schema.
    /// </summary>
    protected DatabaseTestBase()
    {
        DapperConfig.Register();
        Initializer = new DbInitializer(ConnectionFactory);
        Initializer.Initialize();
    }

    /// <inheritdoc />
    public void Dispose() => ConnectionFactory.Dispose();
}
