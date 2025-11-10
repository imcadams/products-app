using System.Net;
using System.Text.Json;
using ProductCatalog.Core.Exceptions;

namespace ProductCatalog.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new
        {
            error = new
            {
                message = exception.Message,
                type = exception.GetType().Name,
                timestamp = DateTime.UtcNow
            }
        };

        var statusCode = exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        // Don't expose internal server error details in production
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            response = new
            {
                error = new
                {
                    message = "An internal server error occurred",
                    type = "InternalServerError",
                    timestamp = DateTime.UtcNow
                }
            };
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(jsonResponse);
    }
}