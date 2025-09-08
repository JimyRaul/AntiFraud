using System.Diagnostics;

namespace Antifraud.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;

        _logger.LogInformation("Starting request {RequestId} {Method} {Path}",
            requestId, context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation("Completed request {RequestId} {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
                requestId, 
                context.Request.Method, 
                context.Request.Path, 
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}