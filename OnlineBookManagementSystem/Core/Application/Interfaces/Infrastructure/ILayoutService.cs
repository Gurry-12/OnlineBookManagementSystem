using System.Security.Claims;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Service interface for layout resolution and management
    /// </summary>
    public interface ILayoutService
    {
        /// <summary>
        /// Determines the appropriate layout based on user context
        /// </summary>
        /// <param name="user">The current user claims principal</param>
        /// <param name="controllerName">The current controller name</param>
        /// <param name="actionName">The current action name</param>
        /// <returns>The layout name to use</returns>
        string DetermineLayout(ClaimsPrincipal user, string? controllerName = null, string? actionName = null);

        /// <summary>
        /// Gets the full layout path for the given layout name
        /// </summary>
        /// <param name="layoutName">The layout name</param>
        /// <returns>The full layout path</returns>
        string GetLayoutPath(string layoutName);

        /// <summary>
        /// Checks if the user has permission to access admin layouts
        /// </summary>
        /// <param name="user">The current user claims principal</param>
        /// <returns>True if user can access admin layouts</returns>
        bool CanAccessAdminLayout(ClaimsPrincipal user);

        /// <summary>
        /// Checks if the user has permission to access super admin layouts
        /// </summary>
        /// <param name="user">The current user claims principal</param>
        /// <returns>True if user can access super admin layouts</returns>
        bool CanAccessSuperAdminLayout(ClaimsPrincipal user);

        /// <summary>
        /// Sets layout data in ViewData dictionary
        /// </summary>
        /// <param name="viewData">The ViewData dictionary</param>
        /// <param name="user">The current user</param>
        /// <param name="controllerName">The controller name</param>
        /// <param name="actionName">The action name</param>
        void SetLayoutData(IDictionary<string, object?> viewData, ClaimsPrincipal user, string? controllerName = null, string? actionName = null);

        /// <summary>
        /// Determines the appropriate theme class based on user context
        /// </summary>
        /// <param name="user">The current user claims principal</param>
        /// <param name="controllerName">The current controller name</param>
        /// <param name="actionName">The current action name</param>
        /// <returns>The theme class to apply</returns>
        string DetermineThemeClass(ClaimsPrincipal user, string? controllerName = null, string? actionName = null);
    }
}