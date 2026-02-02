using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Users;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Infrastructure.Data.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        protected readonly BookManagementContext _context;

        public UserRepository(BookManagementContext context)
        {
            _context = context;
        }

        public async Task<User> AddAsync(User entity)
        {
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<User> UpdateAsync(User entity)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(User entity)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && (u.IsDeleted == null || !(bool)u.IsDeleted));
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => u.IsDeleted == null || !(bool)u.IsDeleted)
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && (u.IsDeleted == null || !(bool)u.IsDeleted));
        }

        public async Task<List<User>> GetPendingApprovalUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsPendingApproval && (u.IsDeleted == null || !(bool)u.IsDeleted))
                .OrderBy(u => u.RequestDate)
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByRoleAsync(string roleName)
        {
            // Use a different approach since IdentityUserRole<int> doesn't have direct Role navigation
            var roleUsers = await _context.UserRoles
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .Where(x => x.Name == roleName)
                .Select(x => x.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => roleUsers.Contains(u.Id) &&
                           (u.IsDeleted == null || !(bool)u.IsDeleted))
                .ToListAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users
                .CountAsync(u => u.IsDeleted == null || !(bool)u.IsDeleted);
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            return await _context.Users
                .CountAsync(u => (u.IsDeleted == null || !(bool)u.IsDeleted) &&
                                u.LastLoginDate >= thirtyDaysAgo);
        }

        public async Task<List<User>> GetRecentUsersAsync(int count)
        {
            return await _context.Users
                .Where(u => u.IsDeleted == null || !(bool)u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && (u.IsDeleted == null || !(bool)u.IsDeleted));
        }

        public async Task<User?> GetUserWithOrdersAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Orders.Where(o => !o.IsDeleted))
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(u => u.Id == userId && (u.IsDeleted == null || !(bool)u.IsDeleted));
        }

        public async Task<User?> GetUserWithFavoritesAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserFavorites.Where(uf => !uf.IsDeleted))
                    .ThenInclude(uf => uf.Book)
                .FirstOrDefaultAsync(u => u.Id == userId && (u.IsDeleted == null || !(bool)u.IsDeleted));
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            var query = _context.Users
                .Where(u => u.IsDeleted == null || !(bool)u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => u.Name.Contains(searchTerm) ||
                                        u.Email.Contains(searchTerm));
            }

            return await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}