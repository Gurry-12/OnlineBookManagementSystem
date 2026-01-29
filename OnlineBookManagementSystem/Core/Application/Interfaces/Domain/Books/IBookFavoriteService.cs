using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;

public interface IBookFavoriteService
{
    Task<List<BookDto>> GetUserFavoritesAsync(int userId);
    Task<bool> ToggleFavoriteAsync(int userId, int bookId);
    Task<bool> IsFavoriteAsync(int userId, int bookId);
    Task<int> GetFavoriteCountAsync(int bookId);
    Task<List<BookDto>> GetTopFavoritedBooksAsync(int count = 10);
    
    // Additional methods for backward compatibility with existing controllers
    Task<List<Book>> GetUserFavoriteBooksAsync(int userId);
    Task<(bool Success, string Message, bool IsFavorite)> ToggleUserFavoriteAsync(int bookId, int userId);
    Task<int> GetUserFavoritesCountAsync(int userId);
    
    // Additional methods for user favorites controller
    Task<bool> AddToFavoritesAsync(int userId, int bookId);
    Task<bool> RemoveFromFavoritesAsync(int userId, int bookId);
    Task<bool> IsBookFavoriteAsync(int userId, int bookId);
    Task<PagedBooksDto> GetUserFavoriteBooksPagedAsync(int userId, int page, int pageSize);
}
