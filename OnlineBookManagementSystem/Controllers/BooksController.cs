using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Services;

namespace OnlineBookManagementSystem.Controllers
{
    
    public class BooksController : BaseController
    {
        private readonly IBookService _bookService;
        private readonly IActivityLogger _activityLoggerService;

        public BooksController(IBookService bookService, IActivityLogger activityLogger)
        {
            _bookService = bookService;
            _activityLoggerService = activityLogger;
        }

        public IActionResult AdminIndex()
        {
            var strId = HttpContext.Session.GetString("userId");

            if(strId == null)
                return RedirectToAction("Login", "Auth");
            int id = int.Parse(strId);


            var AdminInfo = _bookService.GetQuickStats(id);

            return View("Admin/AdminIndex", AdminInfo);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminData()
        {
            var data = await _bookService.GetAllBooksAsync();
            return Json(data);
        }

        public IActionResult UserIndex()
        {
            return View("User/UserIndex");
        }

        [HttpGet]
        
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(new { data = books });
        }

        [HttpGet]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            var redirectUrl = Url.Action("DisplayBookdetails", "Books", new { id = book.Id });
            return Json(new { redirectUrl });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBook([FromBody] Book bookData)
        {
            if (bookData == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid book data." });

            var success = await _bookService.AddBookAsync(bookData);
            if (!success)
                return StatusCode(500, new { message = "Failed to add book." });
            
            //Logs For Add
            await _activityLoggerService.LogAsync("Add", $"Added book: {bookData.Title}.");
            return Json(new { success = true, message = "Book added successfully.", bookData });
        }

        public async Task<IActionResult> CreateBookData()
        {
            var viewModel = await _bookService.GetCreateBookViewModelAsync();
            return View("Admin/CreateBookData", viewModel);
        }

        public IActionResult UserList()
        {
            return View("Admin/UserList");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _bookService.GetAllUsersAsync();
            return Ok(new { success = true, users });
        }

        public async Task<IActionResult> DisplayBookdetails(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            return View(book);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookDetails(int id)
        {
            var viewModel = await _bookService.GetEditBookViewModelAsync(id);
            if (viewModel == null)
                return NotFound();

            return View("Admin/CreateBookData", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBookDetails([FromBody] Book bookData)
        {
            if (bookData == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid book data." });

            var success = await _bookService.UpdateBookAsync(bookData);
            if (!success)
                return NotFound(new { message = "Error: Book not found." });

            // Logs for Update
            await _activityLoggerService.LogAsync("Update", $"Updated book: {bookData.Title}.");

            var redirectUrl = Url.Action("AdminIndex", "Books");
            return Json(new { success = true, redirectUrl });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBook(int id)
        {   var book = await _bookService.GetBookByIdAsync(id);
            var success = await _bookService.SoftDeleteBookAsync(id);
            if (!success)
                return BadRequest(new { message = "Invalid book data." });

            //Logs for Delete
            await _activityLoggerService.LogAsync("Delete", $"Deleted book: {book.Title}.");
            var redirectUrl = Url.Action("AdminIndex", "Books");
            return Json(new { redirectUrl });
        }

        public async Task<IActionResult> Favorite()
        {
            var books = await _bookService.GetFavoriteBooksAsync();
            return View("User/Favorite", books);
        }

        [HttpPost]
        public async Task<IActionResult> AddandRemoveFavorite(int id)
        {
            var success = await _bookService.ToggleFavoriteAsync(id);
            if (!success)
                return BadRequest(new { message = "Book not found." });

            return Json(new { success = true });
        }


        [HttpGet]
        [ActionName("BookList")]
        public async Task<IActionResult> BookListAsync(int page = 1)
        {
            int pageSize = 8;
            var model = await _bookService.GetPaginatedBooksAsync(page, pageSize);
            return View("Admin/BookList", model);
        }

        public async Task<IActionResult> ActivityLogs()
        {
            var logs = await _activityLoggerService.GetAllLogsAsync();
            return View("Admin/ActivityLogs", logs);
        }

        [HttpGet]
        public JsonResult GetMonthlyBookUploads()
        {
            var monthlyData = _bookService.MonthlyBookUpload();
            var labels = monthlyData.Select(m => m.Month).ToList();
            var counts = monthlyData.Select(m => m.Count).ToList();
            return Json(new { labels, counts });
        }

        [HttpGet]
        public JsonResult GetBooksByCategory()
        {
            var data = _bookService.BooksByCategory();
            var labels = data.Select(c => c.CategoryName).ToList();
            var counts = data.Select(c => c.Count).ToList();
            return Json(new { labels, counts });
        }

        [HttpGet]
        public JsonResult GetBooksByAuthor()
        {
            var data = _bookService.BooksByAuthor();
            var labels = data.Select(a => a.AuthorName).ToList();
            var counts = data.Select(a => a.Count).ToList();
            return Json(new { labels, counts });
        }

        [HttpGet]
        public JsonResult GetFavoriteBookStats()
        {
            var data = _bookService.FavoriteStats();
            var labels = new List<string> { "Favorite", "Not Favorite" };
            var counts = new List<int> { data.FavoriteCount, data.NonFavoriteCount };
            return Json(new { labels, counts });
        }


    }
}
