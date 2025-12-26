using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize]  // Global auth; per-action policies
    public class BooksController : BaseController
    {
        private readonly IBookService _bookService;
        private readonly IActivityLogger _activityLoggerService;

        public BooksController(IBookService bookService, IActivityLogger activityLogger)
        {
            _bookService = bookService;
            _activityLoggerService = activityLogger;
        }

        //[Authorize(Policy = "AdminOrHigher")]
        //public async Task<IActionResult> AdminIndex()
        //{
        //    var userId = GetUserIdFromClaims();  // Helper below
        //    if (userId == 0) return RedirectToAction("Login", "Auth");

        //    var adminInfo = _bookService.GetQuickStats(userId);
        //    return View("Admin/AdminIndex", adminInfo);
        //}

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<IActionResult> GetAdminData()
        {
            var data = await _bookService.GetAllBooksAsync();
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
            var books = await _bookService.GetPaginatedBooksAsync(1, 12, search, categoryId, "title");
            var favorites = await _bookService.GetFavoriteBooksAsync(userId);  // For UI highlight
            return Ok(new { data = books.Books, favorites = favorites.Select(f => f.Id).ToList(), success = true });
        }

        [HttpGet]
        [AllowAnonymous]  // Public browse for Guests
        public async Task<IActionResult> PublicList(string? search = null, int? categoryId = null)
        {
            var books = await _bookService.GetPaginatedBooksAsync(1, 20, search, categoryId, "title");
            return View("Public/BookList", books);
        }

        // CRUD Actions (AdminOrHigher)
        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await _bookService.GetCreateBookViewModelAsync();
            return View("Admin/CreateBook", vm);
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _bookService.GetCategoriesAsync();  // Repopulate
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

            var success = await _bookService.AddBookAsync(book, model.ImageFile);
            if (success)
            {
                TempData["Success"] = "Book created successfully!";
                return RedirectToAction("AdminIndex");
            }

            ModelState.AddModelError("", "Failed to create book.");
            model.Categories = await _bookService.GetCategoriesAsync();
            return View("Admin/CreateBook", model);
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _bookService.GetEditBookViewModelAsync(id);
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
                model.Categories = await _bookService.GetCategoriesAsync();
                return View("Admin/EditBook", model);
            }

            var success = await _bookService.UpdateBookAsync(model.Book, model.ImageFile);
            if (success)
            {
                TempData["Success"] = "Book updated!";
                return RedirectToAction("AdminIndex");
            }

            ModelState.AddModelError("", "Failed to update.");
            model.Categories = await _bookService.GetCategoriesAsync();
            return View("Admin/EditBook", model);
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserIdFromClaims();
            var success = await _bookService.SoftDeleteBookAsync(id, userId);
            return Json(new { success });
        }

        // User Actions
        [Authorize(Policy = "UserOrHigher")]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int bookId)
        {
            var userId = GetUserIdFromClaims();
            var success = await _bookService.ToggleFavoriteAsync(bookId, userId);
            return Json(new { success });
        }

        // Pagination/List (shared)
        [HttpGet]
        public async Task<BookListViewModel> BookList(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null)
        {
            var model = await _bookService.GetPaginatedBooksAsync(page, 8, search, categoryId, sortBy);
            return model;
        }

        // Stats/Charts (cached, AdminOrHigher)
        //[Authorize(Policy = "AdminOrHigher")]
        //[HttpGet]
        //public async Task<JsonResult> GetMonthlyBookUploads(DateTime? startDate, DateTime? endDate)
        //{
        //    var data = _bookService.MonthlyBookUpload(startDate, endDate);
        //    var labels = data.Select(m => m.Month).ToList();
        //    var counts = data.Select(m => m.Count).ToList();
        //    return Json(new { labels, counts });
        //}

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public JsonResult GetBooksByCategory()
        {
            var data = _bookService.BooksByCategory();
            var labels = data.Select(c => c.CategoryName).ToList();
            var counts = data.Select(c => c.Count).ToList();
            return Json(new { labels, counts });
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public JsonResult GetBooksByAuthor()
        {
            var data = _bookService.BooksByAuthor();
            var labels = data.Select(a => a.AuthorName).ToList();
            var counts = data.Select(a => a.Count).ToList();
            return Json(new { labels, counts });
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpGet]
        public JsonResult GetFavoriteBookStats()
        {
            var data = _bookService.FavoriteStats();
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