using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Presentation.Controllers.Admin
{
    /// <summary>
    /// Handles admin order management functionality following SRP.
    /// Responsible only for order administration tasks.
    /// </summary>
    [Authorize(Policy = "AdminOrHigher")]
    public class AdminOrderManagementController : BaseController
    {
        private readonly IOrderQueryService _orderQueryService;
        private readonly IOrderCommandService _orderCommandService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<AdminOrderManagementController> _logger;

        public AdminOrderManagementController(
            IOrderQueryService orderQueryService,
            IOrderCommandService orderCommandService,
            IActivityLogger activityLogger,
            ILogger<AdminOrderManagementController> logger)
        {
            _orderQueryService = orderQueryService;
            _orderCommandService = orderCommandService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> OrderManagement(int page = 1, string? search = null, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var viewModel = await _orderQueryService.GetOrdersForAdminAsync(page, 20, search, status, dateFrom, dateTo);

                ViewBag.Search = search;
                ViewBag.Status = status;
                ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
                ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

                await _activityLogger.LogAsync("ViewOrders", "Admin order management accessed", userId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order management for admin user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load orders.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                if (!Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    return Json(new { success = false, message = "Invalid order status" });
                }

                bool success = await _orderCommandService.UpdateOrderStatusAsync(orderId, orderStatus, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateOrderStatus", $"Order {orderId} status changed to {status}", userId);
                    return Json(new { success = true, message = "Order status updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update order status" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status to {Status} by admin {UserId}", orderId, status, userId);
                return Json(new { success = false, message = "An error occurred while updating order status" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(int orderId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                bool success = await _orderCommandService.ProcessOrderAsync(orderId, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("ProcessOrder", $"Order {orderId} marked as processing", userId);
                    return Json(new { success = true, message = "Order marked as processing" });
                }
                return Json(new { success = false, message = "Failed to process order" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order {OrderId} by admin {UserId}", orderId, userId);
                return Json(new { success = false, message = "An error occurred while processing order" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                bool success = await _orderCommandService.CompleteOrderAsync(orderId, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("CompleteOrder", $"Order {orderId} marked as completed", userId);
                    return Json(new { success = true, message = "Order marked as completed" });
                }
                return Json(new { success = false, message = "Failed to complete order" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing order {OrderId} by admin {UserId}", orderId, userId);
                return Json(new { success = false, message = "An error occurred while completing order" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                bool success = await _orderCommandService.CancelOrderAsync(orderId, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("CancelOrder", $"Order {orderId} cancelled by admin", userId);
                    return Json(new { success = true, message = "Order cancelled successfully" });
                }
                return Json(new { success = false, message = "Failed to cancel order or order cannot be cancelled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} by admin {UserId}", orderId, userId);
                return Json(new { success = false, message = "An error occurred while cancelling order" });
            }
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var order = await _orderQueryService.GetOrderDetailsAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(OrderManagement));
                }

                await _activityLogger.LogAsync("ViewOrderDetails", $"Admin viewed details for order {id}", userId);
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for order {OrderId}", id);
                TempData["ErrorMessage"] = "Failed to load order details.";
                return RedirectToAction(nameof(OrderManagement));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderStats()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var monthlyRevenue = await _orderQueryService.GetMonthlyRevenueAsync();
                var statusDistribution = await _orderQueryService.GetOrderStatusDistributionAsync();
                
                return Json(new { 
                    success = true, 
                    data = new { 
                        monthlyRevenue, 
                        statusDistribution 
                    } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order statistics");
                return Json(new { success = false, message = "Failed to load order statistics" });
            }
        }
    }
}