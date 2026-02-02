using OnlineBookManagementSystem.Presentation.Helpers;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Middleware
{
    /// <summary>
    /// Middleware to ensure proper layout resolution for all requests
    /// This acts as a fallback if controllers don't properly set layouts
    /// </summary>
    public class LayoutResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LayoutResolutionMiddleware> _logger;

        public LayoutResolutionMiddleware(RequestDelegate next, ILogger<LayoutResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Store original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                // Only process MVC requests that might need layout resolution
                if (context.Request.Path.StartsWithSegments("/api") || 
                    context.Request.Headers.Accept.Any(h => h?.Contains("application/json") == true))
                {
                    await _next(context);
                    return;
                }

                // Add layout information to HttpContext items for views to access
                var user = context.User;
                var routeData = context.GetRouteData();
                var controllerName = routeData?.Values["controller"]?.ToString();
                var actionName = routeData?.Values["action"]?.ToString();

                // Determine the appropriate layout
                var layout = LayoutHelper.DetermineLayout(user, controllerName, actionName);
                
                // Store layout information in HttpContext for views to access
                context.Items["ResolvedLayout"] = layout;
                context.Items["CanAccessAdmin"] = LayoutHelper.CanAccessAdminLayout(user);
                context.Items["CanAccessSuperAdmin"] = LayoutHelper.CanAccessSuperAdminLayout(user);
                context.Items["UserRoles"] = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                _logger.LogDebug("Layout middleware resolved layout {Layout} for {Controller}/{Action}", 
                    layout, controllerName, actionName);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LayoutResolutionMiddleware");
                await _next(context);
            }
        }
    }

    /// <summary>
    /// Extension method to register the layout resolution middleware
    /// </summary>
    public static class LayoutResolutionMiddlewareExtensions
    {
        public static IApplicationBuilder UseLayoutResolution(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LayoutResolutionMiddleware>();
        }
    }
}