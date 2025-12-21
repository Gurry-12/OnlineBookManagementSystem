using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text;
using Image = SixLabors.ImageSharp.Image;

namespace OnlineBookManagementSystem.Services
{
    public class BookServices : IBookService
    {
        private readonly BookManagementContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BookServices> _logger;
        private readonly IActivityLogger _activityLogger;  // For audit

        public BookServices(
            BookManagementContext context,
            IWebHostEnvironment env,
            IMemoryCache cache,
            ILogger<BookServices> logger,
            IActivityLogger activityLogger)
        {
            _context = context;
            _env = env;
            _cache = cache;
            _logger = logger;
            _activityLogger = activityLogger;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);
        }

        public async Task<bool> AddBookAsync(Book bookData, IFormFile? imageFile = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (imageFile != null)
                {
                    bookData.ImageUrl = await SaveImageAsync(imageFile, bookData.Id.ToString());
                }

                bookData.CreatedAt = DateTimeOffset.UtcNow;
                bookData.UpdatedAt = DateTimeOffset.UtcNow;
                bookData.IsDeleted = false;

                await _context.Books.AddAsync(bookData);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _cache.Remove("allBooks");  // Invalidate cache
                _logger.LogInformation("Book added: {Title} by User {UserId}", bookData.Title, bookData.CategoryId);  // UserId from context?
                await _activityLogger.LogAsync("BookAdded", $"New book '{bookData.Title}' created.", bookData.CategoryId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to add book: {Title}", bookData.Title);
                return false;
            }
        }

        public async Task<bool> UpdateBookAsync(Book bookData, IFormFile? imageFile = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookData.Id && b.IsDeleted == false);
                if (existing == null) return false;

                if (imageFile != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, "images/books", existing.ImageUrl);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                    existing.ImageUrl = await SaveImageAsync(imageFile, bookData.Id.ToString());
                }

                existing.Title = bookData.Title;
                existing.Author = bookData.Author;
                existing.ISBN = bookData.ISBN;
                existing.Price = bookData.Price;
                existing.Description = bookData.Description;
                existing.CategoryId = bookData.CategoryId;
                existing.UpdatedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _cache.Remove($"book_{bookData.Id}");  // Invalidate
                _logger.LogInformation("Book updated: {Id}", bookData.Id);
                await _activityLogger.LogAsync("BookUpdated", $"Book '{bookData.Title}' updated.", bookData.CategoryId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update book: {Id}", bookData.Id);
                return false;
            }
        }

        public async Task<string?> SaveImageAsync(IFormFile image, string bookId)
        {
            if (image == null || image.Length == 0) return null;

            // Validate: Size < 5MB, type jpg/png
            if (image.Length > 5 * 1024 * 1024 || !image.ContentType.StartsWith("image/"))
            {
                _logger.LogWarning("Invalid image upload: {ContentType}, Size: {Length}", image.ContentType, image.Length);
                return null;
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "images/books");
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename: bookId_hash.jpg
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(image.FileName + DateTimeOffset.UtcNow.Ticks.ToString()));
            var filename = $"{bookId}_{Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-")}.{image.ContentType.Split('/')[1]}";
            var filepath = Path.Combine(uploadsDir, filename);

            using var inputStream = image.OpenReadStream();
            using var imageSharp = await Image.LoadAsync(inputStream);
            // Resize to 400x600 max, maintain aspect
            imageSharp.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(400, 600) }));
            await imageSharp.SaveAsJpegAsync(filepath);

            _logger.LogInformation("Image saved: {Filename}", filename);
            return filename;
        }

        public async Task<bool> SoftDeleteBookAsync(int id, int userId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);
            if (book == null) return false;

            book.IsDeleted = true;
            book.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            _cache.Remove($"book_{id}");
            _logger.LogInformation("Book soft-deleted: {Id} by User {UserId}", id, userId);
            await _activityLogger.LogAsync("BookDeleted", $"Book '{book.Title}' soft-deleted.", userId);
            return true;
        }

        public async Task<List<Book>> GetFavoriteBooksAsync(int userId)
        {
            // Assuming favorites are user-specific; add UserId to a junction if needed
            // For now, global favorites; extend to per-user
            return await _context.Books
                .Where(b => b.IsFavorite == true && b.IsDeleted == false)
                .OrderByDescending(b => b.UpdatedAt)
                .ToListAsync();
        }

        public async Task<bool> ToggleFavoriteAsync(int bookId, int userId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.IsDeleted == false);
            if (book == null) return false;

            book.IsFavorite = !book.IsFavorite;
            book.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Favorite toggled for book {BookId} by {UserId}", bookId, userId);
            await _activityLogger.LogAsync("FavoriteToggled", $"Book '{book.Title}' favorited/unfavorited.", userId);
            return true;
        }

        public async Task<List<object>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsDeleted == false)
                .Select(u => new { u.Id, u.Name, u.Email })
                .ToListAsync<object>();
        }

        public async Task<BookFormViewModel?> GetCreateBookViewModelAsync()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            return new BookFormViewModel { Categories = categories };
        }

        public async Task<BookFormViewModel?> GetEditBookViewModelAsync(int id)
        {
            var book = await GetBookByIdAsync(id);
            if (book == null) return null;

            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            return new BookFormViewModel
            {
                Book = book,
                Categories = categories
            };
        }

        public AdminViewModel GetQuickStats(int userId)
        {
            if (!_cache.TryGetValue($"stats_{userId}", out AdminViewModel? stats))
            {
                stats = new AdminViewModel
                {
                    TotalBooks = _context.Books.Count(b => !b.IsDeleted),
                    TotalOrders = _context.Orders.Count(o => !o.IsDeleted),
                    TotalUsers = _context.Users.Count(u => (bool)!u.IsDeleted),
                    RecentActivity = _context.ActivityLogs.OrderByDescending(l => l.Timestamp).Take(5).ToList()
                };
                _cache.Set($"stats_{userId}", stats, TimeSpan.FromMinutes(5));
            }
            return stats;
        }

        public async Task<BookListViewModel> GetPaginatedBooksAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Where(b => !b.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            query = sortBy switch
            {
                "priceAsc" => query.OrderBy(b => b.Price),
                "priceDesc" => query.OrderByDescending(b => b.Price),
                "title" => query.OrderBy(b => b.Title),
                _ => query.OrderByDescending(b => b.CreatedAt)  // Default recent
            };

            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BookListViewModel
            {
                Books = books,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchTerm = search,
                CategoryId = categoryId,
                SortBy = sortBy
            };
        }

        public string GetTimeAgo(DateTimeOffset time)
        {
            var now = DateTimeOffset.UtcNow;
            var diff = now - time;
            return diff.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)diff.TotalMinutes} min ago",
                < 1440 => $"{(int)diff.TotalHours} hrs ago",
                _ => $"{(int)diff.TotalDays} days ago"
            };
        }

        public IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            var query = _context.Books
                .Where(b => !b.IsDeleted)
                .Select(b => b.CreatedAt.Date);

            if (startDate.HasValue) query = query.Where(d => d >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(d => d <= endDate.Value.Date);

            var monthlyData = query
                .GroupBy(d => new { d.Year, d.Month })
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
                .Where(b => b.Category != null && !b.IsDeleted)
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
                .Where(b => !string.IsNullOrEmpty(b.Author) && !b.IsDeleted)
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
            var total = _context.Books.Count(b => !b.IsDeleted);
            var favoriteCount = _context.Books.Count(b => b.IsFavorite == true && !b.IsDeleted);
            return new FavoriteStatsViewModel
            {
                FavoriteCount = favoriteCount,
                NonFavoriteCount = total - favoriteCount
            };
        }

        public async Task<List<SelectListItem>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
        }
    }
}