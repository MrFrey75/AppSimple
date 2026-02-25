namespace AppSimple.Core.Constants;

/// <summary>
/// Application-wide string constants.
/// </summary>
public static class AppConstants
{
    /// <summary>The name of the application.</summary>
    public const string AppName = "AppSimple";

    /// <summary>The default admin username used during database seeding.</summary>
    public const string DefaultAdminUsername = "admin";

    /// <summary>The default admin password used during initial seeding and database reset.</summary>
    public const string DefaultAdminPassword = "Admin123!";

    /// <summary>The minimum password length enforced by validation.</summary>
    public const int MinPasswordLength = 8;

    /// <summary>The maximum username length enforced by validation.</summary>
    public const int MaxUsernameLength = 50;

    /// <summary>The maximum email length enforced by validation.</summary>
    public const int MaxEmailLength = 256;

    /// <summary>The maximum length for first or last name enforced by validation.</summary>
    public const int MaxNameLength = 100;

    /// <summary>The maximum phone number length enforced by validation.</summary>
    public const int MaxPhoneLength = 30;

    /// <summary>The maximum bio length enforced by validation.</summary>
    public const int MaxBioLength = 500;
}
