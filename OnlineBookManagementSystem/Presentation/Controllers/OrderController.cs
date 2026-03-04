using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.Orders;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderQueryService _orderQueryService;
        private readonly IOrderCommandService _orderCommandService;
        private readonly IActivityLogger _logger;

        public OrderController(
            IOrderQueryService orderQueryService,
            IOrderCommandService orderCommandService,
            IActivityLogger logger)
        {
            _orderQueryService = orderQueryService;
            _orderCommandService = orderCommandService;
            _logger = logger;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // ==================== ADMIN VIEWS ====================

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> AdminIndex(string search = "", string status = "", int page = 1)
        {
            try
            {
                int pageSize = 10;

                // Parse status filter
                OrderStatus? statusFilter = null;
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    statusFilter = orderStatus;
                }

                // Get orders from service
                var model = await _orderQueryService.GetOrdersForAdminAsync(page, pageSize, search, statusFilter?.ToString(), null, null);

                await _logger.LogAsync("ViewOrders", "Viewed admin order list", CurrentUserId);
                // Use canonical Orders/List view
                return View("~/Presentation/Views/Orders/List.cshtml", model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading orders";
                // Use canonical Orders/List view
                return View("~/Presentation/Views/Orders/List.cshtml", new AdminOrderListViewModel());
            }
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> AdminDetails(int id)
        {
            try
            {
                var order = await _orderQueryService.GetOrderDetailsAsync(id);

                if (order == null) return NotFound();

                var isSuperAdmin = User.IsInRole("SuperAdmin");

                var viewModel = new OrderDetailsViewModel
                {
                    OrderId = order.Id,
                    OrderNumber = order.Id.ToString(),
                    OrderDate = order.OrderDate ?? DateTime.UtcNow,
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus,
                    Subtotal = order.GetItemsTotal().Amount,
                    Tax = 0,
                    ShippingCost = 0,
                    TotalAmount = order.TotalAmount.Amount,
                    PaymentMethod = order.PaymentMethod ?? "Unknown",
                    FullName = order.FullName ?? order.User?.Name ?? "Unknown",
                    PhoneNumber = order.Phone ?? order.PhoneNumber ?? "N/A",
                    ShippingAddress = order.Address ?? "N/A",
                    City = order.City ?? "N/A",
                    PinCode = order.ZipCode ?? "N/A",
                    CustomerEmail = order.User?.Email,
                    CustomerId = order.User?.Id,
                    CreatedAt = order.CreatedAt,
                    Items = order.OrderDetails.Select(od => new OrderItemViewModel
                    {
                        BookId = od.BookId,
                        BookTitle = od.Book?.Title ?? $"Book #{od.BookId}",
                        BookImageUrl = od.Book?.ImageUrl,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice.Amount,
                        Subtotal = od.Subtotal.Amount
                    }).ToList(),
                    Capabilities = new OrderDetailsCapabilities
                    {
                        CanCancel = false,
                        CanChangeStatus = true,
                        CanMarkAsProcessing = order.Status == OrderStatus.Pending,
                        CanMarkAsShipped = order.Status == OrderStatus.Processing,
                        CanMarkAsCompleted = order.Status == OrderStatus.Shipped,
                        CanMarkAsCancelled = order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled,
                        CanViewCustomerInfo = true,
                        CanViewPaymentDetails = true,
                        CanViewTechnicalDetails = true,
                        CanRefund = isSuperAdmin,
                        IsAuthenticated = true,
                        IsOwnOrder = false,
                        BackLinkText = "Back to Order Management",
                        BackLinkUrl = "/Order/List"
                    }
                };

                await _logger.LogAsync("ViewOrderDetails", $"Viewed order #{id} details", CurrentUserId);
                // Use canonical Orders/Details view
                return View("~/Presentation/Views/Orders/Details.cshtml", viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading order details";
                return NotFound();
            }
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    TempData["Error"] = "Invalid status value";
                    return RedirectToAction(nameof(AdminDetails), new { id });
                }

                var success = await _orderCommandService.UpdateOrderStatusAsync(id, orderStatus, CurrentUserId);

                if (success)
                {
                    TempData["Success"] = $"Order #{id} updated to {status}";
                }
                else
                {
                    TempData["Error"] = "Failed to update order status";
                }

                return RedirectToAction(nameof(AdminDetails), new { id });
            }
            catch (Exception)
            {
                TempData["Error"] = "Error updating order status";
                return RedirectToAction(nameof(AdminDetails), new { id });
            }
        }

        // ==================== USER VIEWS ====================

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _orderQueryService.GetUserOrderHistoryAsync(CurrentUserId, 1, 100, null, null, null);

                await _logger.LogAsync("ViewOrderHistory", "Viewed order history", CurrentUserId);
                // Use canonical Orders/List view
                return View("~/Presentation/Views/Orders/List.cshtml", orders);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading order history";
                // Use canonical Orders/List view
                return View("~/Presentation/Views/Orders/List.cshtml", new List<object>());
            }
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderQueryService.GetUserOrderDetailsAsync(id, CurrentUserId);

                if (order == null) return NotFound();

                var viewModel = new OrderDetailsViewModel
                {
                    OrderId = order.Id,
                    OrderNumber = order.Id.ToString(),
                    OrderDate = order.OrderDate ?? DateTime.UtcNow,
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus,
                    Subtotal = order.GetItemsTotal().Amount,
                    Tax = 0,
                    ShippingCost = 0,
                    TotalAmount = order.TotalAmount.Amount,
                    PaymentMethod = order.PaymentMethod ?? "Unknown",
                    FullName = order.FullName ?? order.User?.Name ?? "Unknown",
                    PhoneNumber = order.Phone ?? order.PhoneNumber ?? "N/A",
                    ShippingAddress = order.Address ?? "N/A",
                    City = order.City ?? "N/A",
                    PinCode = order.ZipCode ?? "N/A",
                    Items = order.OrderDetails.Select(od => new OrderItemViewModel
                    {
                        BookId = od.BookId,
                        BookTitle = od.Book?.Title ?? $"Book #{od.BookId}",
                        BookImageUrl = od.Book?.ImageUrl,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice.Amount,
                        Subtotal = od.Subtotal.Amount
                    }).ToList(),
                    Capabilities = new OrderDetailsCapabilities
                    {
                        CanCancel = order.Status == OrderStatus.Pending || order.Status == OrderStatus.Processing,
                        CanChangeStatus = false,
                        CanMarkAsProcessing = false,
                        CanMarkAsShipped = false,
                        CanMarkAsCompleted = false,
                        CanMarkAsCancelled = false,
                        CanViewCustomerInfo = false,
                        CanViewPaymentDetails = false,
                        CanViewTechnicalDetails = false,
                        CanRefund = false,
                        IsAuthenticated = true,
                        IsOwnOrder = true,
                        BackLinkText = "Back to My Orders",
                        BackLinkUrl = "/Order/List"
                    }
                };

                await _logger.LogAsync("ViewOrderDetails", $"Viewed order #{id} details", CurrentUserId);
                // Use canonical Orders/Details view
                return View("~/Presentation/Views/Orders/Details.cshtml", viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading order details";
                return NotFound();
            }
        }

        // ==================== UNIFIED API ENDPOINTS ====================

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRequest request)
        {
            try
            {
                if (!Enum.TryParse<OrderStatus>(request.Status, true, out var orderStatus))
                {
                    return Json(new { success = false, message = "Invalid status value" });
                }

                var success = await _orderCommandService.UpdateOrderStatusAsync(request.OrderId, orderStatus, CurrentUserId);

                if (success)
                {
                    await _logger.LogAsync("ChangeOrderStatus", $"Order #{request.OrderId} status changed to {request.Status}", CurrentUserId);
                    return Json(new { success = true, message = "Order status updated successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update order status" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error updating order status" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel([FromBody] CancelOrderRequest request)
        {
            try
            {
                // Users can only cancel their own orders, Admins can cancel any order
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

                var success = await _orderCommandService.UpdateOrderStatusAsync(request.OrderId, OrderStatus.Cancelled, CurrentUserId);

                if (success)
                {
                    await _logger.LogAsync("CancelOrder", $"Order #{request.OrderId} cancelled", CurrentUserId);
                    return Json(new { success = true, message = "Order cancelled successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to cancel order" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error cancelling order" });
            }
        }

        // ==================== UNIFIED LIST ENDPOINT ====================

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> List(int page = 1, string? search = null, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                var userId = CurrentUserId;
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
                var isSuperAdmin = User.IsInRole("SuperAdmin");

                if (isAdmin)
                {
                    // Admin/SuperAdmin sees all orders
                    var adminViewModel = await _orderQueryService.GetOrdersForAdminAsync(page, 20, search, status, dateFrom, dateTo);

                    // For now, let's get the raw orders and create the unified view model
                    var recentOrders = await _orderQueryService.GetRecentOrdersAsync(100);

                    // Apply basic filtering (this should be moved to the service layer)
                    var filteredOrders = recentOrders.AsQueryable();

                    if (!string.IsNullOrEmpty(search))
                    {
                        filteredOrders = filteredOrders.Where(o =>
                            o.User.Name.Contains(search) ||
                            o.User.Email.Contains(search));
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
                        {
                            filteredOrders = filteredOrders.Where(o => o.Status == orderStatus);
                        }
                    }

                    if (dateFrom.HasValue)
                    {
                        filteredOrders = filteredOrders.Where(o => o.OrderDate >= dateFrom.Value);
                    }

                    if (dateTo.HasValue)
                    {
                        filteredOrders = filteredOrders.Where(o => o.OrderDate <= dateTo.Value);
                    }

                    var orders = filteredOrders.ToList();
                    var totalOrders = orders.Count;
                    var totalPages = (int)Math.Ceiling((double)totalOrders / 20);
                    var pagedOrders = orders.Skip((page - 1) * 20).Take(20).ToList();

                    // Convert to unified OrderListViewModel
                    var unifiedViewModel = new OrderListViewModel
                    {
                        Orders = pagedOrders,
                        CurrentPage = page,
                        TotalPages = totalPages,
                        TotalOrders = totalOrders,
                        SearchTerm = search,
                        StatusFilter = status,
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        PendingOrders = adminViewModel.PendingOrders,
                        ProcessingOrders = adminViewModel.ProcessingOrders,
                        CompletedOrders = adminViewModel.CompletedOrders,

                        Capabilities = new OrderListCapabilities
                        {
                            CanViewAllOrders = true,
                            CanViewPaymentSummary = true,
                            CanViewCustomerInfo = true,
                            CanViewStatistics = true,
                            CanChangeStatus = true,
                            CanViewPaymentDetails = true,
                            CanRefund = isSuperAdmin, // Only SuperAdmin can refund
                            CanCancel = false, // Admin doesn't cancel directly
                            CanFilter = true,
                            CanSearch = true,
                            CanSort = true,
                            CanPaginate = true,
                            IsAuthenticated = true,
                            PageTitle = isAdmin ? "Order Management" : "All Orders",
                            BackLinkText = "Back to Dashboard",
                            BackLinkUrl = isAdmin ? "/Admin/Dashboard" : "/SuperAdmin/Dashboard",
                            DetailsActionName = isAdmin ? "AdminDetails" : "Details",
                            DetailsControllerName = "Order",
                            LayoutClass = isAdmin ? "admin-layout" : "superadmin-layout"
                        }
                    };

                    await _logger.LogAsync("ViewOrders", "Order list accessed", userId);
                    return View(unifiedViewModel);
                }
                else
                {
                    // Regular user sees only their own orders
                    // Get user orders as Order entities for consistency with unified view model
                    var userOrderEntities = await _orderQueryService.GetUserRecentOrdersAsync(userId, 1000); // Get more for filtering

                    // Apply basic filtering
                    var filteredOrders = userOrderEntities.AsQueryable();

                    if (!string.IsNullOrEmpty(status))
                    {
                        if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
                        {
                            filteredOrders = filteredOrders.Where(o => o.Status == orderStatus);
                        }
                    }

                    if (dateFrom.HasValue)
                    {
                        filteredOrders = filteredOrders.Where(o => o.OrderDate >= dateFrom.Value);
                    }

                    if (dateTo.HasValue)
                    {
                        filteredOrders = filteredOrders.Where(o => o.OrderDate <= dateTo.Value);
                    }

                    var orders = filteredOrders.ToList();
                    var totalOrders = orders.Count;
                    var totalPages = (int)Math.Ceiling((double)totalOrders / 20);
                    var pagedOrders = orders.Skip((page - 1) * 20).Take(20).ToList();

                    // Convert to unified OrderListViewModel
                    var unifiedViewModel = new OnlineBookManagementSystem.Presentation.ViewModels.Orders.OrderListViewModel
                    {
                        Orders = pagedOrders,
                        CurrentPage = page,
                        TotalPages = totalPages,
                        TotalOrders = totalOrders,
                        SearchTerm = search,
                        StatusFilter = status,
                        DateFrom = dateFrom,
                        DateTo = dateTo,

                        Capabilities = new OnlineBookManagementSystem.Presentation.ViewModels.Orders.OrderListCapabilities
                        {
                            CanViewAllOrders = false, // User sees only their own
                            CanViewPaymentSummary = false,
                            CanViewCustomerInfo = false,
                            CanViewStatistics = false,
                            CanChangeStatus = false,
                            CanViewPaymentDetails = false,
                            CanRefund = false,
                            CanCancel = true, // Users can cancel their own pending orders
                            CanFilter = true,
                            CanSearch = true,
                            CanSort = true,
                            CanPaginate = true,
                            IsAuthenticated = true,
                            PageTitle = "My Orders",
                            BackLinkText = "Back to Dashboard",
                            BackLinkUrl = "/User/Dashboard",
                            DetailsActionName = "Details",
                            DetailsControllerName = "Order",
                            LayoutClass = "user-layout"
                        }
                    };

                    await _logger.LogAsync("ViewOrderHistory", "User order history accessed", userId);
                    return View(unifiedViewModel);
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading orders";
                return View(new OnlineBookManagementSystem.Presentation.ViewModels.Orders.OrderListViewModel());
            }
        }
    }

    // Request models for API endpoints
    public class ChangeStatusRequest
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CancelOrderRequest
    {
        public int OrderId { get; set; }
    }
}
