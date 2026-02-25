using AppSimple.Core.Enums;

namespace AppSimple.Core.Tests.Enums;

/// <summary>Tests for <see cref="UserRole"/> enum values and membership.</summary>
public sealed class UserRoleTests
{
    [Fact]
    public void User_HasValue_Zero()
    {
        Assert.Equal(0, (int)UserRole.User);
    }

    [Fact]
    public void Admin_HasValue_One()
    {
        Assert.Equal(1, (int)UserRole.Admin);
    }

    [Fact]
    public void AllExpectedMembers_AreDefined()
    {
        var defined = Enum.GetNames<UserRole>();
        Assert.Contains("User", defined);
        Assert.Contains("Admin", defined);
    }

    [Fact]
    public void ExactlyTwoMembers_AreDefined()
    {
        Assert.Equal(2, Enum.GetValues<UserRole>().Length);
    }

    [Theory]
    [InlineData(0, UserRole.User)]
    [InlineData(1, UserRole.Admin)]
    public void IntValue_ParsesTo_CorrectMember(int value, UserRole expected)
    {
        Assert.Equal(expected, (UserRole)value);
    }
}
