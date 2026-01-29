using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User entity);
        Task<User> UpdateAsync(User entity);
        Task DeleteAsync(User entity);
        Task<User?> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetPendingApprovalUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(string roleName);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<List<User>> GetRecentUsersAsync(int count);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetUserWithOrdersAsync(int userId);
        Task<User?> GetUserWithFavoritesAsync(int userId);
        Task<List<User>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 10);
    }
}