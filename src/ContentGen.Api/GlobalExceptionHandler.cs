using ContentGen.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ContentGen.Api;

/// <summary>
/// Translates unhandled exceptions into RFC 7807 ProblemDetails responses and logs them.
/// Known application/infrastructure failures map to meaningful status codes; everything else
/// becomes a 500 with a generic detail so internals are never leaked to the client.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Invalid request"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            AiResponseParseException => (StatusCodes.Status502BadGateway, "Invalid response from AI provider"),
            HttpRequestException => (StatusCodes.Status502BadGateway, "Upstream request failed"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("{ExceptionType} processing {Method} {Path}: {Message}", exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        // Don't expose internal details for server-side faults.
        var detail = status >= StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred while processing your request."
            : exception.Message;

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }
}
