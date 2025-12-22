using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.DTOs;

namespace OnlineBookManagementSystem.Services.Common
{
    public interface IBookManager
    {
        Task<string?> SaveImageAsync(IFormFile image, string bookId);
        void DeleteImage(string imageUrl);
        BookDto MapToDto(Book book);
        Book MapToEntity(CreateBookDto dto);
        void UpdateEntity(Book book, UpdateBookDto dto);
    }
}
