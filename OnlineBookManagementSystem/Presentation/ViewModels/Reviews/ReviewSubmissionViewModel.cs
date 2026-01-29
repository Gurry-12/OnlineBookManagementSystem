using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Reviews
{
    public class ReviewSubmissionViewModel
    {
        [Required]
        public int BookId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 1000 characters")]
        public string ReviewText { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }
        
        public bool IsAnonymous { get; set; }
        
        // For display purposes
        public string BookTitle { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}