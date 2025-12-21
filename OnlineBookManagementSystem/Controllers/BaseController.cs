using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string layout = "_LayoutAuth"; // Default for unauthenticated/Guest

        // Prefer JWT claims over session for stateless auth
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(roleClaim))
        {
            // Multi-role support: Use highest privilege (e.g., SuperAdmin > Admin)
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var primaryRole = userRoles.Contains("SuperAdmin") ? "SuperAdmin" :
                              userRoles.Contains("Admin") ? "Admin" :
                              userRoles.Contains("User") ? "User" : "Guest";

            layout = primaryRole switch
            {
                "SuperAdmin" => "_LayoutSuperAdmin",
                "Admin" => "_LayoutAdmin",
                "User" => "_LayoutUser",
                "Guest" => "_LayoutPublic",  // Public/anonymous
                _ => "_LayoutAuth"  // Fallback
            };
        }

        ViewData["Layout"] = layout;
        base.OnActionExecuting(context);
    }

    public IActionResult SessionExpired()
    {
        HttpContext.Session?.Clear();  // Clear if using hybrid session/JWT
        ViewData["Message"] = "Your session has expired. Please log in again.";
        return View("SessionExpired");  // Uses dynamic layout
    }

    // Helper: Get current role (for views)
    protected string GetCurrentRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Guest";
}