using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication
{
    /// <summary>
    /// Service interface for user authentication operations
    /// </summary>
    public interface IUserAuthenticationService
    {
        // Authentication operations
        Task<AuthenticationResult> AuthenticateAsync(string email, string password);
        Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);

        // Password operations
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<(bool Success, string Message)> ResetPasswordAsync(string email);
        Task<(bool Success, string Message)> ConfirmPasswordResetAsync(string token, string email, string newPassword);

        // Email confirmation
        Task<(bool Success, string Message)> SendEmailConfirmationAsync(int userId);
        Task<(bool Success, string Message)> ConfirmEmailAsync(string token, string email);

        // Account lockout
        Task<bool> LockAccountAsync(int userId, TimeSpan lockoutDuration);
        Task<bool> UnlockAccountAsync(int userId);
        Task<bool> IsAccountLockedAsync(int userId);
    }

    /// <summary>
    /// Represents the result of an authentication operation
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}