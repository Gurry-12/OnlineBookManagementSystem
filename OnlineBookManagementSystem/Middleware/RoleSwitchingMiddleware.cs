using System.Security.Claims;

namespace OnlineBookManagementSystem.Middleware
{
    public class RoleSwitchingMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleSwitchingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only process for authenticated users
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                var originalRole = context.Session.GetString("OriginalRole");
                var currentViewRole = context.Session.GetString("CurrentViewRole");

                // Only SuperAdmins can use role switching
                if (userRole == "SuperAdmin")
                {
                    // If SuperAdmin is viewing as a different role, add a claim for the view role
                    if (!string.IsNullOrEmpty(currentViewRole) && currentViewRole != "SuperAdmin")
                    {
                        var identity = (ClaimsIdentity)context.User.Identity;
                        
                        // Add a custom claim to indicate the current view role
                        var existingViewRoleClaim = identity.FindFirst("ViewRole");
                        if (existingViewRoleClaim != null)
                        {
                            identity.RemoveClaim(existingViewRoleClaim);
                        }
                        identity.AddClaim(new Claim("ViewRole", currentViewRole));
                    }
                }
                else
                {
                    // Clear any role switching session data for non-SuperAdmins
                    context.Session.Remove("OriginalRole");
                    context.Session.Remove("CurrentViewRole");
                }
            }

            await _next(context);
        }
    }
}