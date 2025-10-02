using System.Net;
using System.Text.Json;
using Domain.Exceptions;

namespace Infrastructure.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred. RequestId: {RequestId}", 
                context.TraceIdentifier);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse();

        switch (exception)
        {
            // Excepciones personalizadas del dominio - PRIORIDAD ALTA
            case BaseException baseEx:
                response.StatusCode = baseEx.StatusCode;
                response.Message = baseEx.Message;
                response.Details = baseEx.Details;
                response.ErrorCode = baseEx.ErrorCode;
                break;
                
            // Excepciones del sistema - PRIORIDAD BAJA
            case ArgumentNullException:
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Invalid request parameters";
                response.Details = exception.Message;
                response.ErrorCode = "INVALID_ARGUMENT";
                break;
                
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized access";
                response.ErrorCode = "UNAUTHORIZED_ACCESS";
                break;
                
            case KeyNotFoundException:
            case InvalidOperationException when exception.Message.Contains("not found"):
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "Resource not found";
                response.Details = exception.Message;
                response.ErrorCode = "RESOURCE_NOT_FOUND";
                break;
                
            case TimeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Message = "Request timeout";
                response.ErrorCode = "REQUEST_TIMEOUT";
                break;
                
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An internal server error occurred";
                response.Details = "Please try again later or contact support if the problem persists";
                response.ErrorCode = "INTERNAL_SERVER_ERROR";
                break;
        }

        context.Response.StatusCode = response.StatusCode;
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
}
