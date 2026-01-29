using Microsoft.AspNetCore.Identity;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Users
{
    /// <summary>
    /// Composite Users Service that delegates to focused services.
    /// Maintains backward compatibility while using SOLID-compliant focused services.
    /// </summary>
    public class CompositeUsersService : IUsersService
    {
        private readonly IUserQueryService _userQueryService;
        private readonly IUserCommandService _userCommandService;
        private readonly IUserApprovalService _userApprovalService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CompositeUsersService> _logger;

        public CompositeUsersService(
            IUserQueryService userQueryService,
            IUserCommandService userCommandService,
            IUserApprovalService userApprovalService,
            UserManager<User> userManager,
            ILogger<CompositeUsersService> logger)
        {
            _userQueryService = userQueryService;
            _userCommandService = userCommandService;
            _userApprovalService = userApprovalService;
            _userManager = userManager;
            _logger = logger;
        }

        public int GetTotalUsers()
        {
            return _userQueryService.GetTotalUsers();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _userQueryService.GetTotalUsersCountAsync();
        }

        public async Task<SuperAdminDashboardViewModel> GetSuperAdminDashboardDataAsync()
        {
            return await _userQueryService.GetSuperAdminDashboardDataAsync();
        }

        public async Task<ManageUsersViewModel> GetManageUsersDataAsync(int page, int pageSize, string? search = null, string? role = null, string? status = null)
        {
            return await _userQueryService.GetManageUsersDataAsync(page, pageSize, search, role, status);
        }

        public async Task<AdminUsersViewModel> GetUsersForAdminAsync(int page, int pageSize, string? search = null, string? role = null)
        {
            return await _userQueryService.GetUsersForAdminAsync(page, pageSize, search, role);
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request)
        {
            return await _userCommandService.CreateUserAsync(request);
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            return await _userCommandService.UpdateUserRoleAsync(userId, newRole);
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole, bool isActive)
        {
            var result = await _userCommandService.UpdateUserRoleAsync(userId, newRole);
            if (result)
            {
                await _userCommandService.ToggleUserStatusAsync(userId, isActive);
            }
            return result;
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive)
        {
            return await _userCommandService.ToggleUserStatusAsync(userId, isActive);
        }

        public async Task<List<UserWithRoleViewModel>> GetPendingUsersAsync()
        {
            return await _userQueryService.GetPendingUsersAsync();
        }

        public async Task<(bool Success, string Message)> ApproveUserAsync(int userId, string role)
        {
            return await _userApprovalService.ApproveUserAsync(userId, role);
        }

        public async Task<(bool Success, string Message)> RejectUserAsync(int userId)
        {
            return await _userApprovalService.RejectUserAsync(userId, "Rejected by administrator");
        }

        public async Task<bool> SoftDeleteUserAsync(int userId)
        {
            return await _userCommandService.SoftDeleteUserAsync(userId);
        }

        public async Task<UserDetailsViewModel?> GetUserDetailsAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return null;

                var roles = await _userManager.GetRolesAsync(user);

                return new UserDetailsViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.Name?.Split(' ').FirstOrDefault() ?? string.Empty,
                    LastName = user.Name?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                    IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow,
                    CreatedAt = user.CreatedAt,
                    LastLoginDate = user.LastLoginDate,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? "User"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for user {UserId}", userId);
                return null;
            }
        }

        public async Task<UserStatisticsViewModel> GetUserStatisticsAsync(int userId)
        {
            try
            {
                // This would typically come from repositories, but for now return basic stats
                return new UserStatisticsViewModel
                {
                    UserId = userId,
                    TotalOrders = 0, // Would be populated from order repository
                    TotalSpent = 0m, // Would be populated from order repository
                    FavoriteBooks = 0, // Would be populated from favorites repository
                    ReviewsWritten = 0, // Would be populated from reviews repository
                    LastOrderDate = null,
                    MemberSince = DateTime.UtcNow // Would be populated from user data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics for user {UserId}", userId);
                return new UserStatisticsViewModel { UserId = userId };
            }
        }
    }
}