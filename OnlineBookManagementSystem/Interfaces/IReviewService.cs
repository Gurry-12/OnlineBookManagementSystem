using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IReviewService
    {
        // Review Management
        Task<(bool Success, string Message)> SubmitReviewAsync(int userId, int bookId, int rating, string reviewText);
        Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int userId, int rating, string reviewText);
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
        Task<BookReview?> GetUserReviewForBookAsync(int userId, int bookId);
        
        // Review Display
        Task<PaginatedResult<ReviewDisplayViewModel>> GetBookReviewsAsync(int bookId, int page, int pageSize, ReviewSortOrder sortOrder, int? ratingFilter);
        Task<ReviewDisplayViewModel?> GetReviewByIdAsync(int reviewId);
        
        // Rating Calculations
        Task<BookRatingViewModel> GetBookRatingAsync(int bookId);
        Task RecalculateBookRatingAsync(int bookId);
        Task InvalidateRatingCacheAsync(int bookId);
        
        // Moderation
        Task<PaginatedResult<ReviewModerationViewModel>> GetPendingReviewsAsync(int page, int pageSize);
        Task<bool> ApproveReviewAsync(int reviewId, int moderatorId);
        Task<bool> RejectReviewAsync(int reviewId, int moderatorId, string reason);
        Task<bool> FlagReviewAsync(int reviewId, int moderatorId, string reason);
        
        // Analytics
        Task<ReviewAnalyticsViewModel> GetReviewAnalyticsAsync();
        Task<List<BookRatingStatsViewModel>> GetTopRatedBooksAsync(int count);
        Task<List<BookRatingStatsViewModel>> GetLowestRatedBooksAsync(int count);
    }

    public enum ReviewSortOrder
    {
        NewestFirst,
        OldestFirst,
        HighestRating,
        LowestRating
    }
}