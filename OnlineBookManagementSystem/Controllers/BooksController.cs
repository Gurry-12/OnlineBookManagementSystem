using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class BooksController : Controller
    {
        private readonly BookManagementContext _context;

        public BooksController(BookManagementContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult AdminIndex()
        {
            var data = _context.Books.ToList();
            return View("Admin/AdminIndex", data);
        }

        [AllowAnonymous]
        public IActionResult UserIndex()
        {
            return View("User/UserIndex");
        }

        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public IActionResult GetBooks()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader))
                return Unauthorized(new { message = "Authorization header is missing" });

            var books = _context.Books.ToList();
            return Ok(new { data = books });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound();

            var redirectUrl = Url.Action("DisplayBookdetails", "Books", new { id = book.Id });
            return Json(new { redirectUrl });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AddBook([FromBody] Book bookData)
        {
            if (bookData == null)
                return BadRequest();

            _context.Books.Add(bookData);
            _context.SaveChanges();
            return Json(new { success = true, message = "Book added successfully.", bookData });
        }

        [AllowAnonymous]
        public IActionResult CreateBookData()
        {
            return View("Admin/CreateBookData"); // ✅ Explicit path
        }

        [AllowAnonymous]
        public IActionResult UserList()
        {
            return View("Admin/UserList");
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users
                .Where(u => u.Role == "User")
                .Select(u => new
                {
                    u.Name,
                    u.Email,
                    Role = u.Role
                }).ToList();

            return Ok(new { success = true, data = users });
        }

        [AllowAnonymous]
        public IActionResult DisplayBookdetails(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound();

            return View(book);
        }

        [AllowAnonymous]
        public IActionResult ProfileView()
        {
            var userIdClaim = HttpContext.Session.GetString("userId");

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetBookDetails(int id)
        {
            var data = _context.Books.FirstOrDefault(b => b.Id == id);
            if (data == null)
                return Unauthorized();

            // ✅ Use the full path since view is inside Admin folder
            return View("Admin/CreateBookData", data);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public  IActionResult UpdateBookDetails([FromBody] Book bookData)
        {
            if (bookData == null)
                return BadRequest();

            var updateDetails = _context.Books.FirstOrDefault(b => b.Id == bookData.Id);
            if (updateDetails != null)
            {
                updateDetails.Title = bookData.Title;
                updateDetails.Author = bookData.Author;
                updateDetails.Stock = bookData.Stock;
                updateDetails.Isbn = bookData.Isbn;
                updateDetails.ImgUrl = bookData.ImgUrl;
                updateDetails.Price = bookData.Price;

                _context.SaveChanges();
                var redirectUrl = Url.Action("AdminIndex", "Books");
                return   Json(new { message = "Successfully updated", updateDetails, redirectUrl });
            }

            return Json(new { message = "Error: Book not found." });
        }
    }
}
