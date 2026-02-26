namespace AppSimple.Core.Enums;

/// <summary>Classifies a postal address by its usage context.</summary>
public enum AddressType
{
    /// <summary>Home / residential address.</summary>
    Home = 0,

    /// <summary>Work or business address.</summary>
    Work = 1,

    /// <summary>Any other type.</summary>
    Other = 2,
}
