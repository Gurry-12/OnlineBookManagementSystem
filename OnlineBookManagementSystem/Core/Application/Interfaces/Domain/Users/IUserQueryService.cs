using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users
{
    /// <summary>
    /// Service interface for user read operations and queries
    /// Follows SRP - Only handles user queries, not approval workflow
    /// </summary>
    public interface IUserQueryService
    {
        // Basic user queries
        int GetTotalUsers();
        Task<int> GetTotalUsersCountAsync();

        // Dashboard and statistics
        Task<SuperAdminDashboardViewModel> GetSuperAdminDashboardDataAsync();
        Task<ManageUsersViewModel> GetManageUsersDataAsync(int page, int pageSize, string? search = null, string? role = null, string? status = null);
        Task<AdminUsersViewModel> GetUsersForAdminAsync(int page, int pageSize, string? search = null, string? role = null);
        
        // Additional methods for SuperAdmin functionality
        Task<int> GetActiveUsersCountAsync();
        Task<List<UserWithRoleViewModel>> GetPendingUsersAsync();
    }
}
