using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Controllers;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
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

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to submit a review.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _reviewService.SubmitReviewAsync(userId.Value, model.BookId, model.Rating, model.ReviewText);
            
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

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to update a review.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _reviewService.UpdateReviewAsync(id, userId.Value, model.Rating, model.ReviewText);
            
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
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to delete a review.";
                return RedirectToAction("Login", "Auth");
            }

            var success = await _reviewService.DeleteReviewAsync(id, userId.Value);
            
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

        [HttpGet]
        public async Task<IActionResult> GetBookReviews(int bookId, int page = 1, int pageSize = 10, ReviewSortOrder sortOrder = ReviewSortOrder.NewestFirst, int? ratingFilter = null)
        {
            try
            {
                var reviews = await _reviewService.GetBookReviewsAsync(bookId, page, pageSize, sortOrder, ratingFilter);
                
                // Set CanEdit flag for current user's reviews
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    foreach (var review in reviews.Items)
                    {
                        review.CanEdit = review.UserId == userId.Value;
                    }
                }

                return PartialView("_ReviewList", reviews);
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
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    var userReview = await _reviewService.GetUserReviewForBookAsync(userId.Value, bookId);
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
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var review = await _reviewService.GetUserReviewForBookAsync(userId.Value, bookId);
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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}