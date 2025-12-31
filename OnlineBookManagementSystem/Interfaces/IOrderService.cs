using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IOrderService
    {
        int GetTotalOrders();
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetUserOrdersCountAsync(int userId);
        Task<decimal> GetUserTotalSpentAsync(int userId);
        Task<List<Order>> GetUserRecentOrdersAsync(int userId, int count);
        Task<OrderHistoryViewModel> GetUserOrderHistoryAsync(int userId, int page, int pageSize, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<Order?> GetUserOrderDetailsAsync(int orderId, int userId);
        Task<AdminOrderListViewModel> GetOrdersForAdminAsync(int page, int pageSize, string? search = null, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, int userId);
        Task<bool> CancelOrderAsync(int orderId, int userId);
        Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync();
        Task<List<OrderStatusViewModel>> GetOrderStatusDistributionAsync();
    }

    // Additional ViewModels for Orders
    public class OrderHistoryViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalOrders { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class AdminOrdersViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalOrders { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}