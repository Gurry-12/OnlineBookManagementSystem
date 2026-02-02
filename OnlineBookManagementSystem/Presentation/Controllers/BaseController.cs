using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

public class BaseController : Controller
{
    private ILogger<BaseController>? _logger;

    protected ILogger<BaseController> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<BaseController>>()!;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        try
        {
            var controllerName = ControllerContext.ActionDescriptor.ControllerName;
            var actionName = ControllerContext.ActionDescriptor.ActionName;
            
            // Simplified: Set basic view data without layout service
            ViewData["IsAuthenticated"] = User.Identity?.IsAuthenticated ?? false;
            ViewData["UserRoles"] = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            ViewData["CurrentController"] = controllerName;
            ViewData["CurrentAction"] = actionName;
            ViewData["ThemeClass"] = "theme-public"; // Default theme
            
            Logger.LogDebug("Basic view data set for {Controller}/{Action}", controllerName, actionName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnActionExecuting");
        }
        
        base.OnActionExecuting(context);
    }

    private string GetPrimaryRole()
    {
        var userRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return userRoles.Contains("SuperAdmin") ? "SuperAdmin" :
               userRoles.Contains("Admin") ? "Admin" :
               userRoles.Contains("User") ? "User" : "Guest";
    }

    protected List<string> GetUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    protected bool HasRole(string role)
    {
        return User.IsInRole(role);
    }

    protected bool CanAccessAdmin()
    {
        return User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
    }

    protected bool CanAccessSuperAdmin()
    {
        return User.IsInRole("SuperAdmin");
    }

    public IActionResult SessionExpired()
    {
        HttpContext.Session?.Clear();
        ViewData["Message"] = "Your session has expired. Please log in again.";
        return View("~/Presentation/Views/Shared/SessionExpired.cshtml");
    }

    protected string GetCurrentRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Guest";

    protected int GetUserIdFromClaims()
    {
        if (User.Identity?.IsAuthenticated != true) return 0;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            Logger.LogWarning("Authenticated user missing NameIdentifier claim");
            return 0;
        }

        if (int.TryParse(userIdClaim, out var userId) && userId > 0)
            return userId;

        Logger.LogWarning("Invalid user ID claim: {UserIdClaim}", userIdClaim);
        return 0;
    }

    protected bool IsUserAuthorized(int expectedUserId = 0)
    {
        var currentUserId = GetUserIdFromClaims();
        if (currentUserId == 0) return false;

        return expectedUserId == 0 || currentUserId == expectedUserId;
    }
}
