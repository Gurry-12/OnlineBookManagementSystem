using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;

namespace OnlineBookManagementSystem.Core.Application.Mappings
{
    /// <summary>
    /// Extension methods for mapping Review entities to ViewModels
    /// </summary>
    public static class ReviewMappingExtensions
    {
        /// <summary>
        /// Maps BookReview entity to ReviewDisplayViewModel
        /// </summary>
        public static ReviewDisplayViewModel ToViewModel(this BookReview review)
        {
            if (review == null) return null;

            return new ReviewDisplayViewModel
            {
                Id = review.Id,
                BookId = review.BookId,
                BookTitle = review.Book?.Title,
                UserId = review.UserId,
                UserName = review.User?.Name,
                Rating = review.Rating,
                ReviewText = review.ReviewText,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                CanEdit = false, // This should be set by the calling service
                IsEdited = review.UpdatedAt > review.CreatedAt
            };
        }

        /// <summary>
        /// Maps collection of BookReview entities to ReviewDisplayViewModel collection
        /// </summary>
        public static IEnumerable<ReviewDisplayViewModel> ToViewModels(this IEnumerable<BookReview> reviews)
        {
            return reviews?.Select(review => review.ToViewModel()) ?? Enumerable.Empty<ReviewDisplayViewModel>();
        }

        /// <summary>
        /// Maps ReviewSubmissionViewModel to BookReview entity
        /// </summary>
        public static BookReview ToEntity(this ReviewSubmissionViewModel model, int userId)
        {
            if (model == null) return null;

            return new BookReview
            {
                BookId = model.BookId,
                UserId = userId,
                Rating = model.Rating,
                ReviewText = model.ReviewText,
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Updates BookReview entity from ReviewSubmissionViewModel
        /// </summary>
        public static void UpdateFromViewModel(this BookReview review, ReviewSubmissionViewModel model)
        {
            if (review == null || model == null) return;

            review.Rating = model.Rating;
            review.ReviewText = model.ReviewText;
            review.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Maps BookReview entity to ReviewSubmissionViewModel for editing
        /// </summary>
        public static ReviewSubmissionViewModel ToSubmissionViewModel(this BookReview review)
        {
            if (review == null) return null;

            return new ReviewSubmissionViewModel
            {
                BookId = review.BookId,
                Rating = review.Rating,
                ReviewText = review.ReviewText
            };
        }

        /// <summary>
        /// Updates review status
        /// </summary>
        public static void UpdateStatus(this BookReview review, ReviewStatus newStatus)
        {
            if (review == null) return;

            review.Status = newStatus;
            review.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Maps collection of reviews to PaginatedResult for list views
        /// </summary>
        public static PaginatedResult<ReviewDisplayViewModel> ToListViewModel(this IEnumerable<BookReview> reviews,
            int currentPage, int totalPages, int totalReviews,
            string statusFilter = null, string searchTerm = null)
        {
            return new PaginatedResult<ReviewDisplayViewModel>
            {
                Items = reviews?.ToViewModels().ToList() ?? new List<ReviewDisplayViewModel>(),
                TotalCount = totalReviews,
                PageNumber = currentPage,
                PageSize = totalPages > 0 ? (int)Math.Ceiling((double)totalReviews / totalPages) : 10
            };
        }

        /// <summary>
        /// Calculates average rating from collection of reviews
        /// </summary>
        public static decimal CalculateAverageRating(this IEnumerable<BookReview> reviews)
        {
            var approvedReviews = reviews?.Where(r => r.Status == ReviewStatus.Approved).ToList();
            if (approvedReviews == null || !approvedReviews.Any())
                return 0;

            return Math.Round((decimal)approvedReviews.Average(r => r.Rating), 1);
        }

        /// <summary>
        /// Gets total count of approved reviews
        /// </summary>
        public static int GetApprovedReviewCount(this IEnumerable<BookReview> reviews)
        {
            return reviews?.Count(r => r.Status == ReviewStatus.Approved) ?? 0;
        }
    }
}