using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers.User
{
    /// <summary>
    /// Handles user order functionality following SRP.
    /// Responsible only for user order management and history.
    /// </summary>
    [Authorize(Policy = "UserOrHigher")]
    public class UserOrdersController : BaseController
    {
        private readonly IOrderQueryService _orderQueryService;
        private readonly IOrderCommandService _orderCommandService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<UserOrdersController> _logger;

        public UserOrdersController(
            IOrderQueryService orderQueryService,
            IOrderCommandService orderCommandService,
            IActivityLogger activityLogger,
            ILogger<UserOrdersController> logger)
        {
            _orderQueryService = orderQueryService;
            _orderCommandService = orderCommandService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> OrderHistory(int page = 1, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var viewModel = await _orderQueryService.GetUserOrderHistoryAsync(userId, page, 10, status, dateFrom, dateTo);

                ViewBag.Status = status;
                ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
                ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

                await _activityLogger.LogAsync("ViewOrderHistory", "User viewed order history", userId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order history for user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load order history.";
                return View();
            }
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var order = await _orderQueryService.GetUserOrderDetailsAsync(id, userId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(OrderHistory));
                }

                await _activityLogger.LogAsync("ViewOrderDetails", $"Viewed order details for order {id}", userId);
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for order {OrderId} and user {UserId}", id, userId);
                TempData["ErrorMessage"] = "Failed to load order details.";
                return RedirectToAction(nameof(OrderHistory));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (request.OrderId <= 0)
            {
                return Json(new { success = false, message = "Invalid order ID" });
            }

            try
            {
                var success = await _orderCommandService.CancelOrderAsync(request.OrderId, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("CancelOrder", $"Order {request.OrderId} cancelled by user", userId);
                    return Json(new { success = true, message = "Order cancelled successfully" });
                }
                return Json(new { success = false, message = "Unable to cancel order or order not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", request.OrderId, userId);
                return Json(new { success = false, message = "An error occurred while cancelling the order" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentOrders(int count = 5)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var recentOrders = await _orderQueryService.GetUserRecentOrdersAsync(userId, count);
                return Json(new { success = true, orders = recentOrders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent orders for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load recent orders" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderStats()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var stats = new
                {
                    totalOrders = await _orderQueryService.GetUserOrdersCountAsync(userId),
                    totalSpent = await _orderQueryService.GetUserTotalSpentAsync(userId),
                    recentOrders = await _orderQueryService.GetUserRecentOrdersAsync(userId, 3)
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order statistics for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load order statistics" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReorderItems(int orderId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (orderId <= 0)
            {
                return Json(new { success = false, message = "Invalid order ID" });
            }

            try
            {
                // Get the original order details
                var originalOrder = await _orderQueryService.GetUserOrderDetailsAsync(orderId, userId);
                if (originalOrder == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // This would typically involve adding the items back to cart
                // For now, we'll just return success with a message to redirect to the books
                await _activityLogger.LogAsync("ReorderAttempt", $"User attempted to reorder items from order {orderId}", userId);
                
                return Json(new { 
                    success = true, 
                    message = "Items from this order are available for browsing. Please add them to your cart manually.",
                    redirectUrl = Url.Action("UserBookList", "UserBookBrowsing")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering items from order {OrderId} for user {UserId}", orderId, userId);
                return Json(new { success = false, message = "An error occurred while processing reorder" });
            }
        }

    }

    // Request models
    public class CancelOrderRequest
    {
        public int OrderId { get; set; }
    }
}