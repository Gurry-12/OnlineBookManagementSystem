using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Orders;
using OnlineBookManagementSystem.Core.Application.Mappings;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.User;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Orders
{
    /// <summary>
    /// Refactored Order Query Service following SRP.
    /// Only handles order querying business logic, delegates data access to repository.
    /// </summary>
    public class RefactoredOrderQueryService : IOrderQueryService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<RefactoredOrderQueryService> _logger;

        public RefactoredOrderQueryService(
            IOrderRepository orderRepository,
            ILogger<RefactoredOrderQueryService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public int GetTotalOrders()
        {
            return _orderRepository.GetTotalOrdersCountAsync().GetAwaiter().GetResult();
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await _orderRepository.GetTotalOrdersCountAsync();
        }

        public async Task<int> GetUserOrdersCountAsync(int userId)
        {
            return await _orderRepository.GetUserOrdersCountAsync(userId);
        }

        public async Task<decimal> GetUserTotalSpentAsync(int userId)
        {
            return await _orderRepository.GetUserTotalSpentAsync(userId);
        }

        public async Task<List<Order>> GetUserRecentOrdersAsync(int userId, int count)
        {
            return await _orderRepository.GetUserRecentOrdersAsync(userId, count);
        }

        public async Task<OrderHistoryViewModel> GetUserOrderHistoryAsync(int userId, int page, int pageSize, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                var orders = await _orderRepository.GetUserOrdersAsync(userId, page, pageSize);
                var totalOrders = await _orderRepository.GetUserOrdersCountAsync(userId);
                var totalSpent = await _orderRepository.GetUserTotalSpentAsync(userId);

                // Apply filters if provided
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    orders = orders.Where(o => o.Status == orderStatus).ToList();
                }

                if (dateFrom.HasValue)
                {
                    orders = orders.Where(o => o.OrderDate >= dateFrom.Value).ToList();
                }

                if (dateTo.HasValue)
                {
                    orders = orders.Where(o => o.OrderDate <= dateTo.Value).ToList();
                }

                return new OrderHistoryViewModel
                {
                    Orders = orders.Select(o => o.ToOrderHistoryItem()).ToList(),
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize),
                    TotalOrders = totalOrders,
                    TotalSpent = totalSpent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user order history for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Order?> GetUserOrderDetailsAsync(int orderId, int userId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                return order?.UserId == userId ? order : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details for order {OrderId} and user {UserId}", orderId, userId);
                throw;
            }
        }

        public async Task<AdminOrderListViewModel> GetOrdersForAdminAsync(int page, int pageSize, string? search = null, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                var orders = await _orderRepository.GetRecentOrdersAsync(pageSize * 10); // Get more for filtering
                var totalOrders = await _orderRepository.GetTotalOrdersCountAsync();

                // Apply filters
                if (!string.IsNullOrEmpty(search))
                {
                    orders = orders.Where(o =>
                        o.User.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        o.User.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        o.Id.ToString().Contains(search)).ToList();
                }

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    orders = orders.Where(o => o.Status == orderStatus).ToList();
                }

                if (dateFrom.HasValue)
                {
                    orders = orders.Where(o => o.OrderDate >= dateFrom.Value).ToList();
                }

                if (dateTo.HasValue)
                {
                    orders = orders.Where(o => o.OrderDate <= dateTo.Value).ToList();
                }

                // Apply pagination
                var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return new AdminOrderListViewModel
                {
                    Orders = pagedOrders.Select(o => o.ToAdminOrderItem()).ToList(),
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(orders.Count / (double)pageSize),
                    TotalOrders = orders.Count,
                    StatusDistribution = await _orderRepository.GetOrderStatusDistributionAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin order list");
                throw;
            }
        }

        public async Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var monthlyRevenue = new List<MonthlyRevenueViewModel>();

                for (int month = 1; month <= 12; month++)
                {
                    var revenue = await _orderRepository.GetMonthlyRevenueAsync(currentYear, month);
                    monthlyRevenue.Add(new MonthlyRevenueViewModel
                    {
                        Month = new DateTime(currentYear, month, 1).ToString("MMM"),
                        Revenue = revenue
                    });
                }

                return monthlyRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly revenue");
                throw;
            }
        }

        public async Task<List<OrderStatusViewModel>> GetOrderStatusDistributionAsync()
        {
            try
            {
                var distribution = await _orderRepository.GetOrderStatusDistributionAsync();
                return distribution.Select(kvp => new OrderStatusViewModel
                {
                    Status = kvp.Key,
                    Count = kvp.Value
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order status distribution");
                throw;
            }
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                return await _orderRepository.GetTotalRevenueAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total revenue");
                throw;
            }
        }

        public async Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync(int months, int year)
        {
            try
            {
                var monthlyRevenue = new List<MonthlyRevenueViewModel>();

                // Ensure months is within valid range (1-12)
                months = Math.Min(Math.Max(months, 1), 12);

                for (int month = 1; month <= months; month++)
                {
                    var revenue = await _orderRepository.GetMonthlyRevenueAsync(year, month);
                    monthlyRevenue.Add(new MonthlyRevenueViewModel
                    {
                        Month = new DateTime(year, month, 1).ToString("MMM"),
                        Revenue = revenue
                    });
                }

                return monthlyRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly revenue for {Months} months in {Year}", months, year);
                throw;
            }
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                return await _orderRepository.GetRecentOrdersAsync(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                throw;
            }
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            try
            {
                return await _orderRepository.GetOrderWithDetailsAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<int> GetPendingOrdersCountAsync()
        {
            try
            {
                return await _orderRepository.GetOrdersCountByStatusAsync(OrderStatus.Pending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending orders count");
                throw;
            }
        }
    }
}