using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Presentation.ViewModels.Activity;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookCommandService _bookCommandService;
        private readonly IBookAnalyticsService _bookAnalyticsService;
        private readonly ICartService _cartService;
        private readonly IOrderQueryService _orderQueryService;
        private readonly IOrderCommandService _orderCommandService;
        private readonly IActivityLogger _activityLogger;
        private readonly IUsersService _userService;
        private readonly ICategoryInterface _categoryService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService,
            IBookAnalyticsService bookAnalyticsService,
            ICartService cartService,
            IOrderQueryService orderQueryService,
            IOrderCommandService orderCommandService,
            IActivityLogger activityLogger,
            IUsersService userService,
            ICategoryInterface categoryService,
            ILogger<AdminController> logger
            )
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
            _bookAnalyticsService = bookAnalyticsService;
            _cartService = cartService;
            _orderQueryService = orderQueryService;
            _orderCommandService = orderCommandService;
            _activityLogger = activityLogger;
            _userService = userService;
            _categoryService = categoryService;
            _logger = logger;
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await GetAdminDashboardDataAsync(userId);
            await _activityLogger.LogAsync("Dashboard", "Admin dashboard accessed", userId);

            return View(viewModel);
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> ActivityLogs(int page = 1, string? search = null, string? action = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            // Default to today's logs if no filters are applied
            List<ActivityLogViewModel> viewModel;

            if (string.IsNullOrEmpty(search) && string.IsNullOrEmpty(action) && !dateFrom.HasValue && !dateTo.HasValue)
            {
                // Show today's logs by default
                viewModel = await _activityLogger.GetTodayLogsAsync();
                ViewBag.ShowingToday = true;
            }
            else
            {
                // Show filtered logs
                viewModel = await _activityLogger.GetFilteredLogsAsync(dateFrom, dateTo, search, action);
                ViewBag.ShowingToday = false;
            }

            ViewBag.Search = search;
            ViewBag.Action = action;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View(viewModel);
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> Books(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null, bool? inStock = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var model = await _bookQueryService.GetPaginatedBooksAsync(page, 12, search, categoryId, sortBy, inStock: inStock);

            // Add categories for filter dropdown
            ViewBag.Categories = await _categoryService.GetCategoriesForDropdownAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.InStock = inStock;

            await _activityLogger.LogAsync("ViewBooks", "Admin books page accessed", userId);

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return PartialView("_BooksGrid", model);
            }

            return View(model);
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> CreateBook()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _bookQueryService.GetCreateBookViewModelAsync();

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_BookForm", viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> CreateBook(BookFormViewModel model, IFormFile? imageFile)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return HandleUnauthorized();

            var validationResult = await ValidateBookModel(model);
            if (!validationResult.IsValid)
                return validationResult.Result;

            try
            {
                var success = await _bookCommandService.AddBookAsync(model.Book!, imageFile);
                if (success)
                {
                    await _activityLogger.LogAsync("CreateBook", $"Book '{model.Book!.Title}' created", userId);
                    return HandleSuccess("Book created successfully!", nameof(Books));
                }

                return HandleError("Failed to create book. Please try again.", model);
            }
            catch (Exception)
            {
                return HandleError("An error occurred while creating the book.", model);
            }
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> EditBook(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _bookQueryService.GetEditBookViewModelAsync(id);
            if (viewModel == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Book not found." });

                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction(nameof(Books));
            }

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_BookForm", viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> EditBook(int id, BookFormViewModel model, IFormFile? imageFile)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return HandleUnauthorized();

            if (id != model?.Book?.Id)
                return HandleError("Invalid book ID", null);

            var validationResult = await ValidateBookModel(model);
            if (!validationResult.IsValid)
                return validationResult.Result;

            try
            {
                var success = await _bookCommandService.UpdateBookAsync(model.Book!, imageFile);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateBook", $"Book '{model.Book!.Title}' updated", userId);
                    return HandleSuccess("Book updated successfully!", nameof(Books));
                }

                return HandleError("Failed to update book. Please try again.", model);
            }
            catch (Exception)
            {
                return HandleError("An error occurred while updating the book.", model);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _bookCommandService.SoftDeleteBookAsync(id, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("DeleteBook", $"Book with ID {id} deleted", userId);
                    return Json(new { success = true, message = "Book deleted successfully" });
                }
                return Json(new { success = false, message = "Book not found or already deleted" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while deleting the book" });
            }
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> UserList(int page = 1, string? search = null, string? role = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _userService.GetUsersForAdminAsync(page, 20, search, role);

            ViewBag.Search = search;
            ViewBag.Role = role;

            await _activityLogger.LogAsync("ViewUsers", "Admin user list accessed", userId);
            return View(viewModel);
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> OrderManagement(int page = 1, string? search = null, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _orderQueryService.GetOrdersForAdminAsync(page, 20, search, status, dateFrom, dateTo);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            await _activityLogger.LogAsync("ViewOrders", "Admin order management accessed", userId);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                bool success = await _orderCommandService.UpdateOrderStatusAsync(orderId, Enum.Parse<OrderStatus>(status), userId);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateOrderStatus", $"Order {orderId} status changed to {status}", userId);
                    return Json(new { success = true, message = "Order status updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update order status" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating order status" });
            }
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> CategoryManagement()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var categories = await _categoryService.GetAllCategoriesAsync();
            await _activityLogger.LogAsync("ViewCategories", "Admin category management accessed", userId);

            return View(categories);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _categoryService.CreateCategoryAsync(request.Name, request.Description, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("CreateCategory", $"Category '{request.Name}' created", userId);
                    return Json(new { success = true, message = "Category created successfully" });
                }
                return Json(new { success = false, message = "Failed to create category" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while creating category" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _categoryService.UpdateCategoryAsync(request.Id, request.Name, request.Description, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateCategory", $"Category '{request.Name}' updated", userId);
                    return Json(new { success = true, message = "Category updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update category" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating category" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("DeleteCategory", $"Category with ID {id} deleted", userId);
                    return Json(new { success = true, message = "Category deleted successfully" });
                }
                return Json(new { success = false, message = "Failed to delete category or category not found" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while deleting category" });
            }
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var book = await _bookQueryService.GetBookByIdAsync(id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction(nameof(Books));
            }

            await _activityLogger.LogAsync("ViewBookDetails", $"Admin viewed details for book '{book.Title}'", userId);
            return View(book);
        }

        // API endpoint for dashboard charts
        [HttpGet]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> GetChartData(string chartType)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var data = chartType switch
                {
                    "monthly" => (object)await _bookAnalyticsService.GetMonthlyBookUploadsAsync(),
                    "category" => (object)await _bookAnalyticsService.GetBooksByCategoryAsync(),
                    "author" => (object)await _bookAnalyticsService.GetBooksByAuthorAsync(),
                    "favorites" => (object)await _bookAnalyticsService.GetFavoriteStatsAsync(),
                    "revenue" => (object)await _orderQueryService.GetMonthlyRevenueAsync(),
                    "orderStatus" => (object)await _orderQueryService.GetOrderStatusDistributionAsync(),
                    _ => (object?)null
                };

                if (data == null)
                {
                    return Json(new { success = false, message = "Invalid chart type" });
                }

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load chart data", error = ex.Message });
            }
        }

        private async Task<AdminDashboardViewModel> GetAdminDashboardDataAsync(int userId)
        {
            var totalBooks = await _bookQueryService.GetTotalBooksCountAsync();
            var totalOrders = await _orderQueryService.GetTotalOrdersCountAsync();
            var totalUsers = await _userService.GetTotalUsersCountAsync();
            var totalCategories = await _categoryService.GetTotalCategoriesCountAsync();

            // Get today's activities only for dashboard
            var todayActivities = await _activityLogger.GetTodayLogsAsync();
            var recentActivities = todayActivities.Take(8).Select(log => new ActivityLog
            {
                Id = 0, // Not needed for display
                Action = log.Action,
                Message = log.Description,
                Timestamp = log.Timestamp,
                UserId = null, // Will be handled in view
                User = null // Will show as System or get from UserName
            }).ToList();

            var monthlyStats = await _bookAnalyticsService.GetMonthlyStatsAsync();

            return new AdminDashboardViewModel
            {
                TotalBooks = totalBooks,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                RecentActivity = recentActivities,
                MonthlyBookUploads = monthlyStats.MonthlyUploads,
                BooksByCategory = monthlyStats.CategoryDistribution,
                BooksByAuthor = monthlyStats.AuthorDistribution,
                FavoriteStats = monthlyStats.FavoriteStats
            };
        }

        private IActionResult HandleUnauthorized()
        {
            return IsAjaxRequest()
                ? Json(new { success = false, message = "Unauthorized" })
                : RedirectToAction("Login", "Auth");
        }

        private async Task<(bool IsValid, IActionResult Result)> ValidateBookModel(BookFormViewModel? model)
        {
            if (model?.Book == null)
            {
                var errorMessage = "Invalid book data provided.";
                return (false, IsAjaxRequest()
                    ? Json(new { success = false, message = errorMessage })
                    : BadRequest(errorMessage));
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesForModel(model);

                if (IsAjaxRequest())
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );
                    return (false, Json(new { success = false, message = "Validation failed", errors }));
                }

                return (false, View(model));
            }

            return (true, null!);
        }

        private async Task LoadCategoriesForModel(BookFormViewModel model)
        {
            try
            {
                model.Categories = await _categoryService.GetCategoriesForDropdownAsync() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories for book form");
                model.Categories = new List<SelectListItem>();
                ModelState.AddModelError("", "Unable to load categories. Please try again.");
            }
        }

        private IActionResult HandleSuccess(string message, string redirectAction)
        {
            return IsAjaxRequest()
                ? Json(new { success = true, message })
                : (TempData["SuccessMessage"] = message, RedirectToAction(redirectAction)).Item2;
        }

        private IActionResult HandleError(string message, BookFormViewModel? model)
        {
            if (IsAjaxRequest())
                return Json(new { success = false, message });

            if (model != null)
            {
                ModelState.AddModelError("", message);
                LoadCategoriesForModel(model).Wait();
                return View(model);
            }

            return BadRequest(message);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

    }

    // Request models
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateCategoryRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
