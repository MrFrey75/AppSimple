namespace AppSimple.AdminCli.Services;

/// <summary>Result returned by the WebApi health endpoint.</summary>
public sealed class HealthResult
{
    /// <summary>Gets or sets the status string (e.g., "Healthy").</summary>
    public string Status { get; set; } = "";

    /// <summary>Gets or sets the server timestamp string.</summary>
    public string Timestamp { get; set; } = "";

    /// <summary>Gets or sets the server uptime string.</summary>
    public string Uptime { get; set; } = "";
}
