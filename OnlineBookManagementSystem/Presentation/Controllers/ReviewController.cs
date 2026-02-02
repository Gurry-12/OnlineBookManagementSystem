using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Reviews;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize(Policy = "UserOrHigher")]
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ReviewSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide a valid rating and review text (10-1000 characters).";
                return RedirectToAction("Details", "Books", new { id = model.BookId });
            }

            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to submit a review.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _reviewService.SubmitReviewAsync(userId, model.BookId, model.Rating, model.ReviewText);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Details", "Books", new { id = model.BookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, ReviewSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide a valid rating and review text (10-1000 characters).";
                return RedirectToAction("Details", "Books", new { id = model.BookId });
            }

            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to update a review.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _reviewService.UpdateReviewAsync(id, userId, model.Rating, model.ReviewText);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Details", "Books", new { id = model.BookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int bookId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to delete a review.";
                return RedirectToAction("Login", "Auth");
            }

            var success = await _reviewService.DeleteReviewAsync(id, userId);

            if (success)
            {
                TempData["SuccessMessage"] = "Review deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to delete review. You can only delete your own reviews.";
            }

            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        // AJAX endpoint for delete
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteAjax([FromBody] DeleteReviewRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return Json(new { success = false, message = "You must be logged in to delete a review" });
            }

            try
            {
                var success = await _reviewService.DeleteReviewAsync(request.Id, userId);

                if (success)
                {
                    return Json(new { success = true, message = "Review deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to delete review. You can only delete your own reviews" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", request.Id);
                return Json(new { success = false, message = "An error occurred while deleting the review" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookReviews(int bookId, int page = 1, int pageSize = 10, ReviewSortOrder sortOrder = ReviewSortOrder.NewestFirst, int? ratingFilter = null)
        {
            try
            {
                var reviews = await _reviewService.GetBookReviewsAsync(bookId, page, pageSize, sortOrder, ratingFilter);

                // Convert to unified view model
                var userId = GetUserIdFromClaims();
                var unifiedModel = new UnifiedReviewListViewModel
                {
                    Reviews = reviews.Items.Select(r => new UnifiedReviewViewModel
                    {
                        Id = r.Id,
                        BookId = r.BookId,
                        UserId = r.UserId,
                        UserName = r.UserName,
                        Rating = r.Rating,
                        ReviewText = r.ReviewText,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        IsEdited = r.IsEdited,
                        Status = "Approved", // Public reviews are approved
                        Capabilities = new ReviewCapabilities
                        {
                            CanEditOwn = userId != 0 && r.UserId == userId,
                            CanDeleteOwn = userId != 0 && r.UserId == userId,
                            IsOwnReview = userId != 0 && r.UserId == userId,
                            IsAuthenticated = userId != 0,
                            ViewMode = "public"
                        }
                    }).ToList(),
                    TotalReviews = reviews.TotalCount,
                    CurrentPage = reviews.PageNumber,
                    TotalPages = reviews.TotalPages,
                    BookId = bookId,
                    Capabilities = new ReviewListCapabilities
                    {
                        CanCreate = userId != 0,
                        CanFilterByRating = true,
                        CanSortReviews = true,
                        ViewMode = "public",
                        IsAuthenticated = userId != 0
                    }
                };

                return PartialView("~/Presentation/Views/Reviews/_ReviewList.cshtml", unifiedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews for book {BookId}", bookId);
                return BadRequest("Error loading reviews");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookRating(int bookId)
        {
            try
            {
                var rating = await _reviewService.GetBookRatingAsync(bookId);

                // Check if current user has a review for this book
                var userId = GetUserIdFromClaims();
                if (userId != 0)
                {
                    var userReview = await _reviewService.GetUserReviewForBookAsync(userId, bookId);
                    if (userReview != null)
                    {
                        rating.HasUserReview = true;
                        rating.UserReview = new ReviewDisplayViewModel
                        {
                            Id = userReview.Id,
                            BookId = userReview.BookId,
                            UserId = userReview.UserId,
                            Rating = userReview.Rating,
                            ReviewText = userReview.ReviewText,
                            CreatedAt = userReview.CreatedAt,
                            UpdatedAt = userReview.UpdatedAt,
                            CanEdit = true,
                            IsEdited = userReview.UpdatedAt > userReview.CreatedAt.AddMinutes(1)
                        };
                    }
                }

                return Json(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rating for book {BookId}", bookId);
                return BadRequest("Error loading rating");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserReview(int bookId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return Unauthorized();
            }

            try
            {
                var review = await _reviewService.GetUserReviewForBookAsync(userId, bookId);
                if (review == null)
                {
                    return NotFound();
                }

                var reviewModel = new ReviewDisplayViewModel
                {
                    Id = review.Id,
                    BookId = review.BookId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt,
                    CanEdit = true,
                    IsEdited = review.UpdatedAt > review.CreatedAt.AddMinutes(1)
                };

                return Json(reviewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user review for book {BookId} and user {UserId}", bookId, userId);
                return BadRequest("Error loading review");
            }
        }

    }

    // Request model for AJAX delete
    public class DeleteReviewRequest
    {
        public int Id { get; set; }
        public int BookId { get; set; }
    }
}
