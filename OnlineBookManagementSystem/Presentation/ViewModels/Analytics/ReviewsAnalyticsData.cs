namespace OnlineBookManagementSystem.Presentation.ViewModels.Analytics;

public class ReviewsAnalyticsData
{
    public List<ReviewTrendViewModel> ReviewTrends { get; set; } = new();
    public List<RatingDistributionViewModel> RatingDistribution { get; set; } = new();
    public List<ReviewModerationStats> ModerationStats { get; set; } = new();

    // Summary metrics
    public int TotalReviews { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public int RejectedReviews { get; set; }
    public int FlaggedReviews { get; set; }
    public double AverageRating { get; set; }

    // Moderation metrics
    public int ReviewsThisMonth { get; set; }
    public double ApprovalRate { get; set; }
    public double RejectionRate { get; set; }
}

public class ReviewTrendViewModel
{
    public DateTime Date { get; set; }
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
}

public class RatingDistributionViewModel
{
    public int Rating { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ReviewModerationStats
{
    public DateTime Date { get; set; }
    public int PendingCount { get; set; }
    public int ProcessedCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
}