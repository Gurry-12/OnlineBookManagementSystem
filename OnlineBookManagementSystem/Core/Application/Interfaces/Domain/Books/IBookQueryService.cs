using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.User;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books
{
    /// <summary>
    /// Service interface for book read operations and queries
    /// Follows SRP - Only handles book queries, not analytics or favorites
    /// </summary>
    public interface IBookQueryService
    {
        // Basic book queries
        Task<List<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<int> GetTotalBooksCountAsync();
        Task<int> GetTotalBooks();
        Task<int> GetTotalCategories();

        // Paginated and filtered queries
        Task<BookListViewModel> GetPaginatedBooksAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, bool? inStock = null);
        Task<BookListViewModel> GetBooksForUserAsync(int page, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, int? userId = null);
        Task<BookListViewModel> SearchBooksAsync(string query, int page, int pageSize, int? userId = null);
        Task<BookListViewModel> GetBooksByCategoryAsync(int categoryId, int page, int pageSize, int? userId = null);

        // Book details and recommendations
        Task<BookDetailsViewModel?> GetBookDetailsForUserAsync(int bookId, int userId);
        Task<List<Book>> GetPersonalizedRecommendationsAsync(int userId, int count);
        Task<List<Book>> GetFeaturedBooksAsync(int count);
        Task<List<Book>> GetNewArrivalsAsync(int count, int? userId = null);

        // ViewModels for forms
        Task<BookFormViewModel?> GetCreateBookViewModelAsync();
        Task<BookFormViewModel?> GetEditBookViewModelAsync(int id);
        Task<List<SelectListItem>> GetCategoriesAsync();

        // User profile related
        Task<UserProfileViewModel?> GetUserProfileAsync(int userId);

        // Favorites (for backward compatibility)
        Task<List<Book>> GetFavoriteBooksAsync(int userId);

        // Utility methods
        string GetTimeAgo(DateTime time);
        Task<List<object>> GetAllUsersAsync();
        
        // Additional methods for book browsing controller
        Task<List<Book>> GetBookSuggestionsAsync(string query, int count = 10);
    }
}
