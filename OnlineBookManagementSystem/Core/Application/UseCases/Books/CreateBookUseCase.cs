using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Exceptions;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;

namespace OnlineBookManagementSystem.Core.Application.UseCases.Books
{
    public interface ICreateBookUseCase
    {
        Task<BookDto> ExecuteAsync(CreateBookDto createBookDto, CancellationToken cancellationToken = default);
    }

    public class CreateBookUseCase : ICreateBookUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateBookUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<BookDto> ExecuteAsync(CreateBookDto createBookDto, CancellationToken cancellationToken = default)
        {
            // Validate category exists if provided
            if (createBookDto.CategoryId.HasValue)
            {
                var categoryExists = await _unitOfWork.Categories.ExistsAsync(
                    c => c.Id == createBookDto.CategoryId.Value && !c.IsDeleted, 
                    cancellationToken);
                
                if (!categoryExists)
                    throw new CategoryNotFoundException(createBookDto.CategoryId.Value);
            }

            // Create domain entity
            var price = new Money(createBookDto.Price);
            var book = new Book(createBookDto.Title, createBookDto.Author, price, createBookDto.CategoryId);

            // Set optional properties
            if (!string.IsNullOrWhiteSpace(createBookDto.ISBN))
            {
                book.SetISBN(createBookDto.ISBN);
            }

            if (createBookDto.PublicationDate.HasValue)
            {
                book.SetPublicationDate(createBookDto.PublicationDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(createBookDto.ImageUrl))
            {
                book.SetImageUrl(createBookDto.ImageUrl);
            }

            if (!string.IsNullOrWhiteSpace(createBookDto.Description))
            {
                book.UpdateBasicInfo(book.Title, book.Author, book.Price, createBookDto.Description);
            }

            book.UpdateStock(createBookDto.StockQuantity);
            book.SetLowStockThreshold(createBookDto.LowStockThreshold);
            book.SetFeatured(createBookDto.IsFeatured);

            // Save to repository
            await _unitOfWork.Books.AddAsync(book, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get category name for DTO
            string? categoryName = null;
            if (createBookDto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(createBookDto.CategoryId.Value, cancellationToken);
                categoryName = category?.Name;
            }

            // Return DTO
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