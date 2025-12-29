using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IBookService _bookService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IActivityLogger _activityLogger;
        private readonly ICategoryInterface _categoryService;
        private readonly IAuthService _authService;

        public UserController(
            IBookService bookService,
            ICartService cartService,
            IOrderService orderService,
            IActivityLogger activityLogger,
            ICategoryInterface categoryService,
            IAuthService authService)
        {
            _bookService = bookService;
            _cartService = cartService;
            _orderService = orderService;
            _activityLogger = activityLogger;
            _categoryService = categoryService;
            _authService = authService;
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await GetUserDashboardDataAsync(userId);
            await _activityLogger.LogAsync("Dashboard", "User dashboard accessed", userId);
            
            return View(viewModel);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> UserBookList(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _bookService.GetBooksForUserAsync(page, 12, search, categoryId, sortBy, minPrice, maxPrice);
            
            // Add categories for filter dropdown
            ViewBag.Categories = await _categoryService.GetCategoriesForDropdownAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            
            await _activityLogger.LogAsync("BrowseBooks", "User browsed book catalog", userId);
            return View(viewModel);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> BookDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var book = await _bookService.GetBookDetailsForUserAsync(id, userId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction(nameof(UserBookList));
            }

            await _activityLogger.LogAsync("ViewBook", $"Viewed book '{book.Title}'", userId);
            return View(book);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Favorite()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var favoriteBooks = await _bookService.GetUserFavoriteBooksAsync(userId);
            await _activityLogger.LogAsync("ViewFavorites", "User viewed favorite books", userId);
            
            return View(favoriteBooks);
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _bookService.ToggleUserFavoriteAsync(request.BookId, userId);
                if (result.Success)
                {
                    await _activityLogger.LogAsync("ToggleFavorite", $"Toggled favorite for book ID {request.BookId}", userId);
                    return Json(new { success = true, message = result.Message, isFavorite = result.IsFavorite });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating favorite" });
            }
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> OrderHistory(int page = 1, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _orderService.GetUserOrderHistoryAsync(userId, page, 10, status, dateFrom, dateTo);
            
            ViewBag.Status = status;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            
            await _activityLogger.LogAsync("ViewOrderHistory", "User viewed order history", userId);
            return View(viewModel);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var order = await _orderService.GetUserOrderDetailsAsync(id, userId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(OrderHistory));
            }

            await _activityLogger.LogAsync("ViewOrderDetails", $"Viewed order details for order {id}", userId);
            return View(order);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var profile = await _bookService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                TempData["ErrorMessage"] = "Profile not found.";
                return RedirectToAction("Login", "Auth");
            }

            return View(profile);
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            try
            {
                var success = await _bookService.UpdateUserProfileAsync(userId, model);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateProfile", "User updated profile information", userId);
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }
                
                ModelState.AddModelError("", "Failed to update profile. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating your profile.");
            }

            return View("Profile", model);
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _cartService.AddToCartAsync(userId, request.BookId, request.Quantity);
                if (result.Success)
                {
                    await _activityLogger.LogAsync("AddToCart", $"Added book ID {request.BookId} to cart", userId);
                    return Json(new { success = true, message = result.Message, cartCount = result.CartCount });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding to cart" });
            }
        }

        [HttpGet]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { count = 0 });

            try
            {
                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                return Json(new { count = 0 });
            }
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> SearchBooks(string query, int page = 1)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(UserBookList));
            }

            var viewModel = await _bookService.SearchBooksAsync(query, page, 12);
            ViewBag.SearchQuery = query;
            
            await _activityLogger.LogAsync("SearchBooks", $"Searched for '{query}'", userId);
            return View("UserBookList", viewModel);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> BooksByCategory(int categoryId, int page = 1)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(UserBookList));
            }

            var viewModel = await _bookService.GetBooksByCategoryAsync(categoryId, page, 12);
            ViewBag.CategoryName = category.Name;
            
            await _activityLogger.LogAsync("BrowseCategory", $"Browsed category '{category.Name}'", userId);
            return View("UserBookList", viewModel);
        }

        [HttpGet]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> GetRecommendations()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var recommendations = await _bookService.GetPersonalizedRecommendationsAsync(userId, 6);
                return Json(new { success = true, books = recommendations });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load recommendations" });
            }
        }

        private async Task<UserDashboardViewModel> GetUserDashboardDataAsync(int userId)
        {
            var totalBooks = await _bookService.GetTotalBooksCountAsync();
            var favoritesCount = await _bookService.GetUserFavoritesCountAsync(userId);
            var ordersCount = await _orderService.GetUserOrdersCountAsync(userId);
            var cartItemsCount = await _cartService.GetCartItemCountAsync(userId);
            var totalSpent = await _orderService.GetUserTotalSpentAsync(userId);
            var featuredBooks = await _bookService.GetFeaturedBooksAsync(4);
            var recentOrders = await _orderService.GetUserRecentOrdersAsync(userId, 5);
            var categories = await _categoryService.GetCategoriesWithCountAsync();
            var recommendedBooks = await _bookService.GetPersonalizedRecommendationsAsync(userId, 6);
            var newArrivals = await _bookService.GetNewArrivalsAsync(8);

            return new UserDashboardViewModel
            {
                TotalBooks = totalBooks,
                FavoritesCount = favoritesCount,
                OrdersCount = ordersCount,
                CartItemsCount = cartItemsCount,
                TotalSpent = totalSpent,
                FeaturedBooks = featuredBooks,
                RecentOrders = recentOrders,
                Categories = categories,
                RecommendedBooks = recommendedBooks,
                NewArrivals = newArrivals
            };
        }

        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _orderService.CancelOrderAsync(request.OrderId, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("CancelOrder", $"Order {request.OrderId} cancelled by user", userId);
                    return Json(new { success = true, message = "Order cancelled successfully" });
                }
                return Json(new { success = false, message = "Unable to cancel order or order not found" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while cancelling the order" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (request.NewPassword != request.ConfirmPassword)
            {
                return Json(new { success = false, message = "New passwords do not match" });
            }

            try
            {
                var success = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                if (success)
                {
                    await _activityLogger.LogAsync("ChangePassword", "User password changed", userId);
                    return Json(new { success = true, message = "Password changed successfully" });
                }
                return Json(new { success = false, message = "Current password is incorrect" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while changing password" });
            }
        }
    }

    // Request models
    public class ToggleFavoriteRequest
    {
        public int BookId { get; set; }
    }

    public class AddToCartRequest
    {
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class CancelOrderRequest
    {
        public int OrderId { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
