using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;

namespace OnlineBookManagementSystem.Services
{
    public class BookServices : IBookService
    {
        private readonly BookManagementContext _context;
        private readonly IWebHostEnvironment _env;
        public BookServices(BookManagementContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<Models.Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Where(b => b.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<Models.Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> AddBookAsync(Book bookData)
        {
           
            // Save to DB
            await _context.Books.AddAsync(bookData);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBookAsync(Models.Book bookData)
        {
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookData.Id);
            if (existingBook == null)
                return false;

            existingBook.Title = bookData.Title;
            existingBook.Author = bookData.Author;
            existingBook.Stock = bookData.Stock;
            existingBook.Isbn = bookData.Isbn;
            existingBook.ImgUrl = bookData.ImgUrl;
            existingBook.Price = bookData.Price;
            existingBook.CategoryId = bookData.CategoryId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> SaveImageAsync(IFormFile imgFile)
        {
            // Define the upload folder path (relative to wwwroot)
            // Ensure the folder exists
            var imagesFolder = Path.Combine(_env.WebRootPath, "images", "books-section");
            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            // Full file path
            var filePath = Path.Combine(imagesFolder, imgFile.FileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imgFile.CopyToAsync(stream);
            }

            // Return the relative path to the image (to be stored in the database)
            return $"/images/books-section/{imgFile.FileName}" ;
        }


        public async Task<bool> SoftDeleteBookAsync(int id)
        {
            var book = await _context.Books
                .Where(b => (bool)!b.IsDeleted)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return false;

            book.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Models.Book>> GetFavoriteBooksAsync()
        {
            return await _context.Books
                .Where(b => (bool)!b.IsDeleted && (bool)b.IsFavorite)
                .ToListAsync();
        }

        public async Task<bool> ToggleFavoriteAsync(int id)
        {
            var book = await _context.Books
                .Where(b => (bool)!b.IsDeleted)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return false;

            book.IsFavorite = !(book.IsFavorite ?? false);
            await _context.SaveChangesAsync();
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
                Book = null,
                CategoryList = categories
            };
        }

        public async Task<BookFormViewModel?> GetEditBookViewModelAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return null;

            var categories = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

            return new BookFormViewModel
            {
                Book = book,
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
            // Await the task to get the list of books and then count them
            int totalBooks = (await GetAllBooksAsync()).Count;
            var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

            var books = _context.Books.Where(b => b.IsDeleted == false)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new BookListViewModel
            {
                Books = books,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload()
        {
            // Fetch books with a valid CreatedDate
            var books = _context.Books
                .Where(b => b.CreatedDate != null && b.IsDeleted == false) // Ensure CreatedDate is not null
                .ToList(); // Fetch data to perform client-side operations

            // Group by Year and Month, and create the result
            var monthlyData = books
                .GroupBy(b => new { b.CreatedDate.Year, b.CreatedDate.Month }) // Use Value for nullable DateTime
                .Select(g => new MonthlyBookUploadViewModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            return monthlyData;
        }

        public IEnumerable<CategoryBookCountViewModel> BooksByCategory()
        {
            return _context.Books
                .Where(b => b.Category != null && b.IsDeleted == false)
                .GroupBy(b => b.Category!.Name)
                .Select(g => new CategoryBookCountViewModel
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .ToList();
        }

        public IEnumerable<AuthorBookCountViewModel> BooksByAuthor()
        {
            return _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Author) && b.IsDeleted == false) 
                .GroupBy(b => b.Author!)
                .Select(g => new AuthorBookCountViewModel
                {
                    AuthorName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(a => a.Count)
                .ToList();
        }

        public FavoriteStatsViewModel FavoriteStats()
        {
            var total = _context.Books.Count();
            var favoriteCount = _context.Books.Count(b => b.IsFavorite == true && b.IsDeleted == false);
            return new FavoriteStatsViewModel
            {
                FavoriteCount = favoriteCount,
                NonFavoriteCount = total - favoriteCount
            };
        }


    }
}
