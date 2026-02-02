using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Authentication
{
    /// <summary>
    /// Implementation of role-based redirection service for authenticated users
    /// </summary>
    public class RoleBasedRedirectionService : IRoleBasedRedirectionService
    {
        private readonly IUserQueryService _userQueryService;
        private readonly ILogger<RoleBasedRedirectionService> _logger;

        // Role priority mapping (higher number = higher priority)
        private readonly Dictionary<string, int> _rolePriority = new()
        {
            { "User", 1 },
            { "Admin", 2 },
            { "SuperAdmin", 3 }
        };

        // Default redirect URLs for each role
        private readonly Dictionary<string, string> _roleRedirects = new()
        {
            { "User", "/User/Dashboard" },
            { "Admin", "/Admin/Dashboard" },
            { "SuperAdmin", "/SuperAdmin/Dashboard" }
        };

        // Safe redirect URL patterns
        private readonly HashSet<string> _safeRedirectPatterns = new()
        {
            "/User/",
            "/Admin/",
            "/SuperAdmin/",
            "/Auth/",
            "/Home/"
        };

        public RoleBasedRedirectionService(
            IUserQueryService userQueryService,
            ILogger<RoleBasedRedirectionService> logger)
        {
            _userQueryService = userQueryService;
            _logger = logger;
        }

        public async Task<string> GetRedirectUrlForUserAsync(int userId)
        {
            try
            {
                // For now, we'll use a simplified approach since GetUserByIdAsync doesn't exist
                // In a real implementation, this would query the user's role from the database
                _logger.LogInformation("Getting redirect URL for user {UserId}", userId);

                // Default to User role redirect for now
                // This should be enhanced to actually query the user's role
                return await GetDefaultRedirectForRoleAsync("User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting redirect URL for user {UserId}", userId);
                return "/Auth/Login";
            }
        }

        public async Task<string> GetDefaultRedirectForRoleAsync(string role)
        {
            await Task.CompletedTask; // For consistency with async interface

            if (_roleRedirects.TryGetValue(role, out var redirectUrl))
            {
                return redirectUrl;
            }

            _logger.LogWarning("Unknown role {Role} for redirection, defaulting to User dashboard", role);
            return _roleRedirects["User"];
        }

        public bool ShouldBypassPublicArea(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // Authenticated users should always bypass public area and go to their dashboard
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return roles.Any(role => _rolePriority.ContainsKey(role));
        }

        public string GetHighestPriorityRole(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return string.Empty;
            }

            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var highestRole = userRoles
                .Where(role => _rolePriority.ContainsKey(role))
                .OrderByDescending(role => _rolePriority[role])
                .FirstOrDefault();

            return highestRole ?? "User";
        }

        public async Task<string> GetRedirectUrlForClaimsAsync(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return "/Auth/Login";
            }

            var highestRole = GetHighestPriorityRole(user);
            if (string.IsNullOrEmpty(highestRole))
            {
                _logger.LogWarning("No valid role found for authenticated user, defaulting to User dashboard");
                return _roleRedirects["User"];
            }

            return await GetDefaultRedirectForRoleAsync(highestRole);
        }

        public bool IsValidRedirectUrl(string url, ClaimsPrincipal user)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            // Prevent open redirect attacks
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                return false;
            }

            // Ensure URL starts with /
            if (!url.StartsWith("/"))
            {
                return false;
            }

            // Check if URL matches safe patterns
            var isSafePattern = _safeRedirectPatterns.Any(pattern => url.StartsWith(pattern, StringComparison.OrdinalIgnoreCase));
            if (!isSafePattern)
            {
                return false;
            }

            // Additional role-based validation
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userRole = GetHighestPriorityRole(user);

                // Users can only access User areas
                if (userRole == "User" && (url.StartsWith("/Admin/") || url.StartsWith("/SuperAdmin/")))
                {
                    return false;
                }

                // Admins can access User and Admin areas but not SuperAdmin
                if (userRole == "Admin" && url.StartsWith("/SuperAdmin/"))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the highest priority role from a user's role string
        /// (Handles cases where user.Role might contain multiple roles)
        /// </summary>
        private string GetHighestPriorityRoleForUser(string userRole)
        {
            if (string.IsNullOrWhiteSpace(userRole))
            {
                return "User";
            }

            // Handle comma-separated roles
            var roles = userRole.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(r => r.Trim())
                               .ToList();

            var highestRole = roles
                .Where(role => _rolePriority.ContainsKey(role))
                .OrderByDescending(role => _rolePriority[role])
                .FirstOrDefault();

            return highestRole ?? "User";
        }
    }
}