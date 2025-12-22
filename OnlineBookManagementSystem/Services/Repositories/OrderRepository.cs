using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces.Repositories;
using OnlineBookManagementSystem.Models;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Services.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly BookManagementContext _context;

        public OrderRepository(BookManagementContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task<List<Order>> GetPaginatedAsync(int skip, int take, Expression<Func<Order, bool>>? filter = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .Where(o => !o.IsDeleted);

            if (filter != null)
                query = query.Where(filter);

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Order, bool>>? filter = null)
        {
             var query = _context.Orders.Where(o => !o.IsDeleted);
             if (filter != null) query = query.Where(filter);
             return await query.CountAsync();
        }

        public Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IQueryable<Order> Query()
        {
            return _context.Orders.Where(o => !o.IsDeleted).AsQueryable();
        }
    }
}
