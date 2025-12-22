using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Services.User
{
    public interface IUserOrderService
    {
        Task<List<Order>> GetMyOrdersAsync(int userId);
        Task<Order?> GetOrderDetailsAsync(int orderId, int userId);
    }
}
