using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class BookDetailsViewModel
    {
        public Book Book { get; set; } = null!;
        public BookRatingViewModel Rating { get; set; } = new();
        public bool CanReview { get; set; }
        public ReviewSubmissionViewModel ReviewForm { get; set; } = new();
    }
}