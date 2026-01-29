using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<List<Order>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 10);
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetMonthlyRevenueAsync(int year, int month);
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetUserOrdersCountAsync(int userId);
        Task<decimal> GetUserTotalSpentAsync(int userId);
        Task<List<Order>> GetUserRecentOrdersAsync(int userId, int count);
        Task<List<Order>> GetRecentOrdersAsync(int count);
        Task<Dictionary<string, int>> GetOrderStatusDistributionAsync();
        Task<List<Order>> GetOrdersForDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Order> UpdateAsync(Order entity);
    }
}