using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Models.DTOs;
using OnlineBookManagementSystem.Services.SuperAdmin;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers.SuperAdmin
{
    [Authorize(Policy = "SuperAdminOnly")]
    [Area("SuperAdmin")]
    [Route("SuperAdmin/Books")]
    public class BookController : Controller
    {
        private readonly ISuperAdminBookService _service;

        public BookController(ISuperAdminBookService service)
        {
            _service = service;
        }

        // MVC View for Dashboard/Index
        [HttpGet]
        [Route("")] // Maps to /SuperAdmin/Books
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var stats = await _service.GetQuickStatsAsync(userId);
            return View("Admin/AdminIndex", stats);
        }

        // MVC List View
        [HttpGet]
        [Route("list")] // Maps to /SuperAdmin/Books/list
        public async Task<IActionResult> BookList(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null)
        {
             var model = await _service.GetBooksAsync(page, 10, search, categoryId, sortBy);
             return View("Admin/BookList", model);
        }

        // API Endpoint
        [HttpGet]
        [Route("/api/superadmin/books")] // Absolute route
        public async Task<IActionResult> GetAll()
        {
             var model = await _service.GetBooksAsync(1, 10, null, null, null);
             return Ok(model.Books);
        }

        [HttpGet]
        [Route("create")]
        public async Task<IActionResult> Create()
        {
            var vm = await _service.GetCreateViewModelAsync();
            return View("Admin/CreateBook", vm);
        }

        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Models.ViewModel.BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _service.GetCategoriesAsync();
                return View("Admin/CreateBook", model);
            }

            var dto = new CreateBookDto
            {
                Title = model.Book.Title,
                Author = model.Book.Author,
                Price = model.Book.Price,
                ISBN = model.Book.ISBN,
                StockQuantity = model.Book.StockQuantity,
                Description = model.Book.Description,
                CategoryId = model.Book.CategoryId
            };

            var result = await _service.CreateBookAsync(dto, model.ImageFile);
            if (result == null)
            {
                 ModelState.AddModelError("", "Failed to create book.");
                 model.Categories = await _service.GetCategoriesAsync();
                 return View("Admin/CreateBook", model);
            }

            TempData["Success"] = "Book created successfully!";
            return RedirectToAction("BookList");
        }

        [HttpGet]
        [Route("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _service.GetEditViewModelAsync(id);
            if (vm == null) return NotFound();
            return View("Admin/EditBook", vm);
        }

        [HttpPost]
        [Route("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Models.ViewModel.BookFormViewModel model)
        {
             if (id != model.Book.Id) return BadRequest();

             if (!ModelState.IsValid)
             {
                 model.Categories = await _service.GetCategoriesAsync();
                 return View("Admin/EditBook", model);
             }

             var dto = new UpdateBookDto
             {
                 Id = id,
                 Title = model.Book.Title,
                 Author = model.Book.Author,
                 Price = model.Book.Price,
                 ISBN = model.Book.ISBN,
                 StockQuantity = model.Book.StockQuantity,
                 Description = model.Book.Description,
                 CategoryId = model.Book.CategoryId
             };

             var result = await _service.UpdateBookAsync(id, dto, model.ImageFile);
             if (result == null) return NotFound();

             TempData["Success"] = "Book updated!";
             return RedirectToAction("BookList");
        }

        [HttpPost]
        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _service.DeleteBookAsync(id, userId);
            return Json(new { success });
        }

        // Stats/Charts
        [HttpGet]
        [Route("GetMonthlyBookUploads")]
        public JsonResult GetMonthlyBookUploads(DateTime? startDate, DateTime? endDate)
        {
            var data = _service.MonthlyBookUpload(startDate, endDate);
            var labels = data.Select(m => m.Month).ToList();
            var counts = data.Select(m => m.Count).ToList();
            return Json(new { labels, counts });
        }

        [HttpGet]
        [Route("GetBooksByCategory")]
        public JsonResult GetBooksByCategory()
        {
            var data = _service.BooksByCategory();
            var labels = data.Select(c => c.CategoryName).ToList();
            var counts = data.Select(c => c.Count).ToList();
            return Json(new { labels, counts });
        }

        [HttpGet]
        [Route("GetBooksByAuthor")]
        public JsonResult GetBooksByAuthor()
        {
            var data = _service.BooksByAuthor();
            var labels = data.Select(a => a.AuthorName).ToList();
            var counts = data.Select(a => a.Count).ToList();
            return Json(new { labels, counts });
        }

        [HttpGet]
        [Route("GetFavoriteBookStats")]
        public JsonResult GetFavoriteBookStats()
        {
            var data = _service.FavoriteStats();
            var labels = new List<string> { "Favorite", "Not Favorite" };
            var counts = new List<int> { data.FavoriteCount, data.NonFavoriteCount };
            return Json(new { labels, counts });
        }
    }
}
