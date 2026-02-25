using AppSimple.Core.Auth;
using AppSimple.Core.Logging;
using AppSimple.DataLib.Services.Impl;
using Dapper;
using NSubstitute;

namespace AppSimple.DataLib.Tests.Services;

/// <summary>Integration tests for <see cref="DatabaseResetService"/>.</summary>
public sealed class DatabaseResetServiceTests : DatabaseTestBase
{
    private readonly IPasswordHasher                  _hasher  = Substitute.For<IPasswordHasher>();
    private readonly IAppLogger<DatabaseResetService> _logger  = Substitute.For<IAppLogger<DatabaseResetService>>();
    private readonly DatabaseResetService             _service;

    public DatabaseResetServiceTests()
    {
        _hasher.Hash(Arg.Any<string>()).Returns(call => $"hashed:{call.Arg<string>()}");
        _service = new DatabaseResetService(ConnectionFactory, Initializer, _hasher, _logger);
    }

    [Fact]
    public async Task ResetAndReseedAsync_SeedsAdminUser()
    {
        await _service.ResetAndReseedAsync();

        using var conn = ConnectionFactory.CreateConnection();
        var adminCount = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Users WHERE Role = 1");
        Assert.Equal(1, adminCount);
    }

    [Fact]
    public async Task ResetAndReseedAsync_SeedsThreeSampleUsers()
    {
        await _service.ResetAndReseedAsync();

        using var conn  = ConnectionFactory.CreateConnection();
        var sampleCount = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Users WHERE Role = 0");
        Assert.Equal(3, sampleCount);
    }

    [Fact]
    public async Task ResetAndReseedAsync_TotalUserCountIsFour()
    {
        await _service.ResetAndReseedAsync();

        using var conn = ConnectionFactory.CreateConnection();
        var total      = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Users");
        Assert.Equal(4, total);
    }

    [Fact]
    public async Task ResetAndReseedAsync_SampleUsersHaveExpectedUsernames()
    {
        await _service.ResetAndReseedAsync();

        using var conn = ConnectionFactory.CreateConnection();
        var usernames  = conn.Query<string>("SELECT Username FROM Users WHERE Role = 0 ORDER BY Username")
                             .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("alice", usernames);
        Assert.Contains("bob",   usernames);
        Assert.Contains("carol", usernames);
    }

    [Fact]
    public async Task ResetAndReseedAsync_RemovesPreExistingUsers()
    {
        using (var conn = ConnectionFactory.CreateConnection())
        {
            var uid = Guid.CreateVersion7().ToString();
            var now = DateTime.UtcNow.ToString("O");
            conn.Execute("""
                INSERT INTO Users (Uid, Username, PasswordHash, Email, Role, IsActive, IsSystem, CreatedAt, UpdatedAt)
                VALUES (@Uid, 'ghost', 'hash', 'ghost@test.com', 0, 1, 0, @Now, @Now)
                """, new { Uid = uid, Now = now });
        }

        await _service.ResetAndReseedAsync();

        using var verifyConn = ConnectionFactory.CreateConnection();
        var ghostCount       = verifyConn.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM Users WHERE Username = 'ghost'");
        Assert.Equal(0, ghostCount);
    }

    [Fact]
    public async Task ResetAndReseedAsync_IsIdempotent_SecondCallProducesFourUsers()
    {
        await _service.ResetAndReseedAsync();
        await _service.ResetAndReseedAsync();

        using var conn = ConnectionFactory.CreateConnection();
        var total      = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Users");
        Assert.Equal(4, total);
    }

    [Fact]
    public async Task ResetAndReseedAsync_AdminIsMarkedAsSystem()
    {
        await _service.ResetAndReseedAsync();

        using var conn = ConnectionFactory.CreateConnection();
        var isSystem   = conn.ExecuteScalar<int>("SELECT IsSystem FROM Users WHERE Role = 1");
        Assert.Equal(1, isSystem);
    }

    [Fact]
    public async Task ResetAndReseedAsync_SampleUsersAreNotSystem()
    {
        await _service.ResetAndReseedAsync();

        using var conn       = ConnectionFactory.CreateConnection();
        var nonSystemSamples = conn.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM Users WHERE Role = 0 AND IsSystem = 0");
        Assert.Equal(3, nonSystemSamples);
    }

    [Fact]
    public async Task ResetAndReseedAsync_SampleUsersAreActive()
    {
        await _service.ResetAndReseedAsync();

        using var conn    = ConnectionFactory.CreateConnection();
        var activeSamples = conn.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM Users WHERE Role = 0 AND IsActive = 1");
        Assert.Equal(3, activeSamples);
    }
}
