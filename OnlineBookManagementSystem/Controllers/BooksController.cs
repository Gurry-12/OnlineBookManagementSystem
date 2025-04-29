using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Services;

namespace OnlineBookManagementSystem.Controllers
{
    
    public class BooksController : BaseController
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        public IActionResult AdminIndex()
        {
            return View("Admin/AdminIndex");
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

            var redirectUrl = Url.Action("AdminIndex", "Books");
            return Json(new { success = true, redirectUrl });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var success = await _bookService.SoftDeleteBookAsync(id);
            if (!success)
                return BadRequest(new { message = "Invalid book data." });

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
    }
}
