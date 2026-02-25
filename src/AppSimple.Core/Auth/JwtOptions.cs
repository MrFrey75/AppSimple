using System.ComponentModel.DataAnnotations;

namespace AppSimple.Core.Auth;

/// <summary>
/// Configuration options for JWT token generation and validation.
/// Bind from <c>appsettings.json</c> under the <c>"Jwt"</c> section.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Gets or sets the HMAC-SHA256 signing secret.
    /// Must be at least 32 characters. Keep this value secret.
    /// </summary>
    [Required, MinLength(32)]
    public required string Secret { get; set; }

    /// <summary>Gets or sets the token issuer claim (<c>iss</c>). Defaults to <c>"AppSimple"</c>.</summary>
    public string Issuer { get; set; } = "AppSimple";

    /// <summary>Gets or sets the token audience claim (<c>aud</c>). Defaults to <c>"AppSimple"</c>.</summary>
    public string Audience { get; set; } = "AppSimple";

    /// <summary>
    /// Gets or sets the token lifetime in minutes. Defaults to <c>60</c>.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}
