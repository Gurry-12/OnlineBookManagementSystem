using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;

namespace OnlineBookManagementSystem.Core.Application.UseCases.Books
{
    public interface IAdvancedBookSearchUseCase
    {
        Task<PagedBooksDto> ExecuteAsync(AdvancedBookSearchDto searchDto, CancellationToken cancellationToken = default);
    }

    public class AdvancedBookSearchUseCase : IAdvancedBookSearchUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdvancedBookSearchUseCase> _logger;

        public AdvancedBookSearchUseCase(IUnitOfWork unitOfWork, ILogger<AdvancedBookSearchUseCase> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedBooksDto> ExecuteAsync(AdvancedBookSearchDto searchDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Executing advanced book search with criteria: {@SearchCriteria}", searchDto);

                // Build search predicate
                var predicate = BuildSearchPredicate(searchDto);

                // Get total count
                var totalCount = await _unitOfWork.Books.CountAsync(predicate, cancellationToken);

                // Get books with pagination
                var books = await _unitOfWork.Books.GetByConditionAsync(predicate, cancellationToken);
                
                // Apply sorting
                books = ApplySorting(books, searchDto.SortBy, searchDto.SortDirection);
                
                // Apply pagination
                var pagedBooks = books
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToList();

                // Convert to DTOs
                var bookDtos = await ConvertToDtosAsync(pagedBooks, cancellationToken);

                var result = new PagedBooksDto
                {
                    Books = bookDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize),
                    HasNextPage = searchDto.Page * searchDto.PageSize < totalCount,
                    HasPreviousPage = searchDto.Page > 1
                };

                _logger.LogInformation("Advanced book search completed. Found {TotalCount} books, returning page {Page} of {TotalPages}", 
                    totalCount, searchDto.Page, result.TotalPages);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing advanced book search");
                throw;
            }
        }

        private static System.Linq.Expressions.Expression<Func<Book, bool>> BuildSearchPredicate(AdvancedBookSearchDto searchDto)
        {
            return book => 
                !book.IsDeleted &&
                (string.IsNullOrEmpty(searchDto.Title) || book.Title.Contains(searchDto.Title)) &&
                (string.IsNullOrEmpty(searchDto.Author) || book.Author.Contains(searchDto.Author)) &&
                (string.IsNullOrEmpty(searchDto.ISBN) || (book.ISBN != null && book.ISBN.Value.Contains(searchDto.ISBN))) &&
                (!searchDto.CategoryId.HasValue || book.CategoryId == searchDto.CategoryId) &&
                (!searchDto.MinPrice.HasValue || book.Price.Amount >= searchDto.MinPrice) &&
                (!searchDto.MaxPrice.HasValue || book.Price.Amount <= searchDto.MaxPrice) &&
                (!searchDto.MinRating.HasValue || book.AverageRating >= searchDto.MinRating) &&
                (!searchDto.InStockOnly || book.StockQuantity > 0) &&
                (!searchDto.FeaturedOnly || book.IsFeatured) &&
                (!searchDto.PublishedAfter.HasValue || (book.PublicationDate.HasValue && book.PublicationDate >= searchDto.PublishedAfter)) &&
                (!searchDto.PublishedBefore.HasValue || (book.PublicationDate.HasValue && book.PublicationDate <= searchDto.PublishedBefore));
        }

        private static IEnumerable<Book> ApplySorting(IEnumerable<Book> books, string? sortBy, string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.ToLowerInvariant() switch
            {
                "title" => isDescending ? books.OrderByDescending(b => b.Title) : books.OrderBy(b => b.Title),
                "author" => isDescending ? books.OrderByDescending(b => b.Author) : books.OrderBy(b => b.Author),
                "price" => isDescending ? books.OrderByDescending(b => b.Price.Amount) : books.OrderBy(b => b.Price.Amount),
                "rating" => isDescending ? books.OrderByDescending(b => b.AverageRating) : books.OrderBy(b => b.AverageRating),
                "publicationdate" => isDescending ? books.OrderByDescending(b => b.PublicationDate) : books.OrderBy(b => b.PublicationDate),
                "createdat" => isDescending ? books.OrderByDescending(b => b.CreatedAt) : books.OrderBy(b => b.CreatedAt),
                _ => books.OrderBy(b => b.Title) // Default sorting
            };
        }

        private async Task<List<BookDto>> ConvertToDtosAsync(List<Book> books, CancellationToken cancellationToken)
        {
            var bookDtos = new List<BookDto>();

            foreach (var book in books)
            {
                // Get category name if needed
                string? categoryName = null;
                if (book.CategoryId.HasValue)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(book.CategoryId.Value, cancellationToken);
                    categoryName = category?.Name;
                }

                var bookDto = new BookDto(
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

                bookDtos.Add(bookDto);
            }

            return bookDtos;
        }
    }

    // Advanced search DTO
    public class AdvancedBookSearchDto
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinRating { get; set; }
        public bool InStockOnly { get; set; }
        public bool FeaturedOnly { get; set; }
        public DateTime? PublishedAfter { get; set; }
        public DateTime? PublishedBefore { get; set; }
        public string? SortBy { get; set; } = "title";
        public string? SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}