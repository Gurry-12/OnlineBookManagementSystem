using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Helpers
{
    /// <summary>
    /// Helper class to determine the appropriate layout based on user role and context
    /// </summary>
    public static class LayoutHelper
    {
        public const string LayoutPublic = "_LayoutPublic";
        public const string LayoutAuth = "_LayoutAuth";
        public const string LayoutUser = "_LayoutUser";
        public const string LayoutAdmin = "_LayoutAdmin";
        public const string LayoutSuperAdmin = "_LayoutSuperAdmin";

        /// <summary>
        /// Determines the appropriate layout based on user authentication and role
        /// </summary>
        /// <param name="user">The current user claims principal</param>
        /// <param name="controllerName">The current controller name</param>
        /// <param name="actionName">The current action name</param>
        /// <returns>The layout name to use</returns>
        public static string DetermineLayout(ClaimsPrincipal user, string? controllerName = null, string? actionName = null)
        {
            // Handle authentication-related pages
            if (controllerName?.Equals("Auth", StringComparison.OrdinalIgnoreCase) == true)
            {
                return LayoutAuth;
            }

            // Handle authenticated users
            if (user.Identity?.IsAuthenticated == true)
            {
                var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                // SuperAdmin has highest priority
                if (roles.Contains("SuperAdmin"))
                {
                    return LayoutSuperAdmin;
                }

                // Admin layout
                if (roles.Contains("Admin"))
                {
                    return LayoutAdmin;
                }

                // Regular user layout
                if (roles.Contains("User"))
                {
                    return LayoutUser;
                }
            }

            // Default to public layout for unauthenticated users or unknown roles
            return LayoutPublic;
        }

        /// <summary>
        /// Gets the full layout path for the given layout name
        /// </summary>
        /// <param name="layoutName">The layout name</param>
        /// <returns>The full layout path</returns>
        public static string GetLayoutPath(string layoutName)
        {
            return $"~/Presentation/Views/Shared/{layoutName}.cshtml";
        }

        /// <summary>
        /// Determines and returns the full layout path
        /// </summary>
        /// <param name="user">The current user claims principal</param>
        /// <param name="controllerName">The current controller name</param>
        /// <param name="actionName">The current action name</param>
        /// <returns>The full layout path</returns>
        public static string DetermineLayoutPath(ClaimsPrincipal user, string? controllerName = null, string? actionName = null)
        {
            var layoutName = DetermineLayout(user, controllerName, actionName);
            return GetLayoutPath(layoutName);
        }

        /// <summary>
        /// Checks if the user has permission to access admin layouts
        /// </summary>
        /// <param name="user">The current user claims principal</param>
        /// <returns>True if user can access admin layouts</returns>
        public static bool CanAccessAdminLayout(ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated != true) return false;
            
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return roles.Contains("Admin") || roles.Contains("SuperAdmin");
        }

        /// <summary>
        /// Checks if the user has permission to access super admin layouts
        /// </summary>
        /// <param name="user">The current user claims principal</param>
        /// <returns>True if user can access super admin layouts</returns>
        public static bool CanAccessSuperAdminLayout(ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated != true) return false;
            
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return roles.Contains("SuperAdmin");
        }
    }
}