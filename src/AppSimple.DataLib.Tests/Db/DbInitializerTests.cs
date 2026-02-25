using AppSimple.DataLib.Tests.Helpers;
using Dapper;
using AppSimple.DataLib.Db;

namespace AppSimple.DataLib.Tests.Db;

/// <summary>
/// Tests for <see cref="DbInitializer"/>.
/// </summary>
public sealed class DbInitializerTests : IDisposable
{
    private readonly InMemoryDbConnectionFactory _factory = new();
    private readonly DbInitializer _initializer;

    public DbInitializerTests()
    {
        DapperConfig.Register();
        _initializer = new DbInitializer(_factory);
    }

    [Fact]
    public void Initialize_CreatesUsersTable()
    {
        _initializer.Initialize();

        using var conn = _factory.CreateConnection();
        var tables = conn.Query<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name='Users'");

        Assert.Contains("Users", tables);
    }

    [Fact]
    public void Initialize_IsIdempotent_DoesNotThrow()
    {
        _initializer.Initialize();

        // calling a second time should not throw
        var ex = Record.Exception(() => _initializer.Initialize());
        Assert.Null(ex);
    }

    [Fact]
    public void Initialize_UsersTable_HasExpectedColumns()
    {
        _initializer.Initialize();

        using var conn = _factory.CreateConnection();
        var columns = conn.Query<string>("SELECT name FROM pragma_table_info('Users')")
                          .ToHashSet();

        foreach (var expected in new[]
        {
            "Uid", "Username", "PasswordHash", "Email",
            "FirstName", "LastName", "PhoneNumber", "DateOfBirth",
            "Bio", "AvatarUrl", "Role", "IsActive", "IsSystem",
            "CreatedAt", "UpdatedAt"
        })
        {
            Assert.Contains(expected, columns);
        }
    }

    [Fact]
    public void SeedAdminUser_InsertsAdminRow()
    {
        _initializer.Initialize();
        _initializer.SeedAdminUser("$2a$11$hashedAdminPassword");

        using var conn = _factory.CreateConnection();
        var count = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Users WHERE Role = 1");

        Assert.Equal(1, count);
    }

    [Fact]
    public void SeedAdminUser_IsIdempotent_DoesNotDuplicateAdmin()
    {
        _initializer.Initialize();
        _initializer.SeedAdminUser("$2a$11$hashedAdminPassword");
        _initializer.SeedAdminUser("$2a$11$hashedAdminPassword");

        using var conn = _factory.CreateConnection();
        var count = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Users WHERE Role = 1");

        Assert.Equal(1, count);
    }

    [Fact]
    public void SeedAdminUser_AdminIsMarkedAsSystem()
    {
        _initializer.Initialize();
        _initializer.SeedAdminUser("$2a$11$hashedAdminPassword");

        using var conn = _factory.CreateConnection();
        var isSystem = conn.ExecuteScalar<int>("SELECT IsSystem FROM Users WHERE Role = 1");

        Assert.Equal(1, isSystem);
    }

    [Fact]
    public void SeedAdminUser_SetsExpectedUsername()
    {
        _initializer.Initialize();
        _initializer.SeedAdminUser("$2a$11$hashedAdminPassword");

        using var conn = _factory.CreateConnection();
        var username = conn.ExecuteScalar<string>("SELECT Username FROM Users WHERE Role = 1");

        Assert.Equal("admin", username);
    }

    public void Dispose() => _factory.Dispose();
}
