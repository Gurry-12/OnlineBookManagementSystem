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

                bookData.CreatedAt = DateTime.UtcNow;
                bookData.UpdatedAt = DateTime.UtcNow;
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
                existing.StockQuantity = bookData.StockQuantity; // Update Stock
                existing.LowStockThreshold = bookData.LowStockThreshold; // Update Threshold
                existing.UpdatedAt = DateTime.UtcNow;

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
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(image.FileName + DateTime.UtcNow.Ticks.ToString()));
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
            book.UpdatedAt = DateTime.UtcNow;

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
            book.UpdatedAt = DateTime.UtcNow;

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

        public async Task<BookListViewModel> GetPaginatedBooksAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, bool? inStock = null)
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

            if (minPrice.HasValue)
            {
                query = query.Where(b => b.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice);
            }

            if (inStock.HasValue)
            {
                if (inStock.Value)
                    query = query.Where(b => b.StockQuantity > 0);
                else
                    query = query.Where(b => b.StockQuantity <= 0);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            query = sortBy switch
            {
                "priceAsc" => query.OrderBy(b => b.Price),
                "priceDesc" => query.OrderByDescending(b => b.Price),
                "title" => query.OrderBy(b => b.Title),
                "stock" => query.OrderBy(b => b.StockQuantity),
                "stockDesc" => query.OrderByDescending(b => b.StockQuantity),
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

        public string GetTimeAgo(DateTime time)
        {
            var now = DateTime.UtcNow;
            var diff = now - time;
            return diff.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)diff.TotalMinutes} min ago",
                < 1440 => $"{(int)diff.TotalHours} hrs ago",
                _ => $"{(int)diff.TotalDays} days ago"
            };
        }

        //public IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload(DateTime? startDate = null, DateTime? endDate = null)
        //{
        //    var query = _context.Books
        //        .Where(b => !b.IsDeleted)
        //        .Select(b => b.CreatedAt.Date);

        //    if (startDate.HasValue) query = query.Where(d => d >= startDate.Value.Date);
        //    if (endDate.HasValue) query = query.Where(d => d <= endDate.Value.Date);

        //    var monthlyData = query
        //        .GroupBy(d => new { d.Year, d.Month })
        //        .Select(g => new MonthlyBookUploadViewModel
        //        {
        //            Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
        //            Count = g.Count()
        //        })
        //        .OrderBy(x => x.Month)
        //        .ToList() ?? new List<MonthlyBookUploadViewModel>();

        //    return monthlyData;
        //}

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

        public int GetTotalBooks()
        {
            return _context.Books.Count(b => !b.IsDeleted);
        }

        public int GetTotalCategories()
        {
            return _context.Categories.Count(c => !c.IsDeleted);
        }

        // New methods for enhanced functionality
        public async Task<int> GetTotalBooksCountAsync()
        {
            return await _context.Books.CountAsync(b => !b.IsDeleted);
        }

        public async Task<BookListViewModel> GetBooksForUserAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Where(b => !b.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search) || b.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(b => b.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            query = sortBy switch
            {
                "priceAsc" => query.OrderBy(b => b.Price),
                "priceDesc" => query.OrderByDescending(b => b.Price),
                "title" => query.OrderBy(b => b.Title),
                "author" => query.OrderBy(b => b.Author),
                "rating" => query.OrderByDescending(b => b.AverageRating),
                _ => query.OrderByDescending(b => b.CreatedAt)
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

        public async Task<BookDetailsViewModel?> GetBookDetailsForUserAsync(int bookId, int userId)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == bookId && !b.IsDeleted);

            if (book == null) return null;

            // Check if user has favorited this book
            var isFavorite = await _context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.BookId == bookId);

            // Get related books from same category
            var relatedBooks = await _context.Books
                .Where(b => b.CategoryId == book.CategoryId && b.Id != bookId && !b.IsDeleted)
                .Take(4)
                .ToListAsync();

            var newBook = new Book
            {

                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Price = book.Price,
                Description = book.Description,
                ImageUrl = book.ImageUrl,
                CategoryId = book.CategoryId,
                Category = book.Category,
                StockQuantity = book.StockQuantity,
                AverageRating = book.AverageRating,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                IsFavorite = isFavorite

            };

            return new BookDetailsViewModel
            {
                Book = newBook,
                Rating = new BookRatingViewModel
                {
                    AverageRating = book.AverageRating,
                    TotalReviews = await _context.BookReviews.CountAsync(r => r.BookId == bookId)
                },
                CanReview = await _context.BookReviews
                    .AnyAsync(r => r.BookId == bookId && r.UserId == userId) == false,
                ReviewForm = new ReviewSubmissionViewModel
                {
                    BookId = bookId
                }
            };
        }

        public async Task<List<Book>> GetUserFavoriteBooksAsync(int userId)
        {
            return await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Book)
                    .ThenInclude(b => b.Category)
                .Select(uf => uf.Book)
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.UpdatedAt)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, bool IsFavorite)> ToggleUserFavoriteAsync(int bookId, int userId)
        {
            try
            {
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && !b.IsDeleted);
                if (book == null)
                    return (false, "Book not found", false);

                var existingFavorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.BookId == bookId);

                bool isFavorite;
                if (existingFavorite != null)
                {
                    _context.UserFavorites.Remove(existingFavorite);
                    isFavorite = false;
                    await _activityLogger.LogAsync("FavoriteRemoved", $"Removed '{book.Title}' from favorites", userId);
                }
                else
                {
                    _context.UserFavorites.Add(new UserFavorite
                    {
                        UserId = userId,
                        BookId = bookId,
                        CreatedAt = DateTime.UtcNow
                    });
                    isFavorite = true;
                    await _activityLogger.LogAsync("FavoriteAdded", $"Added '{book.Title}' to favorites", userId);
                }

                await _context.SaveChangesAsync();
                return (true, isFavorite ? "Added to favorites" : "Removed from favorites", isFavorite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle favorite for book {BookId} and user {UserId}", bookId, userId);
                return (false, "An error occurred", false);
            }
        }

        public async Task<UserProfileViewModel?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted != true);
            if (user == null) return null;

            var favoritesCount = await _context.UserFavorites.CountAsync(uf => uf.UserId == userId);
            var ordersCount = await _context.Orders.CountAsync(o => o.UserId == userId && !o.IsDeleted);
            var totalSpent = await _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted && o.PaymentStatus == "Paid")
                .SumAsync(o => o.TotalAmount);

            return new UserProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                MemberSince = user.CreatedAt,
                LastLoginDate = user.LastLoginDate,
                TotalFavorites = favoritesCount,
                TotalOrders = ordersCount,
                TotalSpent = totalSpent
            };
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UserProfileViewModel model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted != true);
                if (user == null) return false;

                user.Name = model.Name;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await _activityLogger.LogAsync("ProfileUpdated", "User profile updated", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user profile for user {UserId}", userId);
                return false;
            }
        }

        public async Task<BookListViewModel> SearchBooksAsync(string query, int page, int pageSize)
        {
            return await GetBooksForUserAsync(page, pageSize, query);
        }

        public async Task<BookListViewModel> GetBooksByCategoryAsync(int categoryId, int page, int pageSize)
        {
            return await GetBooksForUserAsync(page, pageSize, null, categoryId);
        }

        public async Task<List<Book>> GetPersonalizedRecommendationsAsync(int userId, int count)
        {
            // Simple recommendation based on user's favorite categories
            var userFavoriteCategories = await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Book)
                .Select(uf => uf.Book.CategoryId)
                .Distinct()
                .ToListAsync();

            if (!userFavoriteCategories.Any())
            {
                // Return popular books if no favorites
                return await GetFeaturedBooksAsync(count);
            }

            return await _context.Books
                .Where(b => !b.IsDeleted && userFavoriteCategories.Contains(b.CategoryId ?? 0))
                .OrderByDescending(b => b.AverageRating)
                .ThenByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Book>> GetFeaturedBooksAsync(int count)
        {
            return await _context.Books
                .Where(b => !b.IsDeleted && b.IsFeatured == true)
                .OrderByDescending(b => b.AverageRating)
                .ThenByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Book>> GetNewArrivalsAsync(int count)
        {
            return await _context.Books
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUserFavoritesCountAsync(int userId)
        {
            return await _context.UserFavorites.CountAsync(uf => uf.UserId == userId);
        }

        // Chart and analytics methods
        public async Task<List<Models.ViewModel.ChartViewModel.MonthlyBookUploadViewModel>> GetMonthlyBookUploadsAsync()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            // First get the grouped data from database
            var monthlyData = await _context.Books
                .Where(b => !b.IsDeleted && b.CreatedAt >= sixMonthsAgo)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // Then format the month names on the client side
            return monthlyData
                .Select(x => new Models.ViewModel.ChartViewModel.MonthlyBookUploadViewModel
                {
                    Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                    Count = x.Count
                })
                .ToList();
        }

        public async Task<List<CategoryBookCountViewModel>> GetBooksByCategoryAsync()
        {
            return await _context.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.Category)
                .GroupBy(b => b.Category.Name)
                .Select(g => new CategoryBookCountViewModel
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        public async Task<List<AuthorBookCountViewModel>> GetBooksByAuthorAsync()
        {
            return await _context.Books
                .Where(b => !b.IsDeleted && !string.IsNullOrEmpty(b.Author))
                .GroupBy(b => b.Author)
                .Select(g => new AuthorBookCountViewModel
                {
                    AuthorName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();
        }

        public async Task<FavoriteStatsViewModel> GetFavoriteStatsAsync()
        {
            var totalBooks = await _context.Books.CountAsync(b => !b.IsDeleted);
            var favoriteCount = await _context.UserFavorites.CountAsync();

            return new FavoriteStatsViewModel
            {
                FavoriteCount = favoriteCount,
                NonFavoriteCount = totalBooks - favoriteCount
            };
        }

        public async Task<AdminMonthlyStatsViewModel> GetMonthlyStatsAsync()
        {
            return new AdminMonthlyStatsViewModel
            {
                MonthlyUploads = await GetMonthlyBookUploadsAsync(),
                CategoryDistribution = await GetBooksByCategoryAsync(),
                AuthorDistribution = await GetBooksByAuthorAsync(),
                FavoriteStats = await GetFavoriteStatsAsync()
            };
        }
    }
}