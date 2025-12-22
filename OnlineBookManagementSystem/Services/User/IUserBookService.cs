using OnlineBookManagementSystem.Models.DTOs;

namespace OnlineBookManagementSystem.Services.User
{
    public interface IUserBookService
    {
        Task<List<BookDto>> GetBooksAsync(int page, int pageSize, string? search, int? categoryId, string? sortBy);
        Task<bool> ToggleFavoriteAsync(int bookId, int userId);
        Task<List<BookDto>> GetFavoritesAsync(int userId);
    }
}
