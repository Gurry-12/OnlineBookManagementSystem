using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly BookManagementContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly IActivityLogger _activityLogger;

        public OrderService(
            BookManagementContext context,
            ILogger<OrderService> logger,
            IActivityLogger activityLogger)
        {
            _context = context;
            _logger = logger;
            _activityLogger = activityLogger;
        }

        public int GetTotalOrders()
        {
            return _context.Orders.Count(o => !o.IsDeleted);
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
                .Where(o => o.UserId == userId && !o.IsDeleted && o.PaymentStatus == "Paid")
                .SumAsync(o => o.TotalAmount);
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

        public async Task<OrderHistoryViewModel> GetUserOrderHistoryAsync(int userId, int page, int pageSize, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(o => o.OrderDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(o => o.OrderDate <= dateTo.Value.AddDays(1));
            }

            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new OrderHistoryViewModel
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalOrders = totalOrders,
                StatusFilter = status,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
        }

        public async Task<Order?> GetUserOrderDetailsAsync(int orderId, int userId)
        {
            return await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId && !o.IsDeleted)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                        .ThenInclude(b => b.Category)
                .Include(o => o.User)
                .FirstOrDefaultAsync();
        }

        public async Task<AdminOrderListViewModel> GetOrdersForAdminAsync(int page, int pageSize, string? search = null, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = _context.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => 
                    o.FullName.Contains(search) || 
                    o.User.Email.Contains(search) ||
                    o.Id.ToString().Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(o => o.OrderDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(o => o.OrderDate <= dateTo.Value.AddDays(1));
            }

            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new AdminOrderListViewModel
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalOrders = totalOrders,
                SearchTerm = search ?? string.Empty,
                StatusFilter = status ?? string.Empty,
                PendingOrders = await _context.Orders.CountAsync(o => !o.IsDeleted && o.Status == "Pending"),
                ProcessingOrders = await _context.Orders.CountAsync(o => !o.IsDeleted && o.Status == "Processing"),
                CompletedOrders = await _context.Orders.CountAsync(o => !o.IsDeleted && (o.Status == "Delivered" || o.Status == "Completed"))
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, int userId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
                if (order == null) return false;

                var oldStatus = order.Status;
                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                // Update payment status based on order status
                if (status == "Delivered")
                {
                    order.PaymentStatus = "Paid";
                }
                else if (status == "Cancelled")
                {
                    order.PaymentStatus = "Refunded";
                    
                    // Restore stock quantities
                    var orderDetails = await _context.OrderDetails
                        .Include(od => od.Book)
                        .Where(od => od.OrderId == orderId)
                        .ToListAsync();

                    foreach (var detail in orderDetails)
                    {
                        detail.Book.StockQuantity += detail.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                await _activityLogger.LogAsync("OrderStatusUpdated", 
                    $"Order #{orderId} status changed from '{oldStatus}' to '{status}'", userId);

                _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus} by user {UserId}", 
                    orderId, oldStatus, status, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update order status for order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && !o.IsDeleted);

                if (order == null)
                    return false;

                // Only allow cancellation of pending or processing orders
                if (order.Status != "Pending" && order.Status != "Processing")
                    return false;

                var oldStatus = order.Status;
                order.Status = "Cancelled";
                order.PaymentStatus = "Refunded";
                order.UpdatedAt = DateTime.UtcNow;

                // Restore stock quantities
                foreach (var detail in order.OrderDetails)
                {
                    detail.Book.StockQuantity += detail.Quantity;
                }

                await _context.SaveChangesAsync();
                await _activityLogger.LogAsync("OrderCancelled", 
                    $"Order #{orderId} cancelled by user", userId);

                _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel order {OrderId} for user {UserId}", orderId, userId);
                return false;
            }
        }
    }
}
