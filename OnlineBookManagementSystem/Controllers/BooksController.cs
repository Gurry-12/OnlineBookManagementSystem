using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

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

        public IActionResult AdminIndex()
        {

            return View("Admin/AdminIndex");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminData()
        {
            var data = await _context.Books.Where(b => (bool)!b.IsDeleted).ToListAsync();
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

            var books = _context.Books.Where(b => (bool)!b.IsDeleted).ToList();
            return Ok(new { data = books });
        }

        [HttpGet]
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

            if (!ModelState.IsValid)
            {

                return Json(new { message = "Validation failed", });
            }

            await _context.Books.AddAsync(bookData);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Book added successfully.", bookData });
        }


        public IActionResult CreateBookData()
        {
            var viewModel = new BookFormViewModel
            {
                Book = null,
                CategoryList = (IEnumerable<SelectListItem>)_context.Categories
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList()
            };

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
            var users = await _context.Users
                .Where(u => u.Role == "User")
                .Select(u => new { u.Name, u.Email, Role = u.Role, CartItemCount = u.ShoppingCarts.Count(sc => sc.UserId == u.Id) })
                .ToListAsync();

            return Ok(new { success = true, users });
        }

        public async Task<IActionResult> DisplayBookdetails(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound();

            return View(book);
        }



        [HttpGet]

        public async Task<IActionResult> GetBookDetails(int id)
        {
            var data = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (data == null)
                return NotFound();

            var viewModel = new BookFormViewModel
            {
                Book = data,
                CategoryList = (IEnumerable<SelectListItem>)_context.Categories
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList()
            };


            return View("Admin/CreateBookData", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBookDetails([FromBody] Book bookData)
        {
            try
            {
                if (bookData == null)
                    return BadRequest(new { message = "Invalid book data." });
                if (!ModelState.IsValid)
                {
                    return Json(new { message = "Validation failed", });
                }

                var updateDetails = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookData.Id);
                if (updateDetails == null)
                    return NotFound(new { message = "Error: Book not found." });

                updateDetails.Title = bookData.Title;
                updateDetails.Author = bookData.Author;
                updateDetails.Stock = bookData.Stock;
                updateDetails.Isbn = bookData.Isbn;
                updateDetails.ImgUrl = bookData.ImgUrl;
                updateDetails.Price = bookData.Price;
                updateDetails.CategoryId = bookData.CategoryId;
                await _context.SaveChangesAsync();

                var redirectUrl = Url.Action("AdminIndex", "Books");
                return Json(new { success = true, redirectUrl });
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
            var book = await _context.Books.Where(b => (bool)!b.IsDeleted).FirstOrDefaultAsync(b => b.Id == Id);
            if (book == null)
                BadRequest(new { message = "Invalid book data." });

            book.IsDeleted = true;
            await _context.SaveChangesAsync();

            var redirectUrl = Url.Action("AdminIndex", "Books");
            return Json(new { redirectUrl });
        }

        public IActionResult Favorite()
        {
            var books = _context.Books.Where(b => (bool)!b.IsDeleted && (bool)b.IsFavorite).ToList();
            return View("User/Favorite", books);
        }

        [HttpPost]
        public IActionResult AddandRemoveFavorite(int id)
        {
            var book = _context.Books.Where(b => (bool)!b.IsDeleted).FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                book.IsFavorite = !(book.IsFavorite ?? false);
                _context.SaveChanges();
            }
            
            return Json(new { success = true });
        }
    }
}
