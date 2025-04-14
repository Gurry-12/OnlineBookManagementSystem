using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize(Roles = "User,Admin")] // ✅ Add this
    //[Authorize(Roles = "Admin,User")]
    public class BooksController : Controller
    {
        public readonly BookManagementContext _context;

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

        //[Authorize(Roles ="Admin,User")]
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public IActionResult GetBooks()
        {
            // Accessing the Authorization header from the incoming request
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized(new { message = "Authorization header is missing" });
            }


            var books = _context.Books.ToList();
            return Ok(new { data = books });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            // Generate URL with book id for redirection
            var redirectUrl = Url.Action("DisplayBookdetails", "Books", new { id = book.Id });

            return Json(new { redirectUrl });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AddBook([FromBody] Book bookData)
        {
            if (bookData == null)
            {
                return BadRequest();
            }
            _context.Books.Add(bookData);
            _context.SaveChanges();
            return Json(new { success = true, message = "Book added successfully.", bookData });
        }

        //[Authorize(Roles = "User")]
        [AllowAnonymous]
        public IActionResult CreateBookData()
        {
            return View("Admin/CreateBookData");
        }

        //  [Authorize(Policy ="AdminOnly")]
        [AllowAnonymous]
        
        public IActionResult UserList()
        {
            //var data = _context.Users.ToList();
            return View("Admin/UserList");
        }

        [HttpGet]
        [Authorize(Policy ="AdminOnly")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.Where(u => u.Role == "User").Select(u => new {
                u.Name,
                u.Email,
                Role = u.Role // Or navigate if roles are in another table
            }).ToList();

            return Ok(new { success = true, data = users });
        }


        [AllowAnonymous]
        public IActionResult DisplayBookdetails(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book); // Pass book model to Razor view
        }
    }
}

