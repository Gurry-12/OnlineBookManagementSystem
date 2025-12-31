using Serilog;

namespace OnlineBookManagementSystem.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            await _next(context);
        }
        finally
        {
            var elapsed = DateTime.UtcNow - startTime;
            var statusCode = context.Response.StatusCode;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var userId = context.User?.Identity?.Name ?? "Anonymous";

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms for user {UserId} with {UserAgent}",
                method, path, statusCode, elapsed.TotalMilliseconds, userId, userAgent);
        }
    }
}