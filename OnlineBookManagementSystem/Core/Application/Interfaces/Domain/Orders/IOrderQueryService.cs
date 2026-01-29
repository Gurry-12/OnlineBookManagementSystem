using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.User;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders
{
    /// <summary>
    /// Service interface for order read operations and queries
    /// </summary>
    public interface IOrderQueryService
    {
        // Basic order queries
        int GetTotalOrders();
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetUserOrdersCountAsync(int userId);
        Task<decimal> GetUserTotalSpentAsync(int userId);

        // User order queries
        Task<List<Order>> GetUserRecentOrdersAsync(int userId, int count);
        Task<OrderHistoryViewModel> GetUserOrderHistoryAsync(int userId, int page, int pageSize, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<Order?> GetUserOrderDetailsAsync(int orderId, int userId);

        // Admin order queries
        Task<AdminOrderListViewModel> GetOrdersForAdminAsync(int page, int pageSize, string? search = null, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);

        // Analytics and reporting
        Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync();
        Task<List<OrderStatusViewModel>> GetOrderStatusDistributionAsync();
        
        // Additional methods for SuperAdmin and Admin functionality
        Task<decimal> GetTotalRevenueAsync();
        Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync(int months, int year);
        Task<List<Order>> GetRecentOrdersAsync(int count = 10);
        Task<Order?> GetOrderDetailsAsync(int orderId);
    }
}