using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Domain.Exceptions;

namespace OnlineBookManagementSystem.Core.Application.UseCases.Books
{
    public interface IGetBookByIdUseCase
    {
        Task<BookDto> ExecuteAsync(int bookId, CancellationToken cancellationToken = default);
    }

    public class GetBookByIdUseCase : IGetBookByIdUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBookByIdUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<BookDto> ExecuteAsync(int bookId, CancellationToken cancellationToken = default)
        {
            var book = await _unitOfWork.Books.FirstOrDefaultAsync(
                b => b.Id == bookId && !b.IsDeleted, 
                cancellationToken);

            if (book == null)
                throw new BookNotFoundException(bookId);

            // Get category name
            string? categoryName = null;
            if (book.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(book.CategoryId.Value, cancellationToken);
                categoryName = category?.Name;
            }

            return new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Price.Amount,
                book.ISBN?.Value,
                book.PublicationDate,
                book.ImageUrl,
                book.StockQuantity,
                book.LowStockThreshold,
                book.Description,
                book.CategoryId,
                categoryName,
                book.IsFeatured,
                book.AverageRating,
                book.IsAvailable,
                book.IsLowStock,
                book.CreatedAt,
                book.UpdatedAt
            );
        }
    }
}