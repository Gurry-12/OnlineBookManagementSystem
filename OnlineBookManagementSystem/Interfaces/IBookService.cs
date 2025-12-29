using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IBookService
    {
        // Existing methods
        Task<List<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<bool> AddBookAsync(Book bookData, IFormFile? imageFile);
        Task<bool> UpdateBookAsync(Book bookData, IFormFile? imageFile = null);
        Task<string?> SaveImageAsync(IFormFile image, string bookId);
        Task<bool> SoftDeleteBookAsync(int id, int userId);
        Task<List<Book>> GetFavoriteBooksAsync(int userId);
        Task<bool> ToggleFavoriteAsync(int bookId, int userId);
        Task<List<object>> GetAllUsersAsync();
        Task<BookFormViewModel?> GetCreateBookViewModelAsync();
        Task<BookFormViewModel?> GetEditBookViewModelAsync(int id);
        AdminViewModel GetQuickStats(int userId);
        Task<BookListViewModel> GetPaginatedBooksAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, bool? inStock = null);
        string GetTimeAgo(DateTime time);
        IEnumerable<CategoryBookCountViewModel> BooksByCategory();
        IEnumerable<AuthorBookCountViewModel> BooksByAuthor();
        FavoriteStatsViewModel FavoriteStats();
        Task<List<SelectListItem>> GetCategoriesAsync();
        int GetTotalBooks();
        int GetTotalCategories();

        // New methods for enhanced functionality
        Task<int> GetTotalBooksCountAsync();
        Task<BookListViewModel> GetBooksForUserAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null);
        Task<BookDetailsViewModel?> GetBookDetailsForUserAsync(int bookId, int userId);
        Task<List<Book>> GetUserFavoriteBooksAsync(int userId);
        Task<(bool Success, string Message, bool IsFavorite)> ToggleUserFavoriteAsync(int bookId, int userId);
        Task<UserProfileViewModel?> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UserProfileViewModel model);
        Task<BookListViewModel> SearchBooksAsync(string query, int page, int pageSize);
        Task<BookListViewModel> GetBooksByCategoryAsync(int categoryId, int page, int pageSize);
        Task<List<Book>> GetPersonalizedRecommendationsAsync(int userId, int count);
        Task<List<Book>> GetFeaturedBooksAsync(int count);
        Task<List<Book>> GetNewArrivalsAsync(int count);
        Task<int> GetUserFavoritesCountAsync(int userId);

        // Chart and analytics methods
        Task<List<Models.ViewModel.ChartViewModel.MonthlyBookUploadViewModel>> GetMonthlyBookUploadsAsync();
        Task<List<CategoryBookCountViewModel>> GetBooksByCategoryAsync();
        Task<List<AuthorBookCountViewModel>> GetBooksByAuthorAsync();
        Task<FavoriteStatsViewModel> GetFavoriteStatsAsync();
        Task<AdminMonthlyStatsViewModel> GetMonthlyStatsAsync();
    }





    public class MonthlyBookUploadViewModel
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class AdminMonthlyStatsViewModel
    {
        public List<Models.ViewModel.ChartViewModel.MonthlyBookUploadViewModel> MonthlyUploads { get; set; } = new();
        public List<CategoryBookCountViewModel> CategoryDistribution { get; set; } = new();
        public List<AuthorBookCountViewModel> AuthorDistribution { get; set; } = new();
        public FavoriteStatsViewModel FavoriteStats { get; set; } = new();
    }
}