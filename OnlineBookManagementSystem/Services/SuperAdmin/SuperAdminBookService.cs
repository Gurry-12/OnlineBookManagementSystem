using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Interfaces.Repositories;
using OnlineBookManagementSystem.Models.DTOs;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;
using OnlineBookManagementSystem.Services.Common;

namespace OnlineBookManagementSystem.Services.SuperAdmin
{
    public class SuperAdminBookService : ISuperAdminBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBookManager _bookManager;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<SuperAdminBookService> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly ICategoryInterface _categoryService;

        public SuperAdminBookService(
            IBookRepository bookRepository,
            IOrderRepository orderRepository,
            ICategoryInterface categoryService,
            IBookManager bookManager,
            IActivityLogger activityLogger,
            ILogger<SuperAdminBookService> logger)
        {
            _bookRepository = bookRepository;
            _orderRepository = orderRepository;
            _categoryService = categoryService;
            _bookManager = bookManager;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<AdminViewModel> GetQuickStatsAsync(int userId)
        {
             var books = await _bookRepository.CountAsync();
             var orders = await _orderRepository.CountAsync();
             return new AdminViewModel
             {
                 TotalBooks = books,
                 TotalOrders = orders,
                 TotalUsers = 0,
                 RecentActivity = new List<Models.ActivityLog>()
             };
        }

        public async Task<BookListViewModel> GetBooksAsync(int page, int pageSize, string? search, int? categoryId, string? sortBy)
        {
            var books = await _bookRepository.GetPaginatedAsync(
                (page - 1) * pageSize,
                pageSize,
                b => (string.IsNullOrEmpty(search) || b.Title.Contains(search) || b.Author.Contains(search)) &&
                     (!categoryId.HasValue || b.CategoryId == categoryId),
                sortBy
            );

            var total = await _bookRepository.CountAsync(
                b => (string.IsNullOrEmpty(search) || b.Title.Contains(search) || b.Author.Contains(search)) &&
                     (!categoryId.HasValue || b.CategoryId == categoryId)
            );

            return new BookListViewModel
            {
                Books = books,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchTerm = search,
                CategoryId = categoryId,
                SortBy = sortBy
            };
        }

        public async Task<BookDto?> CreateBookAsync(CreateBookDto dto, IFormFile? imageFile)
        {
            try
            {
                var book = _bookManager.MapToEntity(dto);
                await _bookRepository.AddAsync(book);
                await _bookRepository.SaveChangesAsync(); // Get ID

                if (imageFile != null)
                {
                    book.ImageUrl = await _bookManager.SaveImageAsync(imageFile, book.Id.ToString());
                    await _bookRepository.SaveChangesAsync();
                }

                await _activityLogger.LogAsync("BookAdded", $"SuperAdmin created book '{book.Title}'", book.CategoryId ?? 0);
                return _bookManager.MapToDto(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book");
                return null;
            }
        }

        public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto dto, IFormFile? imageFile)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return null;

            _bookManager.UpdateEntity(book, dto);

            if (imageFile != null)
            {
                _bookManager.DeleteImage(book.ImageUrl);
                book.ImageUrl = await _bookManager.SaveImageAsync(imageFile, book.Id.ToString());
            }

            await _bookRepository.SaveChangesAsync();
            await _activityLogger.LogAsync("BookUpdated", $"SuperAdmin updated book '{book.Title}'", book.CategoryId ?? 0);
            return _bookManager.MapToDto(book);
        }

        public async Task<bool> DeleteBookAsync(int id, int userId)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return false;

            book.IsDeleted = true;
            book.UpdatedAt = DateTime.UtcNow;
            await _bookRepository.SaveChangesAsync();

            await _activityLogger.LogAsync("BookDeleted", $"SuperAdmin deleted book '{book.Title}'", userId);
            return true;
        }

        public async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetCategoriesAsync()
        {
            var cats = await _categoryService.GetAllCategoriesAsync();
            return cats.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
        }

        public async Task<BookFormViewModel?> GetCreateViewModelAsync()
        {
            return new BookFormViewModel { Categories = await GetCategoriesAsync() };
        }

        public async Task<BookFormViewModel?> GetEditViewModelAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return null;

            return new BookFormViewModel
            {
                Book = book,
                Categories = await GetCategoriesAsync()
            };
        }

        public IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _bookRepository.Query()
                .Select(b => b.CreatedAt.Date);

            if (startDate.HasValue) query = query.Where(d => d >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(d => d <= endDate.Value.Date);

            var monthlyData = query
                .AsEnumerable()
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
            return _bookRepository.Query()
                .Include(b => b.Category)
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
            return _bookRepository.Query()
                .Where(b => !string.IsNullOrEmpty(b.Author))
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
            var total = _bookRepository.Query().Count();
            var favoriteCount = _bookRepository.Query().Count(b => b.IsFavorite);
            return new FavoriteStatsViewModel
            {
                FavoriteCount = favoriteCount,
                NonFavoriteCount = total - favoriteCount
            };
        }
    }
}
