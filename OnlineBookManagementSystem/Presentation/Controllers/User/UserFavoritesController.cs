using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers.User
{
    /// <summary>
    /// Handles user favorites functionality following SRP.
    /// Responsible only for favorite book management.
    /// </summary>
    [Authorize(Policy = "UserOrHigher")]
    public class UserFavoritesController : BaseController
    {
        private readonly IBookFavoriteService _bookFavoriteService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<UserFavoritesController> _logger;

        public UserFavoritesController(
            IBookFavoriteService bookFavoriteService,
            IActivityLogger activityLogger,
            ILogger<UserFavoritesController> logger)
        {
            _bookFavoriteService = bookFavoriteService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> Favorite()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var favoriteBooks = await _bookFavoriteService.GetUserFavoriteBooksAsync(userId);
                await _activityLogger.LogAsync("ViewFavorites", "User viewed favorite books", userId);
                return View(favoriteBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading favorites for user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load favorite books.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (request.BookId <= 0)
            {
                return Json(new { success = false, message = "Invalid book ID" });
            }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite for book {BookId} and user {UserId}", request.BookId, userId);
                return Json(new { success = false, message = "An error occurred while updating favorite" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int bookId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (bookId <= 0)
            {
                return Json(new { success = false, message = "Invalid book ID" });
            }

            try
            {
                var success = await _bookFavoriteService.AddToFavoritesAsync(userId, bookId);
                if (success)
                {
                    await _activityLogger.LogAsync("AddToFavorites", $"Added book ID {bookId} to favorites", userId);
                    return Json(new { success = true, message = "Book added to favorites" });
                }
                return Json(new { success = false, message = "Failed to add book to favorites" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book {BookId} to favorites for user {UserId}", bookId, userId);
                return Json(new { success = false, message = "An error occurred while adding to favorites" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites(int bookId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (bookId <= 0)
            {
                return Json(new { success = false, message = "Invalid book ID" });
            }

            try
            {
                var success = await _bookFavoriteService.RemoveFromFavoritesAsync(userId, bookId);
                if (success)
                {
                    await _activityLogger.LogAsync("RemoveFromFavorites", $"Removed book ID {bookId} from favorites", userId);
                    return Json(new { success = true, message = "Book removed from favorites" });
                }
                return Json(new { success = false, message = "Failed to remove book from favorites" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing book {BookId} from favorites for user {UserId}", bookId, userId);
                return Json(new { success = false, message = "An error occurred while removing from favorites" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFavoriteCount()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { count = 0 });

            try
            {
                var count = await _bookFavoriteService.GetUserFavoritesCountAsync(userId);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading favorite count for user {UserId}", userId);
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> IsFavorite(int bookId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { isFavorite = false });

            try
            {
                var isFavorite = await _bookFavoriteService.IsBookFavoriteAsync(userId, bookId);
                return Json(new { isFavorite });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if book {BookId} is favorite for user {UserId}", bookId, userId);
                return Json(new { isFavorite = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFavoriteBooks(int page = 1, int pageSize = 12)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var favoriteBooks = await _bookFavoriteService.GetUserFavoriteBooksPagedAsync(userId, page, pageSize);
                return Json(new { success = true, data = favoriteBooks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paged favorites for user {UserId}", userId);
                return Json(new { success = false, message = "Failed to load favorite books" });
            }
        }
    }

    // Request models
    public class ToggleFavoriteRequest
    {
        public int BookId { get; set; }
    }
}