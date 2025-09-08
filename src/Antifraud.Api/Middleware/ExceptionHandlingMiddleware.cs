using System.Net;
using System.Text.Json;
using Antifraud.Domain.Exceptions;

namespace Antifraud.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var problemDetails = exception switch
        {
            TransactionNotFoundException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Transaction Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            AccountNotFoundException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Account Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            DomainException domainEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Domain Error",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = domainEx.Message,
                Instance = context.Request.Path
            },
            ArgumentException argEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Invalid Argument",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = argEx.Message,
                Instance = context.Request.Path
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "Access denied",
                Instance = context.Request.Path
            },
            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An unexpected error occurred. Please try again later.",
                Instance = context.Request.Path
            }
        };

        response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await response.WriteAsync(jsonResponse);
    }

    private class ProblemDetails
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int? Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
    }
}