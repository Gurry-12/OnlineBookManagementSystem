using OnlineBookManagementSystem.Controllers;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IUsersService
    {
        int GetTotalUsers();
        Task<int> GetTotalUsersCountAsync();
        Task<SuperAdminDashboardViewModel> GetSuperAdminDashboardDataAsync();
        Task<ManageUsersViewModel> GetManageUsersDataAsync(int page, int pageSize, string? search = null, string? role = null, string? status = null);
        Task<Models.ViewModel.AdminUsersViewModel> GetUsersForAdminAsync(int page, int pageSize, string? search = null, string? role = null);
        Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request);
        Task<bool> UpdateUserRoleAsync(int userId, string newRole);
        Task<bool> ToggleUserStatusAsync(int userId, bool isActive);

        // Pending Approval Workflow
        Task<List<UserWithRoleViewModel>> GetPendingUsersAsync();
        Task<(bool Success, string Message)> ApproveUserAsync(int userId, string role);
        Task<(bool Success, string Message)> RejectUserAsync(int userId);
    }
}
