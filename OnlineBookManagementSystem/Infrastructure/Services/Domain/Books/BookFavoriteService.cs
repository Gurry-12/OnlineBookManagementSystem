using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Application.Mappings;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Books;

public class BookFavoriteService : IBookFavoriteService
{
    private readonly BookManagementContext _context;
    private readonly IActivityLogger _activityLogger;
    private readonly ILogger<BookFavoriteService> _logger;

    public BookFavoriteService(
        BookManagementContext context,
        IActivityLogger activityLogger,
        ILogger<BookFavoriteService> logger)
    {
        _context = context;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<List<BookDto>> GetUserFavoritesAsync(int userId)
    {
        try
        {
            var favorites = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Category)
                .Select(f => f.Book)
                .Where(b => !b.IsDeleted)
                .ToListAsync();

            return favorites.Select(b => b.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites for user {UserId}", userId);
            return new List<BookDto>();
        }
    }

    public async Task<bool> ToggleFavoriteAsync(int userId, int bookId)
    {
        try
        {
            var existingFavorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (existingFavorite != null)
            {
                // Remove favorite
                _context.UserFavorites.Remove(existingFavorite);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    "Favorite",
                    $"Removed book {bookId} from favorites", userId);

                return false; // Removed
            }
            else
            {
                // Add favorite
                var favorite = new UserFavorite
                {
                    UserId = userId,
                    BookId = bookId,
                    AddedAt = DateTime.UtcNow
                };

                _context.UserFavorites.Add(favorite);
                await _context.SaveChangesAsync();

                await _activityLogger.LogActivityAsync(
                    "Favorite",
                    $"Added book {bookId} to favorites", userId);

                return true; // Added
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite for user {UserId}, book {BookId}", userId, bookId);
            throw;
        }
    }

    public async Task<bool> IsFavoriteAsync(int userId, int bookId)
    {
        try
        {
            return await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking favorite status for user {UserId}, book {BookId}", userId, bookId);
            return false;
        }
    }

    public async Task<int> GetFavoriteCountAsync(int bookId)
    {
        try
        {
            return await _context.UserFavorites
                .CountAsync(f => f.BookId == bookId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite count for book {BookId}", bookId);
            return 0;
        }
    }

    public async Task<List<BookDto>> GetTopFavoritedBooksAsync(int count = 10)
    {
        try
        {
            var topBooks = await _context.UserFavorites
                .GroupBy(f => f.Book)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .Where(b => !b.IsDeleted)
                .Include(b => b.Category)
                .ToListAsync();

            return topBooks.Select(b => b.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top favorited books");
            return new List<BookDto>();
        }
    }

    // Additional methods for backward compatibility with existing controllers
    public async Task<List<Book>> GetUserFavoriteBooksAsync(int userId)
    {
        try
        {
            return await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Category)
                .Select(f => f.Book)
                .Where(b => !b.IsDeleted)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite books for user {UserId}", userId);
            return new List<Book>();
        }
    }

    public async Task<(bool Success, string Message, bool IsFavorite)> ToggleUserFavoriteAsync(int bookId, int userId)
    {
        try
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && !b.IsDeleted);
            if (book == null)
                return (false, "Book not found", false);

            var existingFavorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

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
                    AddedAt = DateTime.UtcNow
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

    public async Task<int> GetUserFavoritesCountAsync(int userId)
    {
        try
        {
            return await _context.UserFavorites.CountAsync(f => f.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites count for user {UserId}", userId);
            return 0;
        }
    }

    // Additional methods for user favorites controller
    public async Task<bool> AddToFavoritesAsync(int userId, int bookId)
    {
        try
        {
            var existingFavorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (existingFavorite != null)
                return false; // Already exists

            var favorite = new UserFavorite
            {
                UserId = userId,
                BookId = bookId,
                AddedAt = DateTime.UtcNow
            };

            _context.UserFavorites.Add(favorite);
            await _context.SaveChangesAsync();

            await _activityLogger.LogActivityAsync(
                "Favorite",
                $"Added book {bookId} to favorites", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book {BookId} to favorites for user {UserId}", bookId, userId);
            return false;
        }
    }

    public async Task<bool> RemoveFromFavoritesAsync(int userId, int bookId)
    {
        try
        {
            var existingFavorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (existingFavorite == null)
                return false; // Not found

            _context.UserFavorites.Remove(existingFavorite);
            await _context.SaveChangesAsync();

            await _activityLogger.LogActivityAsync(
                "Favorite",
                $"Removed book {bookId} from favorites", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing book {BookId} from favorites for user {UserId}", bookId, userId);
            return false;
        }
    }

    public async Task<bool> IsBookFavoriteAsync(int userId, int bookId)
    {
        try
        {
            return await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if book {BookId} is favorite for user {UserId}", bookId, userId);
            return false;
        }
    }

    public async Task<PagedBooksDto> GetUserFavoriteBooksPagedAsync(int userId, int page, int pageSize)
    {
        try
        {
            var query = _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Category)
                .Select(f => f.Book)
                .Where(b => !b.IsDeleted);

            var totalBooks = await query.CountAsync();
            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedBooksDto
            {
                Books = books.Select(b => b.ToDto()).ToList(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalBooks / (double)pageSize),
                TotalBooks = totalBooks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged favorite books for user {UserId}", userId);
            return new PagedBooksDto
            {
                Books = new List<BookDto>(),
                CurrentPage = page,
                TotalPages = 0,
                TotalBooks = 0
            };
        }
    }
}
