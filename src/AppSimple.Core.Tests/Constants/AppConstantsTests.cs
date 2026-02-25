using AppSimple.Core.Constants;

namespace AppSimple.Core.Tests.Constants;

/// <summary>Tests that <see cref="AppConstants"/> values remain stable and within expected ranges.</summary>
public sealed class AppConstantsTests
{
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
