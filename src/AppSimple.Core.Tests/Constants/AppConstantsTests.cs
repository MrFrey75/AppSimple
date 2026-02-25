using AppSimple.Core.Constants;

namespace AppSimple.Core.Tests.Constants;

/// <summary>Tests that <see cref="AppConstants"/> values remain stable and within expected ranges.</summary>
public sealed class AppConstantsTests
{
    // ── Identity ─────────────────────────────────────────────────────────────

    [Fact]
    public void AppName_IsAppSimple()
    {
        Assert.Equal("AppSimple", AppConstants.AppName);
    }

    [Fact]
    public void DefaultAdminUsername_IsAdmin()
    {
        Assert.Equal("admin", AppConstants.DefaultAdminUsername);
    }

    [Fact]
    public void DefaultAdminPassword_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppConstants.DefaultAdminPassword));
    }

    [Fact]
    public void DefaultAdminPassword_MeetsMinLength()
    {
        Assert.True(AppConstants.DefaultAdminPassword.Length >= AppConstants.MinPasswordLength);
    }

    [Fact]
    public void DefaultSamplePassword_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppConstants.DefaultSamplePassword));
    }

    [Fact]
    public void DefaultSamplePassword_DifferentFromAdminPassword()
    {
        Assert.NotEqual(AppConstants.DefaultAdminPassword, AppConstants.DefaultSamplePassword);
    }

    [Fact]
    public void DefaultSamplePassword_MeetsMinLength()
    {
        Assert.True(AppConstants.DefaultSamplePassword.Length >= AppConstants.MinPasswordLength);
    }

    // ── Networking ───────────────────────────────────────────────────────────

    [Fact]
    public void DefaultWebApiBaseUrl_IsValidAbsoluteUri()
    {
        var valid = Uri.TryCreate(AppConstants.DefaultWebApiBaseUrl, UriKind.Absolute, out _);
        Assert.True(valid, $"'{AppConstants.DefaultWebApiBaseUrl}' is not a valid absolute URI.");
    }

    // ── JWT ──────────────────────────────────────────────────────────────────

    [Fact]
    public void DefaultJwtExpirationMinutes_IsPositive()
    {
        Assert.True(AppConstants.DefaultJwtExpirationMinutes > 0);
    }

    [Fact]
    public void DefaultJwtExpirationMinutes_IsReasonable()
    {
        // Between 1 minute and 24 hours (1440 minutes)
        Assert.InRange(AppConstants.DefaultJwtExpirationMinutes, 1, 1440);
    }

    // ── Config keys ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(AppConstants.ConfigLoggingEnableFile)]
    [InlineData(AppConstants.ConfigLoggingDirectory)]
    [InlineData(AppConstants.ConfigDatabaseConnectionString)]
    [InlineData(AppConstants.ConfigWebApiBaseUrl)]
    [InlineData(AppConstants.ConfigJwtSecret)]
    [InlineData(AppConstants.ConfigJwtIssuer)]
    [InlineData(AppConstants.ConfigJwtAudience)]
    [InlineData(AppConstants.ConfigJwtExpiration)]
    public void ConfigKey_IsNotEmpty(string key)
    {
        Assert.False(string.IsNullOrWhiteSpace(key));
    }

    [Theory]
    [InlineData(AppConstants.ConfigLoggingEnableFile)]
    [InlineData(AppConstants.ConfigLoggingDirectory)]
    [InlineData(AppConstants.ConfigDatabaseConnectionString)]
    [InlineData(AppConstants.ConfigWebApiBaseUrl)]
    [InlineData(AppConstants.ConfigJwtSecret)]
    [InlineData(AppConstants.ConfigJwtIssuer)]
    [InlineData(AppConstants.ConfigJwtAudience)]
    [InlineData(AppConstants.ConfigJwtExpiration)]
    public void ConfigKey_ContainsColon_IndicatingSection(string key)
    {
        Assert.Contains(':', key);
    }

    // ── Validation limits ────────────────────────────────────────────────────

    [Fact]
    public void MinPasswordLength_IsAtLeast8()
    {
        Assert.True(AppConstants.MinPasswordLength >= 8,
            $"MinPasswordLength should be at least 8, was {AppConstants.MinPasswordLength}");
    }

    [Fact]
    public void MaxUsernameLength_IsPositive()
    {
        Assert.True(AppConstants.MaxUsernameLength > 0);
    }

    [Fact]
    public void MaxEmailLength_MeetsRfcMinimum()
    {
        // RFC 5321 allows up to 254 chars; we require at least 100
        Assert.True(AppConstants.MaxEmailLength >= 100,
            $"MaxEmailLength should allow reasonable emails, was {AppConstants.MaxEmailLength}");
    }

    [Fact]
    public void MaxNameLength_IsPositive()
    {
        Assert.True(AppConstants.MaxNameLength > 0);
    }

    [Fact]
    public void MaxPhoneLength_IsPositive()
    {
        Assert.True(AppConstants.MaxPhoneLength > 0);
    }

    [Fact]
    public void MaxBioLength_IsGreaterThan_MaxNameLength()
    {
        Assert.True(AppConstants.MaxBioLength > AppConstants.MaxNameLength,
            "Bio should allow more characters than a name");
    }

    [Fact]
    public void MaxUsernameLength_IsLessThan_MaxEmailLength()
    {
        Assert.True(AppConstants.MaxUsernameLength < AppConstants.MaxEmailLength,
            "Email should permit more characters than a username");
    }
}
