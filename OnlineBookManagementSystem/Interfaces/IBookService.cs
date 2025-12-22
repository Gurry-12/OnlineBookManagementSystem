using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<bool> AddBookAsync(Book bookData, IFormFile? imageFile);
        Task<bool> UpdateBookAsync(Book bookData, IFormFile? imageFile = null);
        Task<string?> SaveImageAsync(IFormFile image, string bookId);
        Task<bool> SoftDeleteBookAsync(int id, int userId);  // Log soft delete
        Task<List<Book>> GetFavoriteBooksAsync(int userId);  // User-specific
        Task<bool> ToggleFavoriteAsync(int bookId, int userId);  // User-specific
        Task<List<object>> GetAllUsersAsync();  // For admin
        Task<BookFormViewModel?> GetCreateBookViewModelAsync();
        Task<BookFormViewModel?> GetEditBookViewModelAsync(int id);
        AdminViewModel GetQuickStats(int userId);  // Cached

        Task<BookListViewModel> GetPaginatedBooksAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null);
        string GetTimeAgo(DateTime time);
        IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload(DateTime? startDate = null, DateTime? endDate = null);  // Filtered
        IEnumerable<CategoryBookCountViewModel> BooksByCategory();
        IEnumerable<AuthorBookCountViewModel> BooksByAuthor();
        FavoriteStatsViewModel FavoriteStats();
        Task<List<SelectListItem>> GetCategoriesAsync();
        int GetTotalBooks();
        int GetTotalCategories();
    }
}