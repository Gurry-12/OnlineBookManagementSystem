using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders
{
    /// <summary>
    /// Service interface for order write operations and commands
    /// </summary>
    public interface IOrderCommandService
    {
        // Order management operations
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, int userId);
        Task<bool> CancelOrderAsync(int orderId, int userId);

        // Order creation and processing
        Task<(bool Success, int OrderId, string Message)> CreateOrderAsync(CreateOrderRequest request);
        Task<bool> ProcessOrderAsync(int orderId, int userId);
        Task<bool> CompleteOrderAsync(int orderId, int userId);
    }

    /// <summary>
    /// Request model for creating a new order
    /// </summary>
    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request model for order items
    /// </summary>
    public class OrderItemRequest
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}