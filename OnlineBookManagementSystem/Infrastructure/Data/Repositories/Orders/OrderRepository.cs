using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Orders;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Infrastructure.Data.Repositories.Orders
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(BookManagementContext context) : base(context)
        {
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status && !o.IsDeleted)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<int> GetOrdersCountByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status && !o.IsDeleted)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted && o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount.Amount);
        }

        public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted &&
                           o.PaymentStatus == PaymentStatus.Paid &&
                           o.OrderDate.HasValue &&
                           o.OrderDate.Value.Year == year &&
                           o.OrderDate.Value.Month == month)
                .SumAsync(o => o.TotalAmount.Amount);
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await _context.Orders.CountAsync(o => !o.IsDeleted);
        }

        public async Task<int> GetUserOrdersCountAsync(int userId)
        {
            return await _context.Orders.CountAsync(o => o.UserId == userId && !o.IsDeleted);
        }

        public async Task<decimal> GetUserTotalSpentAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted && o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount.Amount);
        }

        public async Task<List<Order>> GetUserRecentOrdersAsync(int userId, int count)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count)
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetOrderStatusDistributionAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .GroupBy(o => o.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
        }

        public async Task<List<Order>> GetOrdersForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted &&
                           o.OrderDate >= startDate &&
                           o.OrderDate <= endDate)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> UpdateAsync(Order entity)
        {
            return await base.UpdateAsync(entity);
        }
    }
}