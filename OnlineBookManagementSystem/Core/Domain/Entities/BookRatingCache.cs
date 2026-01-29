namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class BookRatingCache
    {
        private double _averageRating;

        public int BookId { get; set; }
        
        public double AverageRating 
        { 
            get => _averageRating;
            set
            {
                if (value < 0 || value > 5)
                    throw new ArgumentException("Average rating must be between 0 and 5", nameof(value));
                _averageRating = Math.Round(value, 2);
            }
        }

        public int TotalReviews { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Book Book { get; set; } = null!;

        // Private constructor for EF Core
        public BookRatingCache() { }

        public BookRatingCache(int bookId)
        {
            if (bookId <= 0)
                throw new ArgumentException("BookId must be positive", nameof(bookId));

            BookId = bookId;
            AverageRating = 0.0;
            TotalReviews = 0;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateRating(double averageRating, int totalReviews)
        {
            if (totalReviews < 0)
                throw new ArgumentException("Total reviews cannot be negative", nameof(totalReviews));

            AverageRating = averageRating;
            TotalReviews = totalReviews;
            LastUpdated = DateTime.UtcNow;
        }

        public void IncrementReview(int newRating)
        {
            if (newRating < 1 || newRating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(newRating));

            var totalRatingPoints = AverageRating * TotalReviews + newRating;
            TotalReviews++;
            AverageRating = totalRatingPoints / TotalReviews;
            LastUpdated = DateTime.UtcNow;
        }

        public void DecrementReview(int removedRating)
        {
            if (removedRating < 1 || removedRating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(removedRating));
            if (TotalReviews <= 0)
                throw new InvalidOperationException("Cannot decrement when no reviews exist");

            if (TotalReviews == 1)
            {
                AverageRating = 0.0;
                TotalReviews = 0;
            }
            else
            {
                var totalRatingPoints = AverageRating * TotalReviews - removedRating;
                TotalReviews--;
                AverageRating = totalRatingPoints / TotalReviews;
            }
            
            LastUpdated = DateTime.UtcNow;
        }

        public void RecalculateFromReviews(IEnumerable<BookReview> approvedReviews)
        {
            var reviews = approvedReviews.ToList();
            TotalReviews = reviews.Count;
            AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            LastUpdated = DateTime.UtcNow;
        }
    }
}