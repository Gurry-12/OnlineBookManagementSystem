using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.User;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers.User
{
    /// <summary>
    /// Handles user dashboard functionality following SRP.
    /// Responsible only for dashboard data aggregation and display.
    /// </summary>
    [Authorize(Policy = "UserOrHigher")]
    public class UserDashboardController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookFavoriteService _bookFavoriteService;
        private readonly ICartService _cartService;
        private readonly IOrderQueryService _orderQueryService;
        private readonly ICategoryInterface _categoryService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<UserDashboardController> _logger;

        public UserDashboardController(
            IBookQueryService bookQueryService,
            IBookFavoriteService bookFavoriteService,
            ICartService cartService,
            IOrderQueryService orderQueryService,
            ICategoryInterface categoryService,
            IActivityLogger activityLogger,
            ILogger<UserDashboardController> logger)
        {
            _bookQueryService = bookQueryService;
            _bookFavoriteService = bookFavoriteService;
            _cartService = cartService;
            _orderQueryService = orderQueryService;
            _categoryService = categoryService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var viewModel = await GetUserDashboardDataAsync(userId);
                await _activityLogger.LogAsync("Dashboard", "User dashboard accessed", userId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user dashboard for user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load dashboard data.";
                return View(new UserDashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendations()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var recommendations = await _bookQueryService.GetPersonalizedRecommendationsAsync(userId, 6);
                return Json(new { success = true, books = recommendations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recommendations for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load recommendations" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNewArrivals()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var newArrivals = await _bookQueryService.GetNewArrivalsAsync(3, userId);
                return Json(new { success = true, books = newArrivals });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading new arrivals for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load new arrivals" });
            }
        }

        [HttpGet]
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
                _logger.LogError(ex, "Error loading cart count for user {UserId}", userId);
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuickStats()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var stats = new
                {
                    favoritesCount = await _bookFavoriteService.GetUserFavoritesCountAsync(userId),
                    ordersCount = await _orderQueryService.GetUserOrdersCountAsync(userId),
                    cartItemsCount = await _cartService.GetCartItemCountAsync(userId),
                    totalSpent = await _orderQueryService.GetUserTotalSpentAsync(userId)
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quick stats for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load statistics" });
            }
        }

        private async Task<UserDashboardViewModel> GetUserDashboardDataAsync(int userId)
        {
            var totalBooks = await _bookQueryService.GetTotalBooksCountAsync();
            var favoritesCount = await _bookFavoriteService.GetUserFavoritesCountAsync(userId);
            var ordersCount = await _orderQueryService.GetUserOrdersCountAsync(userId);
            var cartItemsCount = await _cartService.GetCartItemCountAsync(userId);
            var totalSpent = await _orderQueryService.GetUserTotalSpentAsync(userId);
            var featuredBooks = await _bookQueryService.GetFeaturedBooksAsync(4);
            var recentOrders = await _orderQueryService.GetUserRecentOrdersAsync(userId, 5);
            var categories = await _categoryService.GetCategoriesWithCountAsync();
            var recommendedBooks = await _bookQueryService.GetPersonalizedRecommendationsAsync(userId, 6);
            var newArrivals = await _bookQueryService.GetNewArrivalsAsync(3);

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

    }
}