using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Interfaces.Repositories;
using OnlineBookManagementSystem.Models.DTOs;
using OnlineBookManagementSystem.Services.Common;

namespace OnlineBookManagementSystem.Services.User
{
    public class UserBookService : IUserBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBookManager _bookManager;
        private readonly IActivityLogger _activityLogger;

        public UserBookService(
            IBookRepository bookRepository,
            IBookManager bookManager,
            IActivityLogger activityLogger)
        {
            _bookRepository = bookRepository;
            _bookManager = bookManager;
            _activityLogger = activityLogger;
        }

        public async Task<List<BookDto>> GetBooksAsync(int page, int pageSize, string? search, int? categoryId, string? sortBy)
        {
             var books = await _bookRepository.GetPaginatedAsync(
                 (page - 1) * pageSize,
                 pageSize,
                 b => (string.IsNullOrEmpty(search) || b.Title.Contains(search) || b.Author.Contains(search)) &&
                      (!categoryId.HasValue || b.CategoryId == categoryId),
                 sortBy
             );

             return books.Select(b => _bookManager.MapToDto(b)).ToList();
        }

        public async Task<bool> ToggleFavoriteAsync(int bookId, int userId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null) return false;

            book.IsFavorite = !book.IsFavorite;
            await _bookRepository.SaveChangesAsync();
            await _activityLogger.LogAsync("FavoriteToggled", $"User toggled favorite for '{book.Title}'", userId);
            return true;
        }

        public async Task<List<BookDto>> GetFavoritesAsync(int userId)
        {
             var books = await _bookRepository.Query()
                 .Where(b => b.IsFavorite)
                 .ToListAsync();

             return books.Select(b => _bookManager.MapToDto(b)).ToList();
        }
    }
}
