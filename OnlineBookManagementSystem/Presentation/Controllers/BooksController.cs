using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;
using System.Security.Claims;
using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Reviews;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Application.Mappings;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]  // Global auth; per-action policies
    public class BooksController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookCommandService _bookCommandService;
        private readonly IBookAnalyticsService _bookAnalyticsService;
        private readonly IBookFavoriteService _bookFavoriteService;
        private readonly IActivityLogger _activityLoggerService;
        private readonly IReviewService _reviewService;

        public BooksController(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService,
            IBookAnalyticsService bookAnalyticsService,
            IBookFavoriteService bookFavoriteService,
            IActivityLogger activityLogger,
            IReviewService reviewService)
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
            _bookAnalyticsService = bookAnalyticsService;
            _bookFavoriteService = bookFavoriteService;
            _activityLoggerService = activityLogger;
            _reviewService = reviewService;
        }

        //[Authorize(Policy = "AdminOrHigher")]
        //public async Task<IActionResult> AdminIndex()
        //{
        //    var userId = GetUserIdFromClaims();  // Helper below
        //    if (userId == 0) return RedirectToAction("Login", "Auth");

        //    var adminInfo = _bookQueryService.GetQuickStats(userId);
        //    return View("Admin/AdminIndex", adminInfo);
        //}

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<IActionResult> GetAdminData()
        {
            var data = await _bookQueryService.GetAllBooksAsync();
            return Json(new { data, success = true });
        }

        [Authorize(Policy = "UserOrHigher")]
        public IActionResult UserIndex()
        {
            return View("User/UserIndex");
        }

        [Authorize(Policy = "UserOrHigher")]
        [HttpGet]
        public async Task<IActionResult> GetBooks(string? search = null, int? categoryId = null)
        {
            var userId = GetUserIdFromClaims();
            var books = await _bookQueryService.GetPaginatedBooksAsync(1, 12, search, categoryId, "title");
            var favorites = await _bookQueryService.GetFavoriteBooksAsync(userId);  // For UI highlight
            return Ok(new { data = books.Books, favorites = favorites.Select(f => f.Id).ToList(), success = true });
        }

        [HttpGet]
        [AllowAnonymous]  // Public browse for Guests
        public async Task<IActionResult> PublicList(string? search = null, int? categoryId = null)
        {
            var books = await _bookQueryService.GetPaginatedBooksAsync(1, 20, search, categoryId, "title");
            return View("Public/BookList", books);
        }

        // CRUD Actions (AdminOrHigher)
        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await _bookQueryService.GetCreateBookViewModelAsync();
            return View("Admin/CreateBook", vm);
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _bookQueryService.GetCategoriesAsync();  // Repopulate
                return View("Admin/CreateBook", model);
            }

            var book = new Book
            {
                Title = model.Book.Title,
                Author = model.Book.Author,
                ISBN = model.Book.ISBN,
                Price = model.Book.Price,
                Description = model.Book.Description,
                StockQuantity = model.Book.StockQuantity,
                CategoryId = model.Book.CategoryId,
                // Image handled in service
            };

            var success = await _bookCommandService.AddBookAsync(book, model.ImageFile);
            if (success)
            {
                TempData["Success"] = "Book created successfully!";
                return RedirectToAction("AdminIndex");
            }

            ModelState.AddModelError("", "Failed to create book.");
            model.Categories = await _bookQueryService.GetCategoriesAsync();
            return View("Admin/CreateBook", model);
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _bookQueryService.GetEditBookViewModelAsync(id);
            if (vm == null) return NotFound();
            return View("Admin/EditBook", vm);
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookFormViewModel model)
        {
            if (id != model.Book.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                model.Categories = await _bookQueryService.GetCategoriesAsync();
                return View("Admin/EditBook", model);
            }

            var success = await _bookCommandService.UpdateBookAsync(model.Book, model.ImageFile);
            if (success)
            {
                TempData["Success"] = "Book updated!";
                return RedirectToAction("AdminIndex");
            }

            ModelState.AddModelError("", "Failed to update.");
            model.Categories = await _bookQueryService.GetCategoriesAsync();
            return View("Admin/EditBook", model);
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserIdFromClaims();
            var success = await _bookCommandService.SoftDeleteBookAsync(id, userId);
            return Json(new { success });
        }

        // User Actions
        [Authorize(Policy = "UserOrHigher")]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int bookId)
        {
            var userId = GetUserIdFromClaims();
            var success = await _bookFavoriteService.ToggleFavoriteAsync(userId, bookId);
            return Json(new { success });
        }

        // Book Details with Reviews
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookQueryService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var userId = GetUserIdFromClaims();
            var bookRating = await _reviewService.GetBookRatingAsync(id);

            // Check if current user has a review for this book
            if (userId > 0)
            {
                var userReview = await _reviewService.GetUserReviewForBookAsync(userId, id);
                if (userReview != null)
                {
                    bookRating.HasUserReview = true;
                    bookRating.UserReview = new ReviewDisplayViewModel
                    {
                        Id = userReview.Id,
                        BookId = userReview.BookId,
                        UserId = userReview.UserId,
                        Rating = userReview.Rating,
                        ReviewText = userReview.ReviewText,
                        CreatedAt = userReview.CreatedAt,
                        UpdatedAt = userReview.UpdatedAt,
                        CanEdit = true,
                        IsEdited = userReview.UpdatedAt > userReview.CreatedAt.AddMinutes(1)
                    };
                }
            }

            var viewModel = book.ToDetailsViewModel(userId > 0 && !bookRating.HasUserReview, userId);
            viewModel.Rating = bookRating;

            return View(viewModel);
        }

        // Admin Book List (View)
        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<IActionResult> BookList(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, bool? inStock = null)
        {
            var model = await _bookQueryService.GetPaginatedBooksAsync(page, 12, search, categoryId, sortBy, minPrice, maxPrice, inStock);

            // Populate ViewBag for Filters UI
            ViewBag.Categories = await _bookQueryService.GetCategoriesAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.InStock = inStock;

            return View("Admin/Books", model);
        }

        // Chart Data Endpoints for Admin Dashboard
        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<JsonResult> GetMonthlyBookUploads()
        {
            try
            {
                var data = await _bookAnalyticsService.GetMonthlyBookUploadsAsync();
                var labels = data.Select(m => m.Month).ToList();
                var counts = data.Select(m => m.Count).ToList();
                return Json(new { success = true, labels, counts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load monthly data", error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<JsonResult> GetBooksByCategory()
        {
            var data = await _bookAnalyticsService.GetBooksByCategoryAsync();
            var labels = data.Select(c => c.CategoryName).ToList();
            var counts = data.Select(c => c.Count).ToList();
            return Json(new { labels, counts });
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<JsonResult> GetBooksByAuthor()
        {
            var data = await _bookAnalyticsService.GetBooksByAuthorAsync();
            var labels = data.Select(a => a.AuthorName).ToList();
            var counts = data.Select(a => a.Count).ToList();
            return Json(new { labels, counts });
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<JsonResult> GetFavoriteBookStats()
        {
            var data = await _bookAnalyticsService.GetFavoriteStatsAsync();
            var labels = new List<string> { "Favorite", "Not Favorite" };
            var counts = new List<int> { data.FavoriteCount, data.NonFavoriteCount };
            return Json(new { labels, counts });
        }

        // Helpers
        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }


    }
}
