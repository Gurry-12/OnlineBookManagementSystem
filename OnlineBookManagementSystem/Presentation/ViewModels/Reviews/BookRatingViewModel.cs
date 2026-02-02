namespace OnlineBookManagementSystem.Presentation.ViewModels.Reviews
{
    public class BookRatingViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }

        public Dictionary<int, int> RatingDistribution => new()
        {
            { 5, FiveStarCount },
            { 4, FourStarCount },
            { 3, ThreeStarCount },
            { 2, TwoStarCount },
            { 1, OneStarCount }
        };

        public string FormattedRating => AverageRating.ToString("F1");
        public string RatingStars => new string('★', (int)Math.Round(AverageRating)) +
                                    new string('☆', 5 - (int)Math.Round(AverageRating));

        // User-specific properties
        public bool HasUserReview { get; set; }
        public ReviewDisplayViewModel? UserReview { get; set; }
    }
}