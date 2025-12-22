using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Services.Admin
{
    public interface IAdminOrderService
    {
        Task<AdminOrderListViewModel> GetOrdersAsync(int page, int pageSize, string? search, string? status);
        Task<Order?> GetOrderDetailsAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int id, string status, int adminUserId);
    }
}
