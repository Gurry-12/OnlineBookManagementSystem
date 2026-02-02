using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books
{
    /// <summary>
    /// Service interface for book write operations and commands
    /// Follows SRP - Only handles book CRUD operations
    /// </summary>
    public interface IBookCommandService
    {
        // Book CRUD operations
        Task<bool> AddBookAsync(Book bookData, IFormFile? imageFile = null);
        Task<bool> UpdateBookAsync(Book bookData, IFormFile? imageFile = null);
        Task<bool> SoftDeleteBookAsync(int id, int userId);

        // Image handling
        Task<string?> SaveImageAsync(IFormFile image, string bookId);
    }
}
