using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Middleware;

/// <summary>
/// Turns exceptions thrown by services into ProblemDetails responses, so
/// controllers don't need try/catch blocks for every action.
/// </summary>
public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            NotSupportedException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                // The frontend shows `title` to the user. Don't leak internals on a 500.
                Title = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred"
                    : exception.Message
            }
        });
    }
}
