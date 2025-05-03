using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IAuthInterface
    {
        Task<(bool Success, string Message, User User)> ValidateUserAsync(LoginViewModel data);
        string GenerateJwtToken(User user);
        Task<bool> RegisterUserAsync(RegisterViewModel data);
        Task<UserViewModel> GetUserProfileAsync(int userId);
        User GetUserById(int id);
Task<bool> UpdateUserDetailAsync(ProfileViewModel model);
        void UpdateUserDetailAsync(User user);
    }
}
