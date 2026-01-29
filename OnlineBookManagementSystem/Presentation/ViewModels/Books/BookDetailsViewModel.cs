using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Books;

public class BookDetailsViewModel
{
    public Book Book { get; set; } = null!;
    public BookRatingViewModel Rating { get; set; } = new();
    public bool CanReview { get; set; }
    public ReviewSubmissionViewModel ReviewForm { get; set; } = new();
    public bool IsPublicView { get; set; } = false;
}