using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;

namespace OnlineBookManagementSystem.Core.Application.UseCases.Books
{
    public interface ISearchBooksUseCase
    {
        Task<PagedBooksDto> ExecuteAsync(BookSearchDto searchDto, CancellationToken cancellationToken = default);
    }

    public class SearchBooksUseCase : ISearchBooksUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchBooksUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<PagedBooksDto> ExecuteAsync(BookSearchDto searchDto, CancellationToken cancellationToken = default)
        {
            var (books, totalCount) = await _unitOfWork.Books.GetPagedBooksAsync(
                searchDto.Page,
                searchDto.PageSize,
                searchDto.SearchTerm,
                searchDto.CategoryId,
                searchDto.SortBy,
                cancellationToken);

            // Get category names for books that have categories
            var categoryIds = books.Where(b => b.CategoryId.HasValue)
                                  .Select(b => b.CategoryId!.Value)
                                  .Distinct()
                                  .ToList();

            var categories = new Dictionary<int, string>();
            if (categoryIds.Any())
            {
                var categoryEntities = await _unitOfWork.Categories.FindAsync(
                    c => categoryIds.Contains(c.Id) && !c.IsDeleted, 
                    cancellationToken);
                
                categories = categoryEntities.ToDictionary(c => c.Id, c => c.Name);
            }

            // Convert to DTOs
            var bookDtos = books.Select(book => new BookDto(
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
                book.CategoryId.HasValue && categories.ContainsKey(book.CategoryId.Value) 
                    ? categories[book.CategoryId.Value] 
                    : null,
                book.IsFeatured,
                book.AverageRating,
                book.IsAvailable,
                book.IsLowStock,
                book.CreatedAt,
                book.UpdatedAt
            ));

            var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

            return new PagedBooksDto(
                bookDtos,
                totalCount,
                searchDto.Page,
                searchDto.PageSize,
                totalPages
            );
        }
    }
}