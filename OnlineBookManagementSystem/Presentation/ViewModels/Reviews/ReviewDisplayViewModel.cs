using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Reviews
{
    public class ReviewDisplayViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ReviewStatus Status { get; set; }
        public bool IsAnonymous { get; set; }
        
        // Computed properties
        public string RatingStars => new string('★', Rating) + new string('☆', 5 - Rating);
        public string StatusDisplay => Status.ToDisplayString();
        public string TimeAgo => CalculateTimeAgo(CreatedAt);
        public string DisplayName => IsAnonymous ? "Anonymous" : UserName;
        public bool IsApproved => Status == ReviewStatus.Approved;
        public bool IsPending => Status == ReviewStatus.Pending;
        public bool IsRejected => Status == ReviewStatus.Rejected;
        
        // User interaction properties
        public bool CanEdit { get; set; }
        public bool IsEdited { get; set; }
        
        private string CalculateTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            
            return timeSpan.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)timeSpan.TotalMinutes} minutes ago",
                < 1440 => $"{(int)timeSpan.TotalHours} hours ago", // 24 hours
                < 43200 => $"{(int)timeSpan.TotalDays} days ago", // 30 days
                _ => dateTime.ToString("MMM dd, yyyy")
            };
        }
    }
}