using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers.User
{
    /// <summary>
    /// Handles user book browsing functionality following SRP.
    /// Responsible only for book discovery, search, and viewing.
    /// </summary>
    [Authorize(Policy = "UserOrHigher")]
    public class UserBookBrowsingController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly ICategoryInterface _categoryService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<UserBookBrowsingController> _logger;

        public UserBookBrowsingController(
            IBookQueryService bookQueryService,
            ICategoryInterface categoryService,
            IActivityLogger activityLogger,
            ILogger<UserBookBrowsingController> logger)
        {
            _bookQueryService = bookQueryService;
            _categoryService = categoryService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> UserBookList(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var viewModel = await _bookQueryService.GetBooksForUserAsync(page, 12, search, categoryId, sortBy, minPrice, maxPrice, userId);

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
                    return PartialView("_UserBooksGrid", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book list for user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load books.";
                return View();
            }
        }

        public async Task<IActionResult> BookDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var book = await _bookQueryService.GetBookDetailsForUserAsync(id, userId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Book not found.";
                    return RedirectToAction(nameof(UserBookList));
                }

                await _activityLogger.LogAsync("ViewBook", $"Viewed book '{book.Title}'", userId);
                return View(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book details for book {BookId} and user {UserId}", id, userId);
                TempData["ErrorMessage"] = "Failed to load book details.";
                return RedirectToAction(nameof(UserBookList));
            }
        }

        // Alias for BookDetails to handle /User/Details/{id} URLs
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var book = await _bookQueryService.GetBookDetailsForUserAsync(id, userId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Book not found.";
                    return RedirectToAction(nameof(UserBookList));
                }

                await _activityLogger.LogAsync("ViewBook", $"Viewed book '{book.Title}'", userId);
                return View("BookDetails", book); // Use the BookDetails view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book details for book {BookId} and user {UserId}", id, userId);
                TempData["ErrorMessage"] = "Failed to load book details.";
                return RedirectToAction(nameof(UserBookList));
            }
        }

        public async Task<IActionResult> SearchBooks(string query, int page = 1)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(UserBookList));
            }

            try
            {
                var viewModel = await _bookQueryService.SearchBooksAsync(query, page, 12, userId);
                ViewBag.SearchQuery = query;

                await _activityLogger.LogAsync("SearchBooks", $"Searched for '{query}'", userId);
                return View("UserBookList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books with query '{Query}' for user {UserId}", query, userId);
                TempData["ErrorMessage"] = "Failed to search books.";
                return RedirectToAction(nameof(UserBookList));
            }
        }

        public async Task<IActionResult> BooksByCategory(int categoryId, int page = 1)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction(nameof(UserBookList));
                }

                var viewModel = await _bookQueryService.GetBooksByCategoryAsync(categoryId, page, 12, userId);
                ViewBag.CategoryName = category.Name;

                await _activityLogger.LogAsync("BrowseCategory", $"Browsed category '{category.Name}'", userId);
                return View("UserBookList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books by category {CategoryId} for user {UserId}", categoryId, userId);
                TempData["ErrorMessage"] = "Failed to load books for this category.";
                return RedirectToAction(nameof(UserBookList));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFeaturedBooks(int count = 6)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var featuredBooks = await _bookQueryService.GetFeaturedBooksAsync(count);
                return Json(new { success = true, books = featuredBooks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured books for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load featured books" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBooksByPriceRange(decimal minPrice, decimal maxPrice, int page = 1)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var books = await _bookQueryService.GetBooksForUserAsync(page, 12, null, null, null, minPrice, maxPrice, userId);
                return Json(new { success = true, data = books });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books by price range for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load books" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookSuggestions(string query)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { success = true, suggestions = new List<object>() });
            }

            try
            {
                var suggestions = await _bookQueryService.GetBookSuggestionsAsync(query, 5);
                return Json(new { success = true, suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book suggestions for query '{Query}' and user {UserId}", query, userId);
                return Json(new { success = false, message = "Failed to load suggestions" });
            }
        }

    }
}