namespace Antifraud.Api.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrGenerateCorrelationId(context);
        
        // Agregar a la respuesta
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
        
        // Agregar al contexto para logging
        using var scope = context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>()
            .BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } });

        await _next(context);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}