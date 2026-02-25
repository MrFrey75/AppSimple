using System.Net;
using System.Text.Json;
using AppSimple.Core.Common.Exceptions;

namespace AppSimple.WebApi.Middleware;

/// <summary>
/// Global exception-handling middleware. Maps domain exceptions to appropriate
/// HTTP status codes and returns a consistent JSON error envelope.
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>Initializes a new instance of <see cref="ExceptionMiddleware"/>.</summary>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
        _logger.LogInformation("ExceptionMiddleware initialized.");
    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        (HttpStatusCode status, string message) = ex switch
        {
            EntityNotFoundException e   => (HttpStatusCode.NotFound,             e.Message),
            DuplicateEntityException e  => (HttpStatusCode.Conflict,             e.Message),
            UnauthorizedException e     => (HttpStatusCode.Unauthorized,         e.Message),
            SystemEntityException e     => (HttpStatusCode.Forbidden,            e.Message),
            _                           => (HttpStatusCode.InternalServerError,  "An unexpected error occurred.")
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception");

        context.Response.StatusCode  = (int)status;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { error = message });
        _logger.LogDebug("Returning error response: {StatusCode} - {Message}", context.Response.StatusCode, message);
        await context.Response.WriteAsync(body);
    }
}
