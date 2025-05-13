using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string layout = "_LayoutAuth"; // Default layout for unauthenticated users

        // Get role from session
        var userRole = HttpContext.Session.GetString("userRole");

        if (!string.IsNullOrEmpty(userRole))
        {
            // Set layout based on user role
            layout = userRole switch
            {
                "Admin" => "_LayoutAdmin",  // Admin layout
                "User" => "_LayoutUser",    // User layout
                _ => "_AuthLayout"          // Fallback layout
            };
        }

        ViewData["Layout"] = layout; // Set the layout dynamically
        base.OnActionExecuting(context); // Continue with the action execution
    }

    public IActionResult SessionExpired()
    {
        HttpContext.Session.Clear();
        ViewData["Message"] = "Your session has expired. Please log in again.";
        return View("SessionExpired");
    }

}
