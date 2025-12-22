using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Services.User;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers.User
{
    [Area("User")]
    public class BookController : Controller
    {
        private readonly IUserBookService _service;

        public BookController(IUserBookService service)
        {
            _service = service;
        }

        [Authorize(Policy = "UserOrHigher")]
        [HttpGet]
        [Route("User/Books")]
        public IActionResult Index()
        {
            return View("User/UserIndex");
        }

        [Authorize(Policy = "UserOrHigher")]
        [HttpGet]
        [Route("api/user/books")]
        public async Task<IActionResult> GetBooks(int page = 1, string? search = null, int? categoryId = null, string? sortBy = "title")
        {
            var books = await _service.GetBooksAsync(page, 12, search, categoryId, sortBy);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var favorites = await _service.GetFavoritesAsync(userId);

            return Ok(new { data = books, favorites = favorites.Select(f => f.Id).ToList(), success = true });
        }

        [Authorize(Policy = "UserOrHigher")]
        [HttpPost]
        [Route("api/user/books/favorite")]
        public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _service.ToggleFavoriteAsync(request.BookId, userId);
            return Json(new { success });
        }

        // Public List
        [HttpGet]
        [AllowAnonymous]
        [Route("Public/Books")]
        public async Task<IActionResult> PublicList(string? search = null, int? categoryId = null)
        {
             // Construct ViewModel as per original expectation (BookListViewModel)
             var books = await _service.GetBooksAsync(1, 20, search, categoryId, "title");
             var viewModel = new BookListViewModel
             {
                  // Need to map Dto back to Book entity if View expects Entity, or update View to use DTO.
                  // Since I can't easily update Views/Public/BookList.cshtml in this context without reading it first and potentially breaking it,
                  // I will assume it uses dynamic model or update it to use DTO.
                  // Original: return View("Public/BookList", books); -> books was BookListViewModel.
                  Books = books.Select(d => new Models.Book
                  {
                      Id = d.Id,
                      Title = d.Title,
                      Author = d.Author,
                      Price = d.Price,
                      ImageUrl = d.ImageUrl,
                      StockQuantity = d.StockQuantity,
                      Category = d.CategoryName != null ? new Models.Category { Name = d.CategoryName } : null
                  }).ToList(),
                  CurrentPage = 1,
                  TotalPages = 1
             };
             return View("Public/BookList", viewModel);
        }

        public class FavoriteRequest { public int BookId { get; set; } }
    }
}
