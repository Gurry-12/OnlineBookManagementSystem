using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Controllers;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Services
{
    public class UsersService : IUsersService
    {
        private readonly BookManagementContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<UsersService> _logger;
        private readonly IActivityLogger _activityLogger;

        public UsersService(
            BookManagementContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<UsersService> logger,
            IActivityLogger activityLogger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _activityLogger = activityLogger;
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
                .Where(o => !o.IsDeleted && o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);
            
            var revenueToday = await _context.Orders
                .Where(o => !o.IsDeleted && o.Status == "Completed" && o.OrderDate.HasValue && o.OrderDate.Value.Date == DateTime.UtcNow.Date)
                .SumAsync(o => o.TotalAmount);

            var recentActivities = await _context.ActivityLogs
                .Include(al => al.User)
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

        public async Task<Models.ViewModel.AdminUsersViewModel> GetUsersForAdminAsync(int page, int pageSize, string? search = null, string? role = null)
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

            return new Models.ViewModel.AdminUsersViewModel
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

        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return (false, "User with this email already exists");
                }

                // Validate role
                if (!await _roleManager.RoleExistsAsync(request.Role))
                {
                    return (false, "Invalid role specified");
                }

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    Name = request.Name,
                    EmailConfirmed = true,
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(user, request.Role);

                _logger.LogInformation("User created: {Email} with role {Role}", request.Email, request.Role);
                return (true, "User created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user: {Email}", request.Email);
                return (false, "An error occurred while creating the user");
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || (bool)user.IsDeleted)
                    return false;

                if (!await _roleManager.RoleExistsAsync(newRole))
                    return false;

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);

                _logger.LogInformation("User role updated: {UserId} to {Role}", userId, newRole);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user role: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return false;

                if (isActive)
                {
                    // Activate user
                    user.LockoutEnd = null;
                    user.IsDeleted = false;
                }
                else
                {
                    // Deactivate user
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                }

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle user status: {UserId}", userId);
                return false;
            }
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

        public async Task<(bool Success, string Message)> ApproveUserAsync(int userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (false, "User not found");

            if (!await _roleManager.RoleExistsAsync(role)) return (false, "Invalid role");

            user.IsPendingApproval = false;
            user.IsEmailConfirmed = true; // Auto confirm email on approval if not already

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return (false, "Failed to update user");

            // Assign role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            // Send email notification (placeholder)
            // await _emailSender.SendEmailAsync(user.Email, "Account Approved", "Your account has been approved.");

            await _activityLogger.LogAsync("ApproveUser", $"User {user.Email} approved as {role}", 0); // System action

            return (true, "User approved successfully");
        }

        public async Task<(bool Success, string Message)> RejectUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (false, "User not found");

            // Soft delete
            user.IsDeleted = true;
            user.IsPendingApproval = false; // clear pending

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return (false, "Failed to reject user");

             // Send email notification (placeholder)

            await _activityLogger.LogAsync("RejectUser", $"User {user.Email} rejected", 0);

            return (true, "User rejected successfully");
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
    }

    // Additional ViewModels
    public class AdminUsersViewModel
    {
        public List<UserWithRoleViewModel> Users { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
        public string? SearchTerm { get; set; }
        public string? SelectedRole { get; set; }
    }
}
