using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Users;

public class UserApprovalService : IUserApprovalService
{
    private readonly BookManagementContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IActivityLogger _activityLogger;
    private readonly ILogger<UserApprovalService> _logger;

    public UserApprovalService(
        BookManagementContext context,
        UserManager<User> userManager,
        IActivityLogger activityLogger,
        ILogger<UserApprovalService> logger)
    {
        _context = context;
        _userManager = userManager;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<List<UserWithRoleViewModel>> GetPendingUsersAsync()
    {
        try
        {
            var pendingUsers = await _context.Users
                .Where(u => u.IsPendingApproval && !u.IsDeleted)
                .Select(u => new UserWithRoleViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    RequestedRole = u.RequestedRole ?? "User",
                    CreatedAt = u.CreatedAt,
                    IsPendingApproval = u.IsPendingApproval
                })
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            return pendingUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending users");
            return new List<UserWithRoleViewModel>();
        }
    }

    public async Task<(bool Success, string Message)> ApproveUserAsync(int userId, string approvedRole)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (false, "User not found");

            if (!user.IsPendingApproval)
                return (false, "User is not pending approval");

            // Validate role
            var validRoles = new[] { "User", "Admin" };
            if (!validRoles.Contains(approvedRole))
                return (false, "Invalid role");

            // Update user status
            user.IsPendingApproval = false;
            user.RequestedRole = null;
            user.EmailConfirmed = true;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return (false, "Failed to update user");

            // Assign role
            var roleResult = await _userManager.AddToRoleAsync(user, approvedRole);
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to user {UserId}", approvedRole, userId);
                return (false, "Failed to assign role");
            }

            // Log activity
            await _activityLogger.LogActivityAsync(
                "UserApproval",
                $"User approved with role: {approvedRole}", userId);

            _logger.LogInformation("User {UserId} approved with role {Role}", userId, approvedRole);
            return (true, "User approved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving user {UserId}", userId);
            return (false, "An error occurred while approving user");
        }
    }

    public async Task<(bool Success, string Message)> RejectUserAsync(int userId, string reason)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (false, "User not found");

            if (!user.IsPendingApproval)
                return (false, "User is not pending approval");

            // Soft delete the user
            user.IsDeleted = true;
            user.IsPendingApproval = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, "Failed to reject user");

            // Log activity
            await _activityLogger.LogActivityAsync(
                "UserRejection",
                $"User rejected. Reason: {reason}", userId);

            _logger.LogInformation("User {UserId} rejected. Reason: {Reason}", userId, reason);
            return (true, "User rejected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting user {UserId}", userId);
            return (false, "An error occurred while rejecting user");
        }
    }

    public async Task<int> GetPendingUsersCountAsync()
    {
        try
        {
            return await _context.Users
                .CountAsync(u => u.IsPendingApproval && !u.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending users count");
            return 0;
        }
    }
}
