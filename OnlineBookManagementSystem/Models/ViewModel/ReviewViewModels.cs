using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class ReviewSubmissionViewModel
    {
        public int BookId { get; set; }
        
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 1000 characters")]
        public string ReviewText { get; set; } = string.Empty;
    }

    public class ReviewDisplayViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool CanEdit { get; set; }
        public bool IsEdited { get; set; }
    }

    public class BookRatingViewModel
    {
        public int BookId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public bool HasUserReview { get; set; }
        public ReviewDisplayViewModel? UserReview { get; set; }
    }

    public class ReviewModerationViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public ReviewStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class ReviewAnalyticsViewModel
    {
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int ApprovedReviews { get; set; }
        public int RejectedReviews { get; set; }
        public int FlaggedReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public List<ReviewTrendViewModel> ReviewTrends { get; set; } = new();
    }

    public class ReviewTrendViewModel
    {
        public DateTime Date { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class BookRatingStatsViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}