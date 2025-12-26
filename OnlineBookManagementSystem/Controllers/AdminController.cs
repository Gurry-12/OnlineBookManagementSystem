using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly IBookService _bookService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IActivityLogger _activityLogger;
        private readonly IUsersService _userService;
        private readonly ICategoryInterface _categoryService;

        public AdminController(IBookService bookService,
            ICartService cartService,
            IOrderService orderService,
            IActivityLogger activityLogger,
            IUsersService userService,
            ICategoryInterface categoryService
            )
        {
            _bookService = bookService;
            _cartService = cartService;
            _orderService = orderService;
            _activityLogger = activityLogger;
            _userService = userService;
            _categoryService = categoryService;
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

            // Admin sees filtered logs (not system-level logs)
            //var viewModel = await _activityLogger.GetActivityLogsAsync(page, 25, search, action, null, dateFrom, dateTo, excludeSystemLogs: true);
            var viewModel = await _activityLogger.GetAllLogsAsync();
            ViewBag.Search = search;
            ViewBag.Action = action;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View(viewModel);
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> Books(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var model = await _bookService.GetPaginatedBooksAsync(page, 12, search, categoryId, sortBy);

            // Add categories for filter dropdown
            ViewBag.Categories = await _categoryService.GetCategoriesForDropdownAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;

            await _activityLogger.LogAsync("ViewBooks", "Admin books page accessed", userId);
            return View(model);
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> CreateBook()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _bookService.GetCreateBookViewModelAsync();
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> CreateBook(BookFormViewModel model, IFormFile? imageFile)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryService.GetCategoriesForDropdownAsync();
                return View(model);
            }

            try
            {
                var success = await _bookService.AddBookAsync(model.Book!, imageFile);
                if (success)
                {
                    await _activityLogger.LogAsync("CreateBook", $"Book '{model.Book!.Title}' created", userId);
                    TempData["SuccessMessage"] = "Book created successfully!";
                    return RedirectToAction(nameof(Books));
                }

                ModelState.AddModelError("", "Failed to create book. Please try again.");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the book.");
            }

            model.Categories = await _categoryService.GetCategoriesForDropdownAsync();
            return View(model);
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> EditBook(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _bookService.GetEditBookViewModelAsync(id);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction(nameof(Books));
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> EditBook(int id, BookFormViewModel model, IFormFile? imageFile)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            if (id != model.Book?.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryService.GetCategoriesForDropdownAsync();
                return View(model);
            }

            try
            {
                var success = await _bookService.UpdateBookAsync(model.Book!, imageFile);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateBook", $"Book '{model.Book!.Title}' updated", userId);
                    TempData["SuccessMessage"] = "Book updated successfully!";
                    return RedirectToAction(nameof(Books));
                }

                ModelState.AddModelError("", "Failed to update book. Please try again.");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the book.");
            }

            model.Categories = await _categoryService.GetCategoriesForDropdownAsync();
            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _bookService.SoftDeleteBookAsync(id, userId);
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

            var viewModel = await _orderService.GetOrdersForAdminAsync(page, 20, search, status, dateFrom, dateTo);

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
                var success = await _orderService.UpdateOrderStatusAsync(orderId, status, userId);
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
                    "monthly" => (object)await _bookService.GetMonthlyBookUploadsAsync(),
                    "category" => (object)await _bookService.GetBooksByCategoryAsync(),
                    "author" => (object)await _bookService.GetBooksByAuthorAsync(),
                    "favorites" => (object)await _bookService.GetFavoriteStatsAsync(),
                    _ => (object?)null
                };

                return Json(new { success = true, data });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to load chart data" });
            }
        }

        private async Task<AdminDashboardViewModel> GetAdminDashboardDataAsync(int userId)
        {
            var totalBooks = await _bookService.GetTotalBooksCountAsync();
            var totalOrders = await _orderService.GetTotalOrdersCountAsync();
            var totalUsers = await _userService.GetTotalUsersCountAsync();
            var totalCategories = await _categoryService.GetTotalCategoriesCountAsync();
            var recentActivities = await _activityLogger.GetRecentActivitiesAsync(10, excludeSystemLogs: true);
            var monthlyStats = await _bookService.GetMonthlyStatsAsync();

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

        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }

    // Request models
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
