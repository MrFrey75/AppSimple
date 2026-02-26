namespace AppSimple.Core.Enums;

/// <summary>Classifies an email address by its usage context.</summary>
public enum EmailType
{
    /// <summary>Personal / home email.</summary>
    Personal = 0,

    /// <summary>Work or business email.</summary>
    Work = 1,

    /// <summary>Any other type.</summary>
    Other = 2,
}
