namespace AppSimple.Core.Enums;

/// <summary>Classifies a phone number by its usage context.</summary>
public enum PhoneType
{
    /// <summary>Mobile / cell phone.</summary>
    Mobile = 0,

    /// <summary>Home landline.</summary>
    Home = 1,

    /// <summary>Work or business phone.</summary>
    Work = 2,

    /// <summary>Any other type.</summary>
    Other = 3,
}
