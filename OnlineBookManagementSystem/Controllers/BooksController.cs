using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Controllers
{
    //[Authorize(Roles = "User,Admin")]
    public class BooksController : BaseController
    {
        private readonly BookManagementContext _context;

        public BooksController(BookManagementContext context)
        {
            _context = context;
        }

       // [AllowAnonymous]
       //[Authorize(Roles ="Admin")]
        public IActionResult AdminIndex()
        {
            //var data = await _context.Books.ToListAsync();
            return View("Admin/AdminIndex");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminData()
        {
            var data = await _context.Books.ToListAsync();
            return Json(data);  // Return the book data as JSON
        }


        public IActionResult UserIndex()
        {
            return View("User/UserIndex");
        }

        [HttpGet]
        [Authorize(Roles = "User")]
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
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound();

            var redirectUrl = Url.Action("DisplayBookdetails", "Books", new { id = book.Id });
            return Json(new { redirectUrl });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBook([FromBody] Book bookData)
        {
            if (bookData == null)
                return BadRequest(new { message = "Invalid book data." });

            await _context.Books.AddAsync(bookData);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Book added successfully.", bookData });
        }

       // [AllowAnonymous]
        public IActionResult CreateBookData()
        {
            return View("Admin/CreateBookData");
        }

       // [AllowAnonymous]
        public IActionResult UserList()
        {
            return View("Admin/UserList");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Where(u => u.Role == "User")
                .Select(u => new { u.Name, u.Email, Role = u.Role })
                .ToListAsync();

            return Ok(new { success = true, data = users });
        }

       // [AllowAnonymous]
        public async Task<IActionResult> DisplayBookdetails(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound();

            return View(book);
        }

        

        [HttpGet]
       // [AllowAnonymous]
        public async Task<IActionResult> GetBookDetails(int id)
        {
            var data = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (data == null)
                return NotFound();

            return View("Admin/CreateBookData", data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBookDetails([FromBody] Book bookData)
        {
            try
            {
                if (bookData == null)
                    return BadRequest(new { message = "Invalid book data." });

                var updateDetails = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookData.Id);
                if (updateDetails == null)
                    return NotFound(new { message = "Error: Book not found." });

                updateDetails.Title = bookData.Title;
                updateDetails.Author = bookData.Author;
                updateDetails.Stock = bookData.Stock;
                updateDetails.Isbn = bookData.Isbn;
                updateDetails.ImgUrl = bookData.ImgUrl;
                updateDetails.Price = bookData.Price;

                await _context.SaveChangesAsync();

                var redirectUrl = Url.Action("AdminIndex", "Books");
                return Json(new { redirectUrl });
            }
            catch (Exception ex)
            {
                // Log the error if necessary
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBook(int Id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == Id);
            if(book == null)
                BadRequest(new { message = "Invalid book data." });

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

             var redirectUrl = Url.Action("AdminIndex", "Books");
            return Json(new { redirectUrl });
        }
    }
}
