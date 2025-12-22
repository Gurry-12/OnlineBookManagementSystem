using OnlineBookManagementSystem.Models;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<List<Order>> GetPaginatedAsync(int skip, int take, Expression<Func<Order, bool>>? filter = null);
        Task<int> CountAsync(Expression<Func<Order, bool>>? filter = null);
        Task UpdateAsync(Order order);
        Task SaveChangesAsync();
        IQueryable<Order> Query();
    }
}
