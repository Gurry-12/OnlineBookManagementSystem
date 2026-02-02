using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;
using OnlineBookManagementSystem.Presentation.Helpers;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure
{
    /// <summary>
    /// Service for layout resolution and management
    /// </summary>
    public class LayoutService : ILayoutService
    {
        private readonly ILogger<LayoutService> _logger;

        public LayoutService(ILogger<LayoutService> logger)
        {
            _logger = logger;
        }

        public string DetermineLayout(ClaimsPrincipal user, string? controllerName = null, string? actionName = null)
        {
            try
            {
                var layout = LayoutHelper.DetermineLayout(user, controllerName, actionName);
                
                _logger.LogDebug("Determined layout {Layout} for controller {Controller}, action {Action}, user authenticated: {IsAuthenticated}", 
                    layout, controllerName, actionName, user.Identity?.IsAuthenticated);
                
                return layout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining layout for controller {Controller}, action {Action}", controllerName, actionName);
                return LayoutHelper.LayoutPublic; // Safe fallback
            }
        }

        public string GetLayoutPath(string layoutName)
        {
            return LayoutHelper.GetLayoutPath(layoutName);
        }

        public bool CanAccessAdminLayout(ClaimsPrincipal user)
        {
            return LayoutHelper.CanAccessAdminLayout(user);
        }

        public bool CanAccessSuperAdminLayout(ClaimsPrincipal user)
        {
            return LayoutHelper.CanAccessSuperAdminLayout(user);
        }

        public void SetLayoutData(IDictionary<string, object?> viewData, ClaimsPrincipal user, string? controllerName = null, string? actionName = null)
        {
            try
            {
                var layout = DetermineLayout(user, controllerName, actionName);
                var themeClass = DetermineThemeClass(user, controllerName, actionName);
                
                viewData["Layout"] = layout;
                viewData["ThemeClass"] = themeClass;
                viewData["IsAuthenticated"] = user.Identity?.IsAuthenticated ?? false;
                viewData["UserRoles"] = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                viewData["CanAccessAdmin"] = CanAccessAdminLayout(user);
                viewData["CanAccessSuperAdmin"] = CanAccessSuperAdminLayout(user);
                viewData["CurrentController"] = controllerName;
                viewData["CurrentAction"] = actionName;
                
                _logger.LogDebug("Set layout data: Layout={Layout}, Theme={Theme}, Controller={Controller}, Action={Action}", 
                    layout, themeClass, controllerName, actionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting layout data");
                viewData["Layout"] = LayoutHelper.LayoutPublic; // Safe fallback
                viewData["ThemeClass"] = "theme-public"; // Safe fallback
            }
        }

        public string DetermineThemeClass(ClaimsPrincipal user, string? controllerName = null, string? actionName = null)
        {
            try
            {
                // Theme selection based on user role and context
                if (user.IsInRole("SuperAdmin"))
                {
                    return "theme-superadmin";
                }
                
                if (user.IsInRole("Admin"))
                {
                    return "theme-admin";
                }
                
                if (user.Identity?.IsAuthenticated == true)
                {
                    return "theme-user";
                }
                
                return "theme-public";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining theme class");
                return "theme-public"; // Safe fallback
            }
        }
    }
}