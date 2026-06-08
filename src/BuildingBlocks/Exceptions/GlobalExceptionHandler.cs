using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            // DomainException de cualquier servicio (usan nombres distintos pero el patrón es el mismo)
            _ when exception.GetType().Name == "DomainException"
                => (HttpStatusCode.BadRequest, exception.Message),

            ValidationException validationEx
                => (HttpStatusCode.BadRequest, string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),

            KeyNotFoundException
                => (HttpStatusCode.NotFound, exception.Message),

            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor.")
        };

        logger.LogError(exception, "Error {StatusCode}: {Message}", (int)statusCode, message);

        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}
