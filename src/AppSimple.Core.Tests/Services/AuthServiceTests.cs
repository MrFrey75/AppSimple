using AppSimple.Core.Auth;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;
using AppSimple.Core.Services.Impl;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AppSimple.Core.Tests.Services;

/// <summary>Unit tests for <see cref="AuthService"/>.</summary>
public sealed class AuthServiceTests
{
    private readonly IUserRepository _repo   = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwt   = Substitute.For<IJwtTokenService>();
    private readonly IAppLogger<AuthService> _log = Substitute.For<IAppLogger<AuthService>>();
    private readonly AuthService _svc;

    public AuthServiceTests()
    {
        _svc = new AuthService(_repo, _hasher, _jwt, _log);
    }

    private static User MakeUser(bool isActive = true) => new()
    {
        Uid          = Guid.CreateVersion7(),
        Username     = "alice",
        Email        = "alice@example.com",
        PasswordHash = "$2a$11$hash",
        IsActive     = isActive
    };

    // -------------------------------------------------------------------------
    // LoginAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WhenUserNotFound()
    {
        _repo.GetByUsernameAsync("nobody").ReturnsNull();

        var result = await _svc.LoginAsync("nobody", "pass");

        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WhenUserInactive()
    {
        var user = MakeUser(isActive: false);
        _repo.GetByUsernameAsync(user.Username).Returns(user);

        var result = await _svc.LoginAsync(user.Username, "pass");

        Assert.False(result.Succeeded);
        Assert.Contains("disabled", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WhenPasswordInvalid()
    {
        var user = MakeUser();
        _repo.GetByUsernameAsync(user.Username).Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await _svc.LoginAsync(user.Username, "wrongpass");

        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task LoginAsync_ReturnsSuccess_WithToken_WhenCredentialsValid()
    {
        var user = MakeUser();
        _repo.GetByUsernameAsync(user.Username).Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _jwt.GenerateToken(user).Returns("jwt.token.string");

        var result = await _svc.LoginAsync(user.Username, "CorrectPass1");

        Assert.True(result.Succeeded);
        Assert.Equal("jwt.token.string", result.Token);
    }

    [Fact]
    public async Task LoginAsync_Success_CallsGenerateToken()
    {
        var user = MakeUser();
        _repo.GetByUsernameAsync(user.Username).Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _jwt.GenerateToken(Arg.Any<User>()).Returns("tok");

        await _svc.LoginAsync(user.Username, "pass");

        _jwt.Received(1).GenerateToken(user);
    }

    [Fact]
    public async Task LoginAsync_Failure_DoesNotCallGenerateToken()
    {
        _repo.GetByUsernameAsync(Arg.Any<string>()).ReturnsNull();

        await _svc.LoginAsync("x", "y");

        _jwt.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    // -------------------------------------------------------------------------
    // ValidateToken
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidateToken_DelegatesToJwtTokenService()
    {
        _jwt.GetUsernameFromToken("tok123").Returns("alice");

        var username = _svc.ValidateToken("tok123");

        Assert.Equal("alice", username);
        _jwt.Received(1).GetUsernameFromToken("tok123");
    }

    [Fact]
    public void ValidateToken_ReturnsNull_WhenTokenInvalid()
    {
        _jwt.GetUsernameFromToken(Arg.Any<string>()).ReturnsNull();

        Assert.Null(_svc.ValidateToken("bad.token"));
    }
}
