using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.Cart;
using OnlineBookManagementSystem.Presentation.ViewModels.User;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookFavoriteService _bookFavoriteService;
        private readonly IBookCommandService _bookCommandService;
        private readonly IUserCommandService _userCommandService;
        private readonly ICartService _cartService;
        private readonly IOrderQueryService _orderQueryService;
        private readonly IOrderCommandService _orderCommandService;
        private readonly IActivityLogger _activityLogger;
        private readonly ICategoryInterface _categoryService;
        private readonly IAuthService _authService;

        public UserController(
            IBookQueryService bookQueryService,
            IBookFavoriteService bookFavoriteService,
            IBookCommandService bookCommandService,
            IUserCommandService userCommandService,
            ICartService cartService,
            IOrderQueryService orderQueryService,
            IOrderCommandService orderCommandService,
            IActivityLogger activityLogger,
            ICategoryInterface categoryService,
            IAuthService authService)
        {
            _bookQueryService = bookQueryService;
            _bookFavoriteService = bookFavoriteService;
            _bookCommandService = bookCommandService;
            _userCommandService = userCommandService;
            _cartService = cartService;
            _orderQueryService = orderQueryService;
            _orderCommandService = orderCommandService;
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

            var books = await _bookQueryService.GetBooksForUserAsync(page, 12, search, categoryId, sortBy, minPrice, maxPrice, userId);

            // Create unified BookListViewModel with user capabilities
            var model = new BookListViewModel
            {
                Books = books.Books,
                CurrentPage = books.CurrentPage,
                TotalPages = books.TotalPages,
                TotalBooks = books.TotalBooks,
                SearchTerm = search,
                CategoryId = categoryId,
                SortBy = sortBy,
                Capabilities = new BookListCapabilities
                {
                    CanCreate = false,
                    CanEdit = false,
                    CanDelete = false,
                    CanSearch = true,
                    CanFilter = true,
                    CanSort = true,
                    CanPaginate = true,
                    CanViewTechnicalInfo = false,
                    CanViewBookDetails = true,
                    CanAddToCart = true,
                    CanFavorite = true,
                    PageTitle = "Browse Books",
                    DetailsControllerName = "Books",
                    DetailsActionName = "Details",
                    IsAuthenticated = true
                }
            };

            // Add categories for filter dropdown
            ViewBag.Categories = await _categoryService.GetCategoriesForDropdownAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            await _activityLogger.LogAsync("BrowseBooks", "User browsed book catalog", userId);

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return PartialView("~/Presentation/Views/Books/BookList.cshtml", model);
            }

            return View("~/Presentation/Views/Books/BookList.cshtml", model);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> BookDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var book = await _bookQueryService.GetBookDetailsForUserAsync(id, userId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction(nameof(UserBookList));
            }

            await _activityLogger.LogAsync("ViewBook", $"Viewed book '{book.Title}'", userId);
            // Use canonical Books/Details view
            return View("~/Presentation/Views/Books/Details.cshtml", book);
        }

        // Alias for BookDetails to handle /User/Details/{id} URLs
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var book = await _bookQueryService.GetBookDetailsForUserAsync(id, userId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction(nameof(UserBookList));
            }

            await _activityLogger.LogAsync("ViewBook", $"Viewed book '{book.Title}'", userId);
            // Use canonical Books/Details view
            return View("~/Presentation/Views/Books/Details.cshtml", book);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Favorite()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var favoriteBooks = await _bookFavoriteService.GetUserFavoriteBooksAsync(userId);
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
                var result = await _bookFavoriteService.ToggleUserFavoriteAsync(request.BookId, userId);
                if (result.Success)
                {
                    await _activityLogger.LogAsync("ToggleFavorite", $"Toggled favorite for book ID {request.BookId}", userId);
                    return Json(new { success = true, message = result.Message, isFavorite = result.IsFavorite });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating favorite" });
            }
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> OrderHistory(int page = 1, string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = await _orderQueryService.GetUserOrderHistoryAsync(userId, page, 10, status, dateFrom, dateTo);

            ViewBag.Status = status;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            await _activityLogger.LogAsync("ViewOrderHistory", "User viewed order history", userId);
            // Use canonical Orders/List view
            return View("~/Presentation/Views/Orders/List.cshtml", viewModel);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var order = await _orderQueryService.GetUserOrderDetailsAsync(id, userId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(OrderHistory));
            }

            await _activityLogger.LogAsync("ViewOrderDetails", $"Viewed order details for order {id}", userId);
            // Use canonical Orders/Details view
            return View("~/Presentation/Views/Orders/Details.cshtml", order);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var profile = await _authService.GetUserProfileAsync(userId);
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
                return View("~/Presentation/Views/User/Profile.cshtml", model);
            }

            try
            {
                // Use proper UserCommandService instead of BookCommandService
                var success = await _userCommandService.UpdateUserProfileAsync(userId, model);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateProfile", "User updated profile information", userId);
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                ModelState.AddModelError("", "Failed to update profile. Please try again.");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating your profile.");
            }

            return View("~/Presentation/Views/User/Profile.cshtml", model);
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
            catch (Exception)
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
            catch (Exception)
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

            var books = await _bookQueryService.SearchBooksAsync(query, page, 12, userId);

            // Create unified BookListViewModel with user capabilities
            var model = new BookListViewModel
            {
                Books = books.Books,
                CurrentPage = books.CurrentPage,
                TotalPages = books.TotalPages,
                TotalBooks = books.TotalBooks,
                SearchTerm = query,
                Capabilities = new BookListCapabilities
                {
                    CanCreate = false,
                    CanEdit = false,
                    CanDelete = false,
                    CanSearch = true,
                    CanFilter = true,
                    CanSort = true,
                    CanPaginate = true,
                    CanViewTechnicalInfo = false,
                    CanViewBookDetails = true,
                    CanAddToCart = true,
                    CanFavorite = true,
                    PageTitle = $"Search Results for '{query}'",
                    DetailsControllerName = "Books",
                    DetailsActionName = "Details",
                    IsAuthenticated = true
                }
            };

            ViewBag.SearchQuery = query;

            await _activityLogger.LogAsync("SearchBooks", $"Searched for '{query}'", userId);
            return View("~/Presentation/Views/Books/BookList.cshtml", model);
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

            var books = await _bookQueryService.GetBooksByCategoryAsync(categoryId, page, 12, userId);

            // Create unified BookListViewModel with user capabilities
            var model = new BookListViewModel
            {
                Books = books.Books,
                CurrentPage = books.CurrentPage,
                TotalPages = books.TotalPages,
                TotalBooks = books.TotalBooks,
                CategoryId = categoryId,
                Capabilities = new BookListCapabilities
                {
                    CanCreate = false,
                    CanEdit = false,
                    CanDelete = false,
                    CanSearch = true,
                    CanFilter = true,
                    CanSort = true,
                    CanPaginate = true,
                    CanViewTechnicalInfo = false,
                    CanViewBookDetails = true,
                    CanAddToCart = true,
                    CanFavorite = true,
                    PageTitle = $"{category.Name} Books",
                    DetailsControllerName = "Books",
                    DetailsActionName = "Details",
                    IsAuthenticated = true
                }
            };

            ViewBag.CategoryName = category.Name;

            await _activityLogger.LogAsync("BrowseCategory", $"Browsed category '{category.Name}'", userId);
            return View("~/Presentation/Views/Books/BookList.cshtml", model);
        }

        [HttpGet]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> GetRecommendations()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var recommendations = await _bookQueryService.GetPersonalizedRecommendationsAsync(userId, 6);
                return Json(new { success = true, books = recommendations });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to load recommendations" });
            }
        }

        [HttpGet]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> GetNewArrivals()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var newArrivals = await _bookQueryService.GetNewArrivalsAsync(3, userId);
                return Json(new { success = true, books = newArrivals });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to load new arrivals" });
            }
        }


        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> UserCart()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var cartItems = await _cartService.GetUserCartAsync(userId);
            var summary = await _cartService.GetCartSummaryAsync(userId);

            // Create unified CartViewModel with user capabilities
            var model = new UnifiedCartViewModel
            {
                CartItems = cartItems.Select(item => new CartItemViewModel
                {
                    BookId = item.BookId,
                    BookTitle = item.BookTitle,
                    BookAuthor = item.BookAuthor,
                    BookPrice = item.BookPrice,
                    Quantity = item.Quantity,
                    Subtotal = item.Subtotal,
                    BookImage = item.BookImage,
                    CategoryName = item.CategoryName,
                    IsAvailable = item.IsAvailable
                }).ToList(),
                Summary = summary,
                UserId = userId,
                LastUpdated = DateTime.Now,
                Capabilities = new CartCapabilities
                {
                    CanViewCart = true,
                    CanViewCartDetails = true,
                    CanModifyCart = true,
                    CanUpdateQuantity = true,
                    CanRemoveItems = true,
                    CanCheckout = true,
                    CanClearCart = true,
                    IsReadOnly = false,
                    IsAuthenticated = true,
                    PageTitle = "My Cart",
                    BackLinkText = "Continue Shopping",
                    BackLinkUrl = "/Books/BookList",
                    CheckoutButtonText = "Proceed to Checkout"
                }
            };

            return View("~/Presentation/Views/Cart/CartView.cshtml", model);
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

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

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

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
