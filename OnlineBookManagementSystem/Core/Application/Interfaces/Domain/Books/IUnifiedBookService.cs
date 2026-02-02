using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;

/// <summary>
/// Unified Book Service Interface - Single service for all roles
/// Eliminates the need for separate Admin/User/Public book services
/// </summary>
public interface IUnifiedBookService
{
    // Generic book operations that adapt based on user role
    Task<BookDetailsViewModel> GetBookDetailsAsync(int bookId, int? userId = null, string? userRole = null);
    Task<PagedBooksDto> GetBooksAsync(int page, int pageSize, string? search = null,
        int? categoryId = null, string? sortBy = null, decimal? minPrice = null,
        decimal? maxPrice = null, int? userId = null, string? userRole = null);

    // Role-aware operations
    Task<bool> CanUserEditBookAsync(int bookId, int userId, string userRole);
    Task<bool> CanUserDeleteBookAsync(int bookId, int userId, string userRole);
    Task<bool> CanUserReviewBookAsync(int bookId, int userId);

    // Unified CRUD operations
    Task<BookDetailsViewModel> CreateBookAsync(CreateBookDto createBookDto, int userId, string userRole);
    Task<BookDetailsViewModel> UpdateBookAsync(int bookId, CreateBookDto updateBookDto, int userId, string userRole);
    Task<bool> DeleteBookAsync(int bookId, int userId, string userRole);

    // Analytics and favorites (role-aware)
    Task<bool> ToggleFavoriteAsync(int bookId, int userId);
    Task<List<int>> GetUserFavoriteBookIdsAsync(int userId);
    Task<Dictionary<string, object>> GetBookAnalyticsAsync(int bookId, string userRole);
}