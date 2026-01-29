using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users
{
    /// <summary>
    /// Service interface for user write operations and commands
    /// Follows SRP - Only handles user CRUD, not approval workflow
    /// </summary>
    public interface IUserCommandService
    {
        // User management operations
        Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request);
        Task<bool> UpdateUserRoleAsync(int userId, string newRole);
        Task<bool> ToggleUserStatusAsync(int userId, bool isActive);
        Task<bool> SoftDeleteUserAsync(int userId);
        
        // Approval workflow operations (delegated from IUserApprovalService)
        Task<(bool Success, string Message)> ApproveUserAsync(int userId, string role);
        Task<(bool Success, string Message)> RejectUserAsync(int userId);
    }
}
