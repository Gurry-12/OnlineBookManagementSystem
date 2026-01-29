using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.UseCases.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers.Api
{
    /// <summary>
    /// API controller for advanced book search functionality following Clean Architecture.
    /// Demonstrates proper use case implementation and API design patterns.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "UserOrHigher")]
    public class AdvancedBookSearchController : BaseController
    {
        private readonly IAdvancedBookSearchUseCase _advancedBookSearchUseCase;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<AdvancedBookSearchController> _logger;

        public AdvancedBookSearchController(
            IAdvancedBookSearchUseCase advancedBookSearchUseCase,
            IActivityLogger activityLogger,
            ILogger<AdvancedBookSearchController> logger)
        {
            _advancedBookSearchUseCase = advancedBookSearchUseCase;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        /// <summary>
        /// Performs advanced book search with multiple criteria and filters.
        /// </summary>
        /// <param name="searchDto">Search criteria and filters</param>
        /// <returns>Paginated search results</returns>
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] AdvancedBookSearchDto searchDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized(new { message = "User not authenticated" });

            try
            {
                // Validate search parameters
                if (searchDto.Page < 1) searchDto.Page = 1;
                if (searchDto.PageSize < 1 || searchDto.PageSize > 100) searchDto.PageSize = 20;

                var result = await _advancedBookSearchUseCase.ExecuteAsync(searchDto);

                // Log search activity
                var searchTerms = new List<string>();
                if (!string.IsNullOrEmpty(searchDto.Title)) searchTerms.Add($"title:{searchDto.Title}");
                if (!string.IsNullOrEmpty(searchDto.Author)) searchTerms.Add($"author:{searchDto.Author}");
                if (!string.IsNullOrEmpty(searchDto.ISBN)) searchTerms.Add($"isbn:{searchDto.ISBN}");
                if (searchDto.CategoryId.HasValue) searchTerms.Add($"category:{searchDto.CategoryId}");
                
                var searchDescription = string.Join(", ", searchTerms);
                await _activityLogger.LogAsync("AdvancedBookSearch", $"Advanced search: {searchDescription}", userId);

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = $"Found {result.TotalCount} books matching your criteria"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid search parameters for user {UserId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced book search for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred while searching books" });
            }
        }

        /// <summary>
        /// Gets search suggestions based on partial input.
        /// </summary>
        /// <param name="query">Partial search query</param>
        /// <param name="type">Type of suggestion (title, author, isbn)</param>
        /// <param name="limit">Maximum number of suggestions</param>
        /// <returns>List of search suggestions</returns>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions(
            [FromQuery] string query, 
            [FromQuery] string type = "title", 
            [FromQuery] int limit = 10)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized(new { message = "User not authenticated" });

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Ok(new { success = true, suggestions = new List<object>() });
            }

            try
            {
                var searchDto = new AdvancedBookSearchDto
                {
                    Page = 1,
                    PageSize = Math.Min(limit, 20)
                };

                // Set search criteria based on type
                switch (type.ToLower())
                {
                    case "author":
                        searchDto.Author = query;
                        break;
                    case "isbn":
                        searchDto.ISBN = query;
                        break;
                    default:
                        searchDto.Title = query;
                        break;
                }

                var result = await _advancedBookSearchUseCase.ExecuteAsync(searchDto);
                
                var suggestions = result.Books.Select(book => new
                {
                    id = book.Id,
                    title = book.Title,
                    author = book.Author,
                    isbn = book.ISBN,
                    price = book.Price,
                    imageUrl = book.ImageUrl
                }).ToList();

                return Ok(new { success = true, suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for query '{Query}' and user {UserId}", query, userId);
                return StatusCode(500, new { success = false, message = "Failed to load suggestions" });
            }
        }

        /// <summary>
        /// Gets available filter options for advanced search.
        /// </summary>
        /// <returns>Available categories, price ranges, and other filter options</returns>
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilterOptions()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized(new { message = "User not authenticated" });

            try
            {
                // This would typically get filter options from the use case or a dedicated service
                // For now, we'll return some sample filter options
                var filterOptions = new
                {
                    categories = new[]
                    {
                        new { id = 1, name = "Fiction" },
                        new { id = 2, name = "Non-Fiction" },
                        new { id = 3, name = "Science" },
                        new { id = 4, name = "Technology" },
                        new { id = 5, name = "History" }
                    },
                    priceRanges = new[]
                    {
                        new { min = 0m, max = (decimal?)10m, label = "Under $10" },
                        new { min = 10m, max = (decimal?)25m, label = "$10 - $25" },
                        new { min = 25m, max = (decimal?)50m, label = "$25 - $50" },
                        new { min = 50m, max = (decimal?)100m, label = "$50 - $100" },
                        new { min = 100m, max = (decimal?)null, label = "Over $100" }
                    },
                    sortOptions = new[]
                    {
                        new { value = "title", label = "Title A-Z" },
                        new { value = "title_desc", label = "Title Z-A" },
                        new { value = "price", label = "Price Low-High" },
                        new { value = "price_desc", label = "Price High-Low" },
                        new { value = "rating", label = "Rating Low-High" },
                        new { value = "rating_desc", label = "Rating High-Low" },
                        new { value = "newest", label = "Newest First" },
                        new { value = "oldest", label = "Oldest First" }
                    }
                };

                return Ok(new { success = true, data = filterOptions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filter options for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Failed to load filter options" });
            }
        }

        /// <summary>
        /// Performs a quick search with minimal parameters.
        /// </summary>
        /// <param name="q">Search query</param>
        /// <param name="page">Page number</param>
        /// <param name="size">Page size</param>
        /// <returns>Quick search results</returns>
        [HttpGet("quick")]
        public async Task<IActionResult> QuickSearch(
            [FromQuery] string q, 
            [FromQuery] int page = 1, 
            [FromQuery] int size = 12)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized(new { message = "User not authenticated" });

            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { success = false, message = "Search query is required" });
            }

            try
            {
                var searchDto = new AdvancedBookSearchDto
                {
                    Title = q, // Search in title by default
                    Page = Math.Max(1, page),
                    PageSize = Math.Min(Math.Max(1, size), 50),
                    SortBy = "relevance"
                };

                var result = await _advancedBookSearchUseCase.ExecuteAsync(searchDto);

                await _activityLogger.LogAsync("QuickSearch", $"Quick search: {q}", userId);

                return Ok(new
                {
                    success = true,
                    data = result,
                    query = q
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing quick search for query '{Query}' and user {UserId}", q, userId);
                return StatusCode(500, new { success = false, message = "Search failed" });
            }
        }
    }
}