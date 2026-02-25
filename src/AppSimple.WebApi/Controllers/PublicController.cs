using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApi.Controllers;

/// <summary>
/// Public endpoints accessible without authentication.
/// </summary>
[ApiController]
[Route("api")]
public sealed class PublicController : ControllerBase
{
    /// <summary>Root ping / health indicator.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index() =>
        Ok(new { message = "AppSimple API is running." });

    /// <summary>Public endpoint — no authentication required.</summary>
    [HttpGet("public")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Public() =>
        Ok(new { message = "This is a public endpoint. No authentication required." });

    /// <summary>Health check — returns API uptime and UTC timestamp.</summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health() =>
        Ok(new
        {
            status    = "healthy",
            timestamp = DateTime.UtcNow,
            uptime    = Environment.TickCount64 / 1000,
        });
}
