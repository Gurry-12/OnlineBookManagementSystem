using System.Security.Claims;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication
{
    /// <summary>
    /// Service for handling role-based user redirection after authentication
    /// </summary>
    public interface IRoleBasedRedirectionService
    {
        /// <summary>
        /// Gets the appropriate redirect URL for a specific user based on their roles
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>Redirect URL for the user's highest priority role</returns>
        Task<string> GetRedirectUrlForUserAsync(int userId);

        /// <summary>
        /// Gets the default redirect URL for a specific role
        /// </summary>
        /// <param name="role">Role name (SuperAdmin, Admin, User)</param>
        /// <returns>Default redirect URL for the role</returns>
        Task<string> GetDefaultRedirectForRoleAsync(string role);

        /// <summary>
        /// Determines if an authenticated user should bypass the public showcase area
        /// </summary>
        /// <param name="user">Claims principal representing the authenticated user</param>
        /// <returns>True if user should be redirected to their dashboard, false if they can view public content</returns>
        bool ShouldBypassPublicArea(ClaimsPrincipal user);

        /// <summary>
        /// Gets the highest priority role for a user (SuperAdmin > Admin > User)
        /// </summary>
        /// <param name="user">Claims principal representing the authenticated user</param>
        /// <returns>Highest priority role name</returns>
        string GetHighestPriorityRole(ClaimsPrincipal user);

        /// <summary>
        /// Gets the redirect URL based on claims principal
        /// </summary>
        /// <param name="user">Claims principal representing the authenticated user</param>
        /// <returns>Appropriate redirect URL</returns>
        Task<string> GetRedirectUrlForClaimsAsync(ClaimsPrincipal user);

        /// <summary>
        /// Validates that a redirect URL is safe and authorized
        /// </summary>
        /// <param name="url">URL to validate</param>
        /// <param name="user">User requesting the redirect</param>
        /// <returns>True if URL is safe and authorized</returns>
        bool IsValidRedirectUrl(string url, ClaimsPrincipal user);
    }
}