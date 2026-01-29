using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Mappings;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;
using OnlineBookManagementSystem.Presentation.ViewModels.User;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Books
{
    public class BookQueryService : IBookQueryService
    {
        private readonly BookManagementContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BookQueryService> _logger;

        public BookQueryService(
            BookManagementContext context,
            IMemoryCache cache,
            ILogger<BookQueryService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
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

        public async Task<int> GetTotalBooksCountAsync()
        {
            return await _context.Books.CountAsync(b => !b.IsDeleted);
        }

        public async Task<int> GetTotalBooks()
        {
            return await _context.Books.CountAsync(b => !b.IsDeleted);
        }

        public async Task<int> GetTotalCategories()
        {
            return await _context.Categories.CountAsync(c => !c.IsDeleted);
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

            return books.ToListViewModel(page, totalPages, totalCount, search, categoryId, sortBy);
        }

        public async Task<BookListViewModel> GetBooksForUserAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, int? userId = null)
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

            // Set favorite status for each book if userId is provided
            if (userId.HasValue && userId.Value > 0)
            {
                await SetFavoriteStatusForUser(books, userId.Value);
            }

            return books.ToListViewModel(page, totalPages, totalCount, search, categoryId, sortBy);
        }

        public async Task<BookListViewModel> SearchBooksAsync(string query, int page, int pageSize, int? userId = null)
        {
            return await GetBooksForUserAsync(page, pageSize, query, null, null, null, null, userId);
        }

        public async Task<BookListViewModel> GetBooksByCategoryAsync(int categoryId, int page, int pageSize, int? userId = null)
        {
            return await GetBooksForUserAsync(page, pageSize, null, categoryId, null, null, null, userId);
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

            // Set the IsFavorite property (non-persisted)
            book.IsFavorite = isFavorite;

            var canReview = await _context.BookReviews
                .AnyAsync(r => r.BookId == bookId && r.UserId == userId) == false;

            return book.ToDetailsViewModel(canReview, userId);
        }

        public async Task<List<Book>> GetPersonalizedRecommendationsAsync(int userId, int count)
        {
            var userFavoriteCategories = await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Book)
                .Select(uf => uf.Book.CategoryId)
                .Distinct()
                .ToListAsync();

            List<Book> books = !userFavoriteCategories.Any()
                ? await GetFeaturedBooksAsync(count)
                : await _context.Books
                    .Where(b => !b.IsDeleted && userFavoriteCategories.Contains(b.CategoryId ?? 0))
                    .OrderByDescending(b => b.AverageRating)
                    .ThenByDescending(b => b.CreatedAt)
                    .Take(count)
                    .ToListAsync();

            await SetFavoriteStatusForUser(books, userId);
            return books;
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

        public async Task<List<Book>> GetNewArrivalsAsync(int count, int? userId = null)
        {
            var books = await _context.Books
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();

            if (userId.HasValue && userId.Value > 0)
            {
                await SetFavoriteStatusForUser(books, userId.Value);
            }

            return books;
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

        public async Task<int> GetUserFavoritesCountAsync(int userId)
        {
            return await _context.UserFavorites.CountAsync(uf => uf.UserId == userId);
        }

        public async Task<BookFormViewModel?> GetCreateBookViewModelAsync()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return BookMappingExtensions.CreateFormViewModel(categories.ToSelectListItems());
        }

        public async Task<BookFormViewModel?> GetEditBookViewModelAsync(int id)
        {
            var book = await GetBookByIdAsync(id);
            if (book == null) return null;

            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return book.ToFormViewModel(categories.ToSelectListItems());
        }

        public async Task<List<SelectListItem>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync()
                .ContinueWith(task => task.Result.ToSelectListItems().ToList());
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

        public async Task<List<Presentation.ViewModels.ChartViewModel.MonthlyBookUploadViewModel>> GetMonthlyBookUploadsAsync()
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
                .Select(x => new Presentation.ViewModels.ChartViewModel.MonthlyBookUploadViewModel
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

        public async Task<UserProfileViewModel?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted != true);
            if (user == null) return null;

            var favoritesCount = await _context.UserFavorites.CountAsync(uf => uf.UserId == userId);
            var ordersCount = await _context.Orders.CountAsync(o => o.UserId == userId && !o.IsDeleted);
            var totalSpent = await _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted && o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount.Amount);

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

        public async Task<List<object>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsDeleted == false)
                .Select(u => new { u.Id, u.Name, u.Email })
                .ToListAsync<object>();
        }

        // Additional methods for book browsing controller
        public async Task<List<Book>> GetBookSuggestionsAsync(string query, int count = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return new List<Book>();

                return await _context.Books
                    .Include(b => b.Category)
                    .Where(b => !b.IsDeleted && 
                               (b.Title.Contains(query) || 
                                b.Author.Contains(query) || 
                                b.Category.Name.Contains(query)))
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting book suggestions for query: {Query}", query);
                return new List<Book>();
            }
        }

        private async Task SetFavoriteStatusForUser(List<Book> books, int userId)
        {
            var userFavoriteBookIds = await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.BookId)
                .ToListAsync();

            foreach (var book in books)
            {
                book.IsFavorite = userFavoriteBookIds.Contains(book.Id);
            }
        }
    }
}