using System.Net;
using System.Text.Json;

namespace OnlineBookManagementSystem.Presentation.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        // Check if request expects JSON (API calls)
        var acceptHeader = context.Request.Headers["Accept"].ToString();
        var isApiRequest = context.Request.Path.StartsWithSegments("/api") || 
                          acceptHeader.Contains("application/json");

        if (isApiRequest)
        {
            // Return JSON for API requests
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = new
                {
                    message = _environment.IsDevelopment() ? exception.Message : "An error occurred",
                    details = _environment.IsDevelopment() ? exception.StackTrace : null,
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                }
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
        else
        {
            // Return HTML error page for browser requests
            context.Response.ContentType = "text/html";
            
            var errorMessage = _environment.IsDevelopment() ? exception.Message : "An error occurred while processing your request.";
            var errorDetails = _environment.IsDevelopment() ? exception.StackTrace : "";
            
            var htmlResponse = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Error {statusCode}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .error-container {{ background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .error-title {{ color: #d32f2f; font-size: 24px; margin-bottom: 20px; }}
        .error-message {{ color: #333; margin-bottom: 20px; }}
        .error-details {{ background: #f8f8f8; padding: 15px; border-radius: 4px; font-family: monospace; font-size: 12px; white-space: pre-wrap; }}
        .back-link {{ color: #1976d2; text-decoration: none; }}
        .back-link:hover {{ text-decoration: underline; }}
    </style>
</head>
<body>
    <div class='error-container'>
        <h1 class='error-title'>Error {statusCode}</h1>
        <p class='error-message'>{errorMessage}</p>
        {(_environment.IsDevelopment() && !string.IsNullOrEmpty(errorDetails) ? $"<div class='error-details'>{errorDetails}</div>" : "")}
        <p><a href='javascript:history.back()' class='back-link'>← Go Back</a></p>
    </div>
</body>
</html>";

            await context.Response.WriteAsync(htmlResponse);
        }
    }
}
