using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Users
{
    public class UserQueryService : IUserQueryService
    {
        private readonly BookManagementContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserQueryService> _logger;

        public UserQueryService(
            BookManagementContext context,
            UserManager<User> userManager,
            ILogger<UserQueryService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public int GetTotalUsers()
        {
            return _context.Users.Count(u => (bool)!u.IsDeleted);
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => (bool)!u.IsDeleted);
        }

        public async Task<SuperAdminDashboardViewModel> GetSuperAdminDashboardDataAsync()
        {
            var totalUsers = await _context.Users.CountAsync(u => (bool)!u.IsDeleted);
            var newUsersToday = await _context.Users.CountAsync(u =>
                (bool)!u.IsDeleted && u.CreatedAt.Date == DateTime.UtcNow.Date);

            var totalBooks = await _context.Books.CountAsync(b => !b.IsDeleted);
            var booksAddedThisMonth = await _context.Books.CountAsync(b =>
                !b.IsDeleted && b.CreatedAt.Month == DateTime.UtcNow.Month && b.CreatedAt.Year == DateTime.UtcNow.Year);

            var totalOrders = await _context.Orders.CountAsync(o => !o.IsDeleted);
            var ordersToday = await _context.Orders.CountAsync(o =>
                !o.IsDeleted && o.OrderDate.HasValue && o.OrderDate.Value.Date == DateTime.UtcNow.Date);

            var totalRevenue = await _context.Orders
                .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

            var revenueToday = await _context.Orders
                .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && o.OrderDate.HasValue && o.OrderDate.Value.Date == DateTime.UtcNow.Date)
                .SumAsync(o => o.TotalAmount);

            // Get today's activities only for dashboard
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var recentActivities = await _context.ActivityLogs
                .Include(al => al.User)
                .Where(al => al.Timestamp >= today && al.Timestamp < tomorrow)
                .OrderByDescending(al => al.Timestamp)
                .Take(10)
                .ToListAsync();

            // Calculate storage usage (simplified)
            var storageUsagePercent = CalculateStorageUsage();

            // Get active sessions (simplified - in production, use a session store)
            var activeSessions = await _context.Users
                .CountAsync(u => (bool)!u.IsDeleted && u.LastLoginDate > DateTime.UtcNow.AddMinutes(-30));

            return new SuperAdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                NewUsersToday = newUsersToday,
                TotalBooks = totalBooks,
                BooksAddedThisMonth = booksAddedThisMonth,
                TotalOrders = totalOrders,
                OrdersToday = ordersToday,
                TotalRevenue = totalRevenue,
                RevenueToday = revenueToday,
                StorageUsagePercent = storageUsagePercent,
                ActiveSessions = activeSessions,
                RecentActivities = recentActivities,
                RecentActivity = recentActivities // For backward compatibility
            };
        }

        public async Task<ManageUsersViewModel> GetManageUsersDataAsync(int page, int pageSize, string? search = null, string? role = null, string? status = null)
        {
            var query = _context.Users.Where(u => (bool)!u.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "active":
                        query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTime.UtcNow);
                        break;
                    case "locked":
                        query = query.Where(u => u.LockoutEnd > DateTime.UtcNow);
                        break;
                    case "inactive":
                        query = query.Where(u => u.LastLoginDate == null || u.LastLoginDate < DateTime.UtcNow.AddDays(-30));
                        break;
                }
            }

            var totalUsers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userViewModels = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "User";

                // Apply role filter
                if (!string.IsNullOrEmpty(role) && userRole != role)
                    continue;

                userViewModels.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = userRole,
                    IsDeleted = (bool)user.IsDeleted,
                    LockoutEnd = user.LockoutEnd,
                    LastLoginDate = user.LastLoginDate,
                    CreatedDate = user.CreatedAt,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return new ManageUsersViewModel
            {
                Users = userViewModels,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalUsers = totalUsers,
                SearchTerm = search,
                SelectedRole = role,
                SelectedStatus = status
            };
        }

        public async Task<AdminUsersViewModel> GetUsersForAdminAsync(int page, int pageSize, string? search = null, string? role = null)
        {
            var query = _context.Users.Where(u => (bool)!u.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            }

            var totalUsers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userViewModels = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "User";

                if (!string.IsNullOrEmpty(role) && userRole != role)
                    continue;

                userViewModels.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = userRole,
                    IsDeleted = (bool)user.IsDeleted,
                    LockoutEnd = user.LockoutEnd,
                    LastLoginDate = user.LastLoginDate,
                    CreatedDate = user.CreatedAt,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return new AdminUsersViewModel
            {
                Users = userViewModels,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalUsers = totalUsers,
                SearchTerm = search,
                RoleFilter = role,
                SelectedRole = role
            };
        }

        public async Task<List<UserWithRoleViewModel>> GetPendingUsersAsync()
        {
            var pendingUsers = await _context.Users
                .Where(u => u.IsPendingApproval && (u.IsDeleted == null || (bool)!u.IsDeleted))
                .OrderBy(u => u.RequestDate)
                .ToListAsync();

            var result = new List<UserWithRoleViewModel>();

            foreach (var user in pendingUsers)
            {
                result.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.RequestedRole ?? "User", // Use requested role as current "role" for display
                    IsDeleted = false,
                    CreatedDate = user.RequestDate ?? user.CreatedAt,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return result;
        }

        private int CalculateStorageUsage()
        {
            try
            {
                // Simplified storage calculation
                // In production, you would check actual disk usage
                var totalBooks = _context.Books.Count(b => !b.IsDeleted);
                var totalUsers = _context.Users.Count(u => (bool)!u.IsDeleted);
                var totalOrders = _context.Orders.Count(o => !o.IsDeleted);

                // Rough calculation based on data volume
                var usage = (totalBooks + totalUsers + totalOrders) / 100;
                return Math.Min(usage, 100); // Cap at 100%
            }
            catch
            {
                return 0;
            }
        }

        // Additional methods for SuperAdmin functionality
        public async Task<int> GetActiveUsersCountAsync()
        {
            try
            {
                // Consider users active if they've logged in within the last 30 days
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                return await _context.Users
                    .CountAsync(u => (bool)!u.IsDeleted && 
                                    u.LastLoginDate.HasValue && 
                                    u.LastLoginDate.Value >= thirtyDaysAgo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users count");
                return 0;
            }
        }
    }
}