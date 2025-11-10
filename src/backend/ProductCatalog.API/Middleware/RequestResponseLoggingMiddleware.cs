using System.Diagnostics;

namespace ProductCatalog.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        // Log request
        _logger.LogInformation("Request {RequestId}: {Method} {Path} started",
            requestId, context.Request.Method, context.Request.Path);

        // Capture original response body stream
        var originalResponseBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Log response
            _logger.LogInformation("Request {RequestId}: {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                requestId, context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);

            // Reset the position to the beginning before copying
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalResponseBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {RequestId}: {Method} {Path} failed after {ElapsedMs}ms",
                requestId, context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalResponseBodyStream;
        }
    }
}