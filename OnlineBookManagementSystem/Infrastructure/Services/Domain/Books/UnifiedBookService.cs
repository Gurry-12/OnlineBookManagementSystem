using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Reviews;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Application.Mappings;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Books;

/// <summary>
/// Unified Book Service Implementation - Single service handling all roles
/// Eliminates separate Admin/User/Public book services following DRY principle
/// </summary>
public class UnifiedBookService : IUnifiedBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookFavoriteService _favoriteService;
    private readonly IReviewService _reviewService;
    private readonly ILogger<UnifiedBookService> _logger;

    public UnifiedBookService(
        IBookRepository bookRepository,
        IBookFavoriteService favoriteService,
        IReviewService reviewService,
        ILogger<UnifiedBookService> logger)
    {
        _bookRepository = bookRepository;
        _favoriteService = favoriteService;
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Get book details with role-based context - Single method for all roles
    /// </summary>
    public async Task<BookDetailsViewModel> GetBookDetailsAsync(int bookId, int? userId = null, string? userRole = null)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null) throw new ArgumentException("Book not found", nameof(bookId));

        var viewModel = book.ToDetailsViewModel();

        // Set capability-based context
        viewModel.Capabilities = BuildCapabilities(userId, userRole);

        // Load user-specific data if authenticated
        if (userId.HasValue && userId > 0)
        {
            viewModel.IsFavorite = await _favoriteService.IsFavoriteAsync(userId.Value, bookId);
            viewModel.CanReview = await CanUserReviewBookAsync(bookId, userId.Value);
        }

        // Load rating data
        var rating = await _reviewService.GetBookRatingAsync(bookId);
        viewModel.Rating = rating;

        // Set admin-only properties


        return viewModel;
    }

    /// <summary>
    /// Get paginated books with role-based filtering
    /// </summary>
    public async Task<PagedBooksDto> GetBooksAsync(int page, int pageSize, string? search = null,
        int? categoryId = null, string? sortBy = null, decimal? minPrice = null,
        decimal? maxPrice = null, int? userId = null, string? userRole = null)
    {
        var (books, totalCount) = await _bookRepository.GetPagedBooksAsync(
            page, pageSize, search, categoryId, sortBy);

        var bookDtos = books.Select(b => b.ToDto()).ToList();

        var pagedResult = new PagedBooksDto
        {
            Books = bookDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        // Add user-specific data if needed
        if (userId.HasValue && userId > 0)
        {
            foreach (var book in pagedResult.Books)
            {
                // This would need to be optimized with a batch query in production
                book.IsFavorite = await _favoriteService.IsFavoriteAsync(userId.Value, book.Id);
            }
        }

        return pagedResult;
    }

    /// <summary>
    /// Create book - Admin/SuperAdmin only
    /// </summary>
    public async Task<BookDetailsViewModel> CreateBookAsync(CreateBookDto createBookDto, int userId, string userRole)
    {
        if (!CanUserEditBookAsync(0, userId, userRole).Result)
            throw new UnauthorizedAccessException("Insufficient permissions to create books");

        // This would need proper implementation with the actual Book entity creation
        // For now, return a placeholder
        throw new NotImplementedException("Book creation not yet implemented in unified service");
    }

    /// <summary>
    /// Update book - Admin/SuperAdmin only
    /// </summary>
    public async Task<BookDetailsViewModel> UpdateBookAsync(int bookId, CreateBookDto updateBookDto, int userId, string userRole)
    {
        if (!await CanUserEditBookAsync(bookId, userId, userRole))
            throw new UnauthorizedAccessException("Insufficient permissions to edit this book");

        // This would need proper implementation with the actual Book entity update
        // For now, return a placeholder
        throw new NotImplementedException("Book update not yet implemented in unified service");
    }

    /// <summary>
    /// Delete book - Admin/SuperAdmin only
    /// </summary>
    public async Task<bool> DeleteBookAsync(int bookId, int userId, string userRole)
    {
        if (!await CanUserDeleteBookAsync(bookId, userId, userRole))
            throw new UnauthorizedAccessException("Insufficient permissions to delete this book");

        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null) return false;

        // Soft delete
        book.IsDeleted = true;
        // Note: UpdatedBy and UpdatedAt would need to be added to the Book entity
        // book.UpdatedBy = userId;
        // book.UpdatedAt = DateTime.UtcNow;

        await _bookRepository.UpdateAsync(book);
        return true;
    }

    /// <summary>
    /// Toggle favorite - User or higher
    /// </summary>
    public async Task<bool> ToggleFavoriteAsync(int bookId, int userId)
    {
        return await _favoriteService.ToggleFavoriteAsync(userId, bookId);
    }

    /// <summary>
    /// Get user's favorite book IDs
    /// </summary>
    public async Task<List<int>> GetUserFavoriteBookIdsAsync(int userId)
    {
        var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
        return favorites.Select(f => f.Id).ToList();
    }

    /// <summary>
    /// Get book analytics based on user role
    /// </summary>
    public async Task<Dictionary<string, object>> GetBookAnalyticsAsync(int bookId, string userRole)
    {
        var analytics = new Dictionary<string, object>();

        // Basic analytics for all roles
        var rating = await _reviewService.GetBookRatingAsync(bookId);
        analytics["averageRating"] = rating.AverageRating;
        analytics["totalReviews"] = rating.TotalReviews;

        // Extended analytics for admin roles
        if (userRole == "Admin" || userRole == "SuperAdmin")
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            analytics["viewCount"] = 0; // Would need to implement view tracking
            analytics["purchaseCount"] = await GetBookPurchaseCountAsync(bookId);
            analytics["favoriteCount"] = 0; // Would need to implement favorite counting
        }

        // System analytics for SuperAdmin only
        if (userRole == "SuperAdmin")
        {
            analytics["revenueGenerated"] = await GetBookRevenueAsync(bookId);
            analytics["conversionRate"] = await GetBookConversionRateAsync(bookId);
        }

        return analytics;
    }

    /// <summary>
    /// Check if user can edit book
    /// </summary>
    public async Task<bool> CanUserEditBookAsync(int bookId, int userId, string userRole)
    {
        if (userRole == "SuperAdmin") return true;
        if (userRole == "Admin") return true;

        // Regular users cannot edit books
        return false;
    }

    /// <summary>
    /// Check if user can delete book
    /// </summary>
    public async Task<bool> CanUserDeleteBookAsync(int bookId, int userId, string userRole)
    {
        if (userRole == "SuperAdmin") return true;

        // Only SuperAdmin can delete books for safety
        return false;
    }

    /// <summary>
    /// Check if user can review book
    /// </summary>
    public async Task<bool> CanUserReviewBookAsync(int bookId, int userId)
    {
        // User must be authenticated
        if (userId <= 0) return false;

        // Check if user already reviewed this book using the review service
        try
        {
            var existingReviews = await _reviewService.GetBookReviewsAsync(bookId, 1, 100, ReviewSortOrder.NewestFirst, null);
            return !existingReviews.Items.Any(r => r.UserId == userId);
        }
        catch
        {
            // If there's an error, assume they can review
            return true;
        }
    }

    /// <summary>
    /// Build capabilities context for ViewModels based on user role
    /// </summary>
    private BookDetailsCapabilities BuildCapabilities(int? userId, string? userRole)
    {
        userRole ??= "Public";
        var isAuthenticated = userId.HasValue && userId > 0;

        return new BookDetailsCapabilities
        {
            // View Capabilities
            CanView = true,
            CanViewTechnicalDetails = userRole == "Public",
            CanViewMetadata = userRole == "Admin" || userRole == "SuperAdmin",

            // Action Capabilities
            CanEdit = userRole == "Admin" || userRole == "SuperAdmin",
            CanDelete = userRole == "SuperAdmin",
            CanAddToCart = userRole == "User",
            CanFavorite = isAuthenticated && userRole != "Public",
            CanReview = userRole == "User",

            // Navigation Capabilities
            CanViewReviews = true,
            CanModerateReviews = userRole == "Admin" || userRole == "SuperAdmin",

            // UI Context
            IsAuthenticated = isAuthenticated,
            BackLinkText = userRole switch
            {
                "Admin" or "SuperAdmin" => "Back to Admin Books",
                "User" => "Back to My Books",
                _ => "Back to Browse"
            },
            BackLinkUrl = userRole switch
            {
                "Admin" or "SuperAdmin" => "/Admin/Books",
                "User" => "/User/UserBookList",
                _ => "/Public/Browse"
            },
            PageTitle = userRole == "Public" ? "Book Showcase" : "Book Details",
            LayoutClass = userRole switch
            {
                "Admin" or "SuperAdmin" => "admin-layout",
                "User" => "user-layout",
                _ => "public-layout"
            }
        };
    }

    /// <summary>
    /// Helper method to get book purchase count
    /// </summary>
    private async Task<int> GetBookPurchaseCountAsync(int bookId)
    {
        // Implementation would depend on your Order/OrderDetail repository
        // This is a placeholder
        return 0;
    }

    /// <summary>
    /// Helper method to get book revenue
    /// </summary>
    private async Task<decimal> GetBookRevenueAsync(int bookId)
    {
        // Implementation would depend on your Order/OrderDetail repository
        // This is a placeholder
        return 0m;
    }

    /// <summary>
    /// Helper method to get book conversion rate
    /// </summary>
    private async Task<double> GetBookConversionRateAsync(int bookId)
    {
        // Implementation would calculate views vs purchases
        // This is a placeholder
        return 0.0;
    }


}