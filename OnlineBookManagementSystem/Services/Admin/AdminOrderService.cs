using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Interfaces.Repositories;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Services.Admin
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IActivityLogger _logger;

        public AdminOrderService(IOrderRepository orderRepository, IActivityLogger logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<AdminOrderListViewModel> GetOrdersAsync(int page, int pageSize, string? search, string? status)
        {
            var orders = await _orderRepository.GetPaginatedAsync(
                (page - 1) * pageSize,
                pageSize,
                o => (string.IsNullOrEmpty(search) || o.FullName.Contains(search) || o.Id.ToString() == search) &&
                     (string.IsNullOrEmpty(status) || o.Status == status)
            );

            var total = await _orderRepository.CountAsync(
                o => (string.IsNullOrEmpty(search) || o.FullName.Contains(search) || o.Id.ToString() == search) &&
                     (string.IsNullOrEmpty(status) || o.Status == status)
            );

            return new AdminOrderListViewModel
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchTerm = search,
                StatusFilter = status
            };
        }

        public async Task<Order?> GetOrderDetailsAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status, int adminUserId)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;

            var oldStatus = order.Status;
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
            await _logger.LogAsync("OrderStatusUpdate", $"Order #{id} status changed: {oldStatus} -> {status}", adminUserId);
            return true;
        }
    }
}
