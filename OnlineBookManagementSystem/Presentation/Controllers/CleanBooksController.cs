using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.UseCases.Books;
using OnlineBookManagementSystem.Core.Domain.Exceptions;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CleanBooksController : ControllerBase
    {
        private readonly ICreateBookUseCase _createBookUseCase;
        private readonly IGetBookByIdUseCase _getBookByIdUseCase;
        private readonly ISearchBooksUseCase _searchBooksUseCase;
        private readonly ILogger<CleanBooksController> _logger;

        public CleanBooksController(
            ICreateBookUseCase createBookUseCase,
            IGetBookByIdUseCase getBookByIdUseCase,
            ISearchBooksUseCase searchBooksUseCase,
            ILogger<CleanBooksController> logger)
        {
            _createBookUseCase = createBookUseCase ?? throw new ArgumentNullException(nameof(createBookUseCase));
            _getBookByIdUseCase = getBookByIdUseCase ?? throw new ArgumentNullException(nameof(getBookByIdUseCase));
            _searchBooksUseCase = searchBooksUseCase ?? throw new ArgumentNullException(nameof(searchBooksUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get a book by ID
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Book details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> GetBook(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid book ID");

                var book = await _getBookByIdUseCase.ExecuteAsync(id, cancellationToken);
                return Ok(book);
            }
            catch (BookNotFoundException)
            {
                return NotFound($"Book with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book with ID {BookId}", id);
                return StatusCode(500, "An error occurred while retrieving the book");
            }
        }

        /// <summary>
        /// Search books with pagination and filtering
        /// </summary>
        /// <param name="searchTerm">Search term for title, author, or description</param>
        /// <param name="categoryId">Filter by category ID</param>
        /// <param name="minPrice">Minimum price filter</param>
        /// <param name="maxPrice">Maximum price filter</param>
        /// <param name="inStock">Filter by stock availability</param>
        /// <param name="sortBy">Sort order (title, author, price, rating, etc.)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of books</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedBooksDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedBooksDto>> SearchBooks(
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? inStock = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate parameters
                if (page <= 0)
                    return BadRequest("Page must be greater than 0");

                if (pageSize <= 0 || pageSize > 100)
                    return BadRequest("Page size must be between 1 and 100");

                if (minPrice.HasValue && minPrice < 0)
                    return BadRequest("Minimum price cannot be negative");

                if (maxPrice.HasValue && maxPrice < 0)
                    return BadRequest("Maximum price cannot be negative");

                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                    return BadRequest("Minimum price cannot be greater than maximum price");

                var searchDto = new BookSearchDto(searchTerm)
                {
                    CategoryId = categoryId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    SortBy = sortBy,
                    PageNumber = page,
                    PageSize = pageSize
                };

                var result = await _searchBooksUseCase.ExecuteAsync(searchDto, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books");
                return StatusCode(500, "An error occurred while searching books");
            }
        }

        /// <summary>
        /// Create a new book
        /// </summary>
        /// <param name="createBookDto">Book creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created book details</returns>
        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<BookDto>> CreateBook(
            [FromBody] CreateBookDto createBookDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var book = await _createBookUseCase.ExecuteAsync(createBookDto, cancellationToken);
                
                return CreatedAtAction(
                    nameof(GetBook),
                    new { id = book.Id },
                    book);
            }
            catch (CategoryNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book");
                return StatusCode(500, "An error occurred while creating the book");
            }
        }
    }
}
