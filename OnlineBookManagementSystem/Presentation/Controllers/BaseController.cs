using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

public class BaseController : Controller
{
    private ILogger<BaseController>? _logger;
    protected ILogger<BaseController> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<BaseController>>()!;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewData["Layout"] = DetermineLayout();
        base.OnActionExecuting(context);
    }

    private string DetermineLayout()
    {
        try
        {
            var primaryRole = GetPrimaryRole();
            return primaryRole switch
            {
                "SuperAdmin" => "~/Presentation/Views/Shared/_LayoutSuperAdmin.cshtml",
                "Admin" => "~/Presentation/Views/Shared/_LayoutAdmin.cshtml",
                "User" => "~/Presentation/Views/Shared/_LayoutUser.cshtml",
                "Guest" => "~/Presentation/Views/Shared/_LayoutPublic.cshtml",
                _ => "~/Presentation/Views/Shared/_LayoutAuth.cshtml"
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error determining user layout");
            return "~/Presentation/Views/Shared/_LayoutAuth.cshtml";
        }
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

    public IActionResult SessionExpired()
    {
        HttpContext.Session?.Clear();
        ViewData["Message"] = "Your session has expired. Please log in again.";
        return View("SessionExpired");
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
