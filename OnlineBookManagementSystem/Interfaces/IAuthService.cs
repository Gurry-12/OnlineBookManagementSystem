using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.AuthViewModels;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IAuthService
    {
        Task SeedRolesAsync();
        Task<(bool Success, string Message, User? User)> ValidateUserAsync(LoginViewModel data);
        (string AccessToken, string RefreshToken) GenerateTokens(User user);
        Task<(bool Success, string Message, string? ConfirmationToken)> RegisterUserAsync(RegisterViewModel data);
        Task<bool> ConfirmEmailAsync(string token, string email);
        Task<bool> UpdatePasswordAsync(string token, string newPassword);
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<UserViewModel?> GetUserProfileAsync(int userId);
        User GetUserById(int id);
        Task<bool> UpdateUserDetailAsync(ProfileViewModel model);
        void UpdateUserDetailAsync(User user);  // Legacy; prefer async
        Task<bool> AssignRoleAsync(int userId, string roleName);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task RevokeRefreshTokensAsync(int userId);
        Task<(bool Success, string AccessToken, string RefreshToken, string Message)> RefreshTokenAsync(string token);
        Task<List<UserViewModel>> ManageUsers();
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}