using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;
using OnlineBookManagementSystem.Repositories;

namespace OnlineBookManagementSystem.Services
{
    public class BookServices : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly BookManagementContext _context;
        private readonly IWebHostEnvironment _env;

        public BookServices(IBookRepository bookRepository, BookManagementContext context, IWebHostEnvironment env)
        {
            _bookRepository = bookRepository;
            _context = context;
            _env = env;
        }

        public async Task<List<Models.Book>> GetAllBooksAsync()
        {
            var books = await _bookRepository.GetActiveBooksAsync();
            return books.ToList();
        }

        public async Task<Models.Book?> GetBookByIdAsync(int id)
        {
            return await _bookRepository.GetByIdAsync(id);
        }

        public async Task<bool> AddBookAsync(BookViewModel bookVm)
        {
            var book = new Book
            {
                Title = bookVm.Title,
                Author = bookVm.Author,
                Price = bookVm.Price,
                Isbn = bookVm.Isbn,
                ImgUrl = bookVm.ImgUrl,
                Stock = bookVm.Stock,
                CategoryId = bookVm.CategoryId,
                IsFavorite = bookVm.IsFavorite ?? false,
                IsDeleted = false
            };

            await _bookRepository.AddAsync(book);
            return true;
        }

        public async Task<bool> UpdateBookAsync(BookViewModel bookVm)
        {
            var existingBook = await _bookRepository.GetByIdAsync(bookVm.Id);
            if (existingBook == null)
                return false;

            existingBook.Title = bookVm.Title;
            existingBook.Author = bookVm.Author;
            existingBook.Stock = bookVm.Stock;
            existingBook.Isbn = bookVm.Isbn;
            existingBook.ImgUrl = bookVm.ImgUrl;
            existingBook.Price = bookVm.Price;
            existingBook.CategoryId = bookVm.CategoryId;

            await _bookRepository.UpdateAsync(existingBook);
            return true;
        }

        public async Task<string> SaveImageAsync(IFormFile imgFile)
        {
            var imagesFolder = Path.Combine(_env.WebRootPath, "images", "books-section");
            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            var filePath = Path.Combine(imagesFolder, imgFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imgFile.CopyToAsync(stream);
            }

            return $"/images/books-section/{imgFile.FileName}";
        }

        public async Task<bool> SoftDeleteBookAsync(int id)
        {
            return await _bookRepository.SoftDeleteAsync(id);
        }

        public async Task<List<Models.Book>> GetFavoriteBooksAsync()
        {
            var books = await _bookRepository.GetFavoriteBooksAsync();
            return books.ToList();
        }

        public async Task<bool> ToggleFavoriteAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book == null || (book.IsDeleted ?? false))
                return false;

            book.IsFavorite = !(book.IsFavorite ?? false);
            await _bookRepository.UpdateAsync(book);
            return true;
        }

        public async Task<List<object>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "User" && u.IsDeleted == false)
                .Select(u => new
                {   u.Id,
                    u.Name,
                    u.Email,
                    Role = u.Role,
                    CartItemCount = u.ShoppingCarts.Count(sc => sc.UserId == u.Id && sc.IsDeleted == false)
                })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<BookFormViewModel?> GetCreateBookViewModelAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

            return new BookFormViewModel
            {
                Book = new BookViewModel(),
                CategoryList = categories
            };
        }

        public async Task<BookFormViewModel?> GetEditBookViewModelAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
                return null;

            var bookVm = new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                Isbn = book.Isbn,
                ImgUrl = book.ImgUrl,
                Stock = book.Stock,
                CategoryId = book.CategoryId,
                IsFavorite = book.IsFavorite,
                CategoryName = book.Category?.Name
            };

            var categories = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

            return new BookFormViewModel
            {
                Book = bookVm,
                CategoryList = categories
            };
        }

        public AdminViewModel GetQuickStats(int id)
        {
            var logs = _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Take(3)
                .ToList();

            var admin = _context.Users.FirstOrDefault(u => u.Id == id && u.IsDeleted == false);

            return new AdminViewModel
            {
                TotalBooks = _context.Books.Count(b => b.IsDeleted == false),
                TotalUsers = _context.Users.Count(u => u.IsDeleted == false && u.Role == "User"),
                TotalOrders = _context.Orders.Count(),
                TotalCategories = _context.Categories.Count(c => !c.IsDeleted),
                User = admin!,
                ActivityLogs = logs.Select(log => new ActivityLogViewModel
                {
                    ActionType = log.ActionType,
                    Description = log.Description,
                    Timestamp = log.Timestamp,
                    UserName = log.User?.Name ?? "System",
                    TimeAgo = GetTimeAgo(log.Timestamp)
                }).ToList()
            };
        }

        public string GetTimeAgo(DateTime time)
        {
            // Convert the input time (which is in IST) to UTC for comparison
            var timeInUtc = TimeZoneInfo.ConvertTimeToUtc(time, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            var span = DateTime.UtcNow.Subtract(timeInUtc);

            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} mins ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
            return $"{(int)span.TotalDays} days ago";
        }

        public async Task<BookListViewModel> GetPaginatedBooksAsync(int page, int pageSize)
        {
            var (books, totalPages, currentPage) = await _bookRepository.GetPaginatedBooksDataAsync(page, pageSize);

            var bookVms = books.Select(b => new BookViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Price = b.Price,
                Isbn = b.Isbn,
                ImgUrl = b.ImgUrl,
                Stock = b.Stock,
                CategoryId = b.CategoryId,
                IsFavorite = b.IsFavorite,
                CategoryName = b.Category?.Name
            }).ToList();

            return new BookListViewModel
            {
                Books = bookVms,
                CurrentPage = currentPage,
                TotalPages = totalPages
            };
        }

        public IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload()
        {
            return _bookRepository.MonthlyBookUpload();
        }

        public IEnumerable<CategoryBookCountViewModel> BooksByCategory()
        {
            return _bookRepository.BooksByCategory();
        }

        public IEnumerable<AuthorBookCountViewModel> BooksByAuthor()
        {
            return _bookRepository.BooksByAuthor();
        }

        public FavoriteStatsViewModel FavoriteStats()
        {
            return _bookRepository.FavoriteStats();
        }
    }
}
