using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Orders;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Orders
{
    /// <summary>
    /// Refactored Order Command Service following SRP.
    /// Only handles order command business logic, delegates data access to repository.
    /// </summary>
    public class RefactoredOrderCommandService : IOrderCommandService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RefactoredOrderCommandService> _logger;
        private readonly IActivityLogger _activityLogger;

        public RefactoredOrderCommandService(
            IOrderRepository orderRepository,
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork,
            ILogger<RefactoredOrderCommandService> logger,
            IActivityLogger activityLogger)
        {
            _orderRepository = orderRepository;
            _bookRepository = bookRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _activityLogger = activityLogger;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, int userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (order == null) return false;

                var oldStatus = order.Status;
                order.Status = status;
                order.UpdateTimestamp();

                // Update payment status based on order status
                if (status == OrderStatus.Delivered)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                }
                else if (status == OrderStatus.Cancelled)
                {
                    order.PaymentStatus = PaymentStatus.Refunded;
                    await RestoreStockQuantitiesAsync(order);
                }

                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log the activity
                await _activityLogger.LogAsync(
                    "Order Status Update",
                    $"Order #{orderId} status changed from {oldStatus} to {status}",
                    userId);

                _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus} by user {UserId}",
                    orderId, oldStatus, status, userId);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating order {OrderId} status to {Status}", orderId, status);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (order == null) return false;

                // Only allow cancellation of pending or processing orders
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                {
                    _logger.LogWarning("Cannot cancel order {OrderId} with status {Status}", orderId, order.Status);
                    return false;
                }

                order.Status = OrderStatus.Cancelled;
                order.PaymentStatus = PaymentStatus.Refunded;
                order.UpdateTimestamp();

                // Restore stock quantities
                await RestoreStockQuantitiesAsync(order);

                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log the activity
                await _activityLogger.LogAsync(
                    "Order Cancelled",
                    $"Order #{orderId} cancelled",
                    userId);

                _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<(bool Success, int OrderId, string Message)> CreateOrderAsync(CreateOrderRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create order entity
                var order = new Order
                {
                    UserId = request.UserId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    PaymentMethod = request.PaymentMethod,
                    Email = request.Email,
                    PhoneNumber = request.Phone,
                    Notes = request.Notes,
                    ShippingAddress = new Address(
                        request.FullName,
                        request.Address,
                        request.City,
                        request.State,
                        request.ZipCode,
                        "USA" // Default
                    )
                };

                // Create order details and calculate total
                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var item in request.Items)
                {
                    var book = await _bookRepository.GetByIdAsync(item.BookId);
                    if (book == null)
                    {
                        return (false, 0, $"Book with ID {item.BookId} not found");
                    }

                    if (book.StockQuantity < item.Quantity)
                    {
                        return (false, 0, $"Insufficient stock for book '{book.Title}'. Available: {book.StockQuantity}, Requested: {item.Quantity}");
                    }

                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = new Money(item.Price),
                        Subtotal = new Money(item.Price * item.Quantity)
                    };

                    orderDetails.Add(orderDetail);
                    totalAmount += item.Price * item.Quantity;

                    // Update stock
                    book.StockQuantity -= item.Quantity;
                    book.UpdateTimestamp();
                    await _bookRepository.UpdateAsync(book);
                }

                order.TotalAmount = new Money(totalAmount);

                // Add order details to the order
                foreach (var detail in orderDetails)
                {
                    order.OrderDetails.Add(detail);
                }

                // Save order
                var createdOrder = await _orderRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log the activity
                await _activityLogger.LogAsync(
                    "Order Created",
                    $"New order #{createdOrder.Id} created with total {totalAmount:C}",
                    request.UserId);

                _logger.LogInformation("Order {OrderId} created for user {UserId} with total {Total}",
                    createdOrder.Id, request.UserId, totalAmount);

                return (true, createdOrder.Id, "Order created successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating order for user {UserId}", request.UserId);
                return (false, 0, "Failed to create order");
            }
        }

        public async Task<bool> ProcessOrderAsync(int orderId, int userId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Processing, userId);
        }

        public async Task<bool> CompleteOrderAsync(int orderId, int userId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Delivered, userId);
        }

        private async Task RestoreStockQuantitiesAsync(Order order)
        {
            foreach (var orderDetail in order.OrderDetails)
            {
                var book = await _bookRepository.GetByIdAsync(orderDetail.BookId);
                if (book != null)
                {
                    book.StockQuantity += orderDetail.Quantity;
                    book.UpdateTimestamp();
                    await _bookRepository.UpdateAsync(book);
                }
            }
        }
    }
}