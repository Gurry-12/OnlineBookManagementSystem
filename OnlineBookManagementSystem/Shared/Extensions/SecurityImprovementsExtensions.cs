using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Shared.Extensions
{
    /// <summary>
    /// Security improvements and enhancements for the application.
    /// Addresses security vulnerabilities identified in code review.
    /// </summary>
    public static class SecurityImprovementsExtensions
    {
        public static IServiceCollection AddSecurityImprovements(this IServiceCollection services, IConfiguration configuration)
        {
            // Add resource-based authorization
            services.AddScoped<IAuthorizationHandler, ResourceOwnerAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, AdminResourceAuthorizationHandler>();

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                // Resource-level authorization policies
                options.AddPolicy("OwnerOrAdmin", policy =>
                    policy.Requirements.Add(new ResourceOwnerRequirement()));

                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin", "SuperAdmin"));

                options.AddPolicy("SuperAdminOnly", policy =>
                    policy.RequireRole("SuperAdmin"));

                // API-specific policies
                options.AddPolicy("ApiAccess", policy =>
                    policy.RequireAuthenticatedUser()
                          .RequireClaim("scope", "api"));

                // Rate limiting policies
                options.AddPolicy("RateLimited", policy =>
                    policy.RequireAuthenticatedUser());
            });

            // Add input validation services
            services.AddScoped<IInputSanitizer, InputSanitizer>();
            services.AddScoped<ISecurityValidator, SecurityValidator>();

            return services;
        }

        public static IApplicationBuilder UseSecurityImprovements(this IApplicationBuilder app)
        {
            // Add security headers
            app.Use(async (context, next) =>
            {
                // OWASP Security Headers
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
                
                // Content Security Policy
                context.Response.Headers.Add("Content-Security-Policy", 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                    "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self' https://cdn.jsdelivr.net; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none';");

                await next();
            });

            return app;
        }
    }

    /// <summary>
    /// Resource owner authorization requirement
    /// </summary>
    public class ResourceOwnerRequirement : IAuthorizationRequirement { }

    /// <summary>
    /// Resource owner authorization handler
    /// </summary>
    public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnerRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin");

            // Check if user is accessing their own resource or is admin
            if (context.Resource is IUserOwnedResource resource)
            {
                if (resource.UserId.ToString() == userId || isAdmin)
                {
                    context.Succeed(requirement);
                }
            }
            else if (isAdmin)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Admin resource authorization handler
    /// </summary>
    public class AdminResourceAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnerRequirement requirement)
        {
            if (context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Interface for user-owned resources
    /// </summary>
    public interface IUserOwnedResource
    {
        int UserId { get; }
    }

    /// <summary>
    /// Input sanitizer service
    /// </summary>
    public interface IInputSanitizer
    {
        string SanitizeHtml(string input);
        string SanitizeString(string input);
        bool IsValidEmail(string email);
        bool IsValidPhoneNumber(string phoneNumber);
    }

    /// <summary>
    /// Input sanitizer implementation
    /// </summary>
    public class InputSanitizer : IInputSanitizer
    {
        public string SanitizeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove potentially dangerous HTML tags and attributes
            // In production, use a library like HtmlSanitizer
            return System.Net.WebUtility.HtmlEncode(input);
        }

        public string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove potentially dangerous characters
            return input.Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#x27;")
                       .Replace("/", "&#x2F;");
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            // Basic phone number validation
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\+?[\d\s\-\(\)]{10,15}$");
        }
    }

    /// <summary>
    /// Security validator service
    /// </summary>
    public interface ISecurityValidator
    {
        bool IsValidInput(string input, int maxLength = 1000);
        bool ContainsSqlInjection(string input);
        bool ContainsXssAttempt(string input);
    }

    /// <summary>
    /// Security validator implementation
    /// </summary>
    public class SecurityValidator : ISecurityValidator
    {
        private readonly string[] _sqlKeywords = { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "EXEC", "UNION" };
        private readonly string[] _xssPatterns = { "<script", "javascript:", "onload=", "onerror=", "onclick=" };

        public bool IsValidInput(string input, int maxLength = 1000)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            return input.Length <= maxLength && 
                   !ContainsSqlInjection(input) && 
                   !ContainsXssAttempt(input);
        }

        public bool ContainsSqlInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var upperInput = input.ToUpperInvariant();
            return _sqlKeywords.Any(keyword => upperInput.Contains(keyword));
        }

        public bool ContainsXssAttempt(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var lowerInput = input.ToLowerInvariant();
            return _xssPatterns.Any(pattern => lowerInput.Contains(pattern));
        }
    }
}