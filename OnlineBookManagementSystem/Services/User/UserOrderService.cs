using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces.Repositories;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Services.User
{
    public class UserOrderService : IUserOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public UserOrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<Order>> GetMyOrdersAsync(int userId)
        {
             // Repository doesn't expose List directly for specific user, so we use Query() or add method.
             // Using Query() as repository pattern allows IQueryable often, or we should add GetByUserId to Repo.
             // I'll use Query() + EF extension
             return await _orderRepository.Query()
                 .Where(o => o.UserId == userId)
                 .Include(o => o.OrderDetails)
                 .ThenInclude(od => od.Book)
                 .OrderByDescending(o => o.OrderDate)
                 .ToListAsync();
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId, int userId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId) return null;
            return order;
        }
    }
}
