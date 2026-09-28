using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Reservation.Application.Common;

namespace Reservation.Api.Common;

/// <summary>
/// Last-resort handler for exceptions that escape the use cases:
/// <see cref="ConflictException"/> → 409 with its safe application message;
/// anything else → 500 with a fixed message. Stack traces, SQL, connection strings
/// and internal exception messages are never written to the response — they only go
/// to the standard ASP.NET Core logs.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private const string UnexpectedErrorMessage = "An unexpected error occurred.";

    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string message;

        if (exception is ConflictException conflict)
        {
            // Known unique-constraint race: controlled conflict with a safe message.
            _logger.LogWarning(
                conflict,
                "Conflict while handling {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            message = conflict.Message;
        }
        else
        {
            // Unexpected: full details only in the server log, fixed message to the client.
            _logger.LogError(
                exception,
                "Unhandled exception while handling {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            message = UnexpectedErrorMessage;
        }

        if (httpContext.Response.HasStarted)
            return true;

        await httpContext.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}
