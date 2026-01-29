using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class BookReview : BaseEntity
    {
        private int _rating;
        private string _reviewText = string.Empty;

        public int BookId { get; set; }
        public int UserId { get; set; }
        
        public int Rating 
        { 
            get => _rating;
            set
            {
                if (value < 1 || value > 5)
                    throw new ArgumentException("Rating must be between 1 and 5", nameof(value));
                _rating = value;
            }
        }

        public string ReviewText 
        { 
            get => _reviewText;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Review text cannot be null or empty", nameof(value));
                if (value.Length < 10)
                    throw new ArgumentException("Review text must be at least 10 characters", nameof(value));
                if (value.Length > 1000)
                    throw new ArgumentException("Review text cannot exceed 1000 characters", nameof(value));
                _reviewText = value.Trim();
            }
        }

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
        public string? RejectionReason { get; set; }
        public int? ModeratedBy { get; set; }
        public DateTime? ModeratedAt { get; set; }

        // Navigation properties
        public virtual Book Book { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual User? Moderator { get; set; }

        // Private constructor for EF Core
        public BookReview() { }

        public BookReview(int bookId, int userId, int rating, string reviewText)
        {
            if (bookId <= 0)
                throw new ArgumentException("BookId must be positive", nameof(bookId));
            if (userId <= 0)
                throw new ArgumentException("UserId must be positive", nameof(userId));

            BookId = bookId;
            UserId = userId;
            Rating = rating;
            ReviewText = reviewText;
            Status = ReviewStatus.Pending;
        }

        public void UpdateReview(int rating, string reviewText)
        {
            Rating = rating;
            ReviewText = reviewText;
            Status = ReviewStatus.Pending; // Reset to pending when updated
            RejectionReason = null;
            ModeratedBy = null;
            ModeratedAt = null;
            UpdateTimestamp();
        }

        public void Approve(int moderatorId)
        {
            if (moderatorId <= 0)
                throw new ArgumentException("ModeratorId must be positive", nameof(moderatorId));

            Status = ReviewStatus.Approved;
            ModeratedBy = moderatorId;
            ModeratedAt = DateTime.UtcNow;
            RejectionReason = null;
            UpdateTimestamp();
        }

        public void Reject(int moderatorId, string rejectionReason)
        {
            if (moderatorId <= 0)
                throw new ArgumentException("ModeratorId must be positive", nameof(moderatorId));
            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new ArgumentException("Rejection reason cannot be null or empty", nameof(rejectionReason));
            if (rejectionReason.Length > 500)
                throw new ArgumentException("Rejection reason cannot exceed 500 characters", nameof(rejectionReason));

            Status = ReviewStatus.Rejected;
            ModeratedBy = moderatorId;
            ModeratedAt = DateTime.UtcNow;
            RejectionReason = rejectionReason.Trim();
            UpdateTimestamp();
        }

        public bool CanBeModerated()
        {
            return Status == ReviewStatus.Pending;
        }

        public bool IsApproved()
        {
            return Status == ReviewStatus.Approved;
        }

        internal object ToDisplayViewModel()
        {
            throw new NotImplementedException();
        }
    }
}