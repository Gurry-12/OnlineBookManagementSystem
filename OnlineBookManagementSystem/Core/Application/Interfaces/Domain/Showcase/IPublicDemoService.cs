using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.Showcase;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Showcase
{
    /// <summary>
    /// Service for providing read-only data access for public demo functionality
    /// </summary>
    public interface IPublicDemoService
    {
        /// <summary>
        /// Gets featured books for the showcase (read-only)
        /// </summary>
        /// <param name="count">Number of books to retrieve</param>
        /// <returns>Featured books view model</returns>
        Task<BookListViewModel> GetFeaturedBooksAsync(int count = 8);

        /// <summary>
        /// Gets categories with book counts for public display
        /// </summary>
        /// <returns>List of categories with counts</returns>
        Task<List<CategoryWithCountViewModel>> GetCategoriesWithCountsAsync();

        /// <summary>
        /// Searches books with read-only constraints
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Search results view model</returns>
        Task<BookListViewModel> SearchBooksAsync(string query, int page = 1, int pageSize = 12);

        /// <summary>
        /// Gets book details for public viewing
        /// </summary>
        /// <param name="bookId">Book identifier</param>
        /// <returns>Book details view model</returns>
        Task<BookDetailsViewModel?> GetBookDetailsAsync(int bookId);

        /// <summary>
        /// Gets system statistics for showcase display
        /// </summary>
        /// <returns>System statistics view model</returns>
        Task<SystemStatisticsViewModel> GetSystemStatisticsAsync();

        /// <summary>
        /// Gets books by category for public browsing
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Books in category view model</returns>
        Task<BookListViewModel> GetBooksByCategoryAsync(int categoryId, int page = 1, int pageSize = 12);

        /// <summary>
        /// Gets showcase content for portfolio display
        /// </summary>
        /// <returns>Complete showcase view model</returns>
        Task<ShowcaseViewModel> GetShowcaseContentAsync();

        /// <summary>
        /// Gets technical highlights for architecture showcase
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <returns>List of technical highlights</returns>
        Task<List<OnlineBookManagementSystem.Core.Domain.Entities.TechnicalHighlight>> GetTechnicalHighlightsAsync(string? category = null);

        /// <summary>
        /// Gets feature showcases for demonstration
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <returns>List of feature showcases</returns>
        Task<List<OnlineBookManagementSystem.Core.Domain.Entities.FeatureShowcase>> GetFeatureShowcasesAsync(string? category = null);

        /// <summary>
        /// Gets performance metrics for technical showcase
        /// </summary>
        /// <returns>Performance statistics</returns>
        Task<PerformanceStatsViewModel> GetPerformanceMetricsAsync();
    }
}