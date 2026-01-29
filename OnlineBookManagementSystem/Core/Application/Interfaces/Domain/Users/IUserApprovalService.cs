using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;

public interface IUserApprovalService
{
    Task<List<UserWithRoleViewModel>> GetPendingUsersAsync();
    Task<(bool Success, string Message)> ApproveUserAsync(int userId, string approvedRole);
    Task<(bool Success, string Message)> RejectUserAsync(int userId, string reason);
    Task<int> GetPendingUsersCountAsync();
}
