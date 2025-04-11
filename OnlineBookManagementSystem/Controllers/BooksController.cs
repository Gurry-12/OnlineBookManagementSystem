using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize] // ✅ Add this
    public class BooksController : Controller
    {
        public readonly BookManagementContext _context;

        public BooksController(BookManagementContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            var books = _context.Books.ToList();
            return Json(new { data = books });
        }

        [HttpGet]
        public IActionResult GetBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            return Json(new { data = book });
        }

        [HttpPost]
        public IActionResult AddBook([FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            _context.Books.Add(book);
            _context.SaveChanges();
            return Json(new { success = true, message = "Book added successfully." });
        }
    }
}

