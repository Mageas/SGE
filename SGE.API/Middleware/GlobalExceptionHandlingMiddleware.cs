using System.Text.Json;
using SGE.Core.Exceptions;
using SGE.Core.Models;

namespace SGE.API.Middleware;

/// <summary>
///     Middleware for handling unhandled exceptions globally in the request processing pipeline.
///     Captures exceptions, logs the details, and formats a standardized error response for clients.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        _logger.LogError(exception,
            "Exception non gérée capturée. TraceId: {TraceId}", traceId);

        var errorResponse =
            exception switch
            {
                ValidationException validationException => ErrorResponse.CreateValidation(
                    validationException.Errors,
                    traceId),

                SgeException sgeException => ErrorResponse.Create(
                    sgeException.Message,
                    sgeException.ErrorCode,
                    sgeException.StatusCode,
                    traceId),

                ArgumentNullException => ErrorResponse.Create(
                    "Un paramètre requis est manquant.",
                    "ARGUMENT_NULL",
                    400,
                    traceId),

                ArgumentException => ErrorResponse.Create(
                    "Un paramètre fourni est invalide.",
                    "INVALID_ARGUMENT",
                    400,
                    traceId),

                UnauthorizedAccessException => ErrorResponse.Create(
                    "Accès non autorisé.",
                    "UNAUTHORIZED",
                    401,
                    traceId),

                NotImplementedException => ErrorResponse.Create(
                    "Fonctionnalité non implémentée.",
                    "NOT_IMPLEMENTED",
                    501,
                    traceId),

                TimeoutException => ErrorResponse.Create(
                    "L'opération a expiré.",
                    "TIMEOUT",
                    408,
                    traceId),

                // Gestion par défaut pour toutes les autres erreurs non gérées (500)
                _ => ErrorResponse.Create(
                    "Une erreur interne du serveur est survenue.",
                    "INTERNAL_SERVER_ERROR",
                    500,
                    traceId)
            };

        context.Response.StatusCode = errorResponse.StatusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}