using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Reviews;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;

namespace OnlineBookManagementSystem.Presentation.Controllers.Admin
{
    [Authorize(Policy = "AdminOrHigher")]
    [Route("Admin/Reviews")]
    public class ReviewModerationController : BaseController
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewModerationController> _logger;

        public ReviewModerationController(IReviewService reviewService, ILogger<ReviewModerationController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("Moderation")]
        public async Task<IActionResult> Index(string status = "", int page = 1, int pageSize = 20)
        {
            try
            {
                // For now, redirect to pending if no status specified
                if (string.IsNullOrEmpty(status))
                {
                    return RedirectToAction("Pending");
                }

                // This would be expanded to handle all statuses
                return RedirectToAction("Pending");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review moderation");
                TempData["ErrorMessage"] = "Error loading reviews.";
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        [HttpGet("Pending")]
        public async Task<IActionResult> Pending(int page = 1, int pageSize = 20)
        {
            try
            {
                var pendingReviews = await _reviewService.GetPendingReviewsAsync(page, pageSize);

                // Convert to unified view model
                var unifiedModel = new UnifiedReviewListViewModel
                {
                    Reviews = pendingReviews.Items.Select(r => new UnifiedReviewViewModel
                    {
                        Id = r.Id,
                        BookId = r.BookId,
                        UserId = r.UserId,
                        UserName = r.UserName,
                        Rating = r.Rating,
                        ReviewText = r.ReviewText,
                        CreatedAt = r.CreatedAt,
                        BookTitle = r.BookTitle,
                        Status = "Pending",
                        Capabilities = new ReviewCapabilities
                        {
                            CanApprove = true,
                            CanReject = true,
                            CanFlag = true,
                            CanViewModerationDetails = true,
                            CanViewUserDetails = true,
                            ViewMode = "moderation"
                        }
                    }).ToList(),
                    TotalReviews = pendingReviews.TotalCount,
                    CurrentPage = pendingReviews.PageNumber,
                    TotalPages = pendingReviews.TotalPages,
                    Capabilities = new ReviewListCapabilities
                    {
                        CanModerate = true,
                        CanBulkApprove = true,
                        CanBulkReject = true,
                        CanViewModerationQueue = true,
                        CanViewAllStatuses = true,
                        ViewMode = "moderation",
                        PageTitle = "Pending Reviews",
                        IsAuthenticated = true
                    }
                };

                ViewBag.StatusFilter = "pending";
                return View("~/Presentation/Views/Reviews/ReviewModeration.cshtml", unifiedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending reviews");
                TempData["ErrorMessage"] = "Error loading pending reviews.";
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        [HttpPost("Approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var moderatorId = GetUserIdFromClaims();
            if (moderatorId == 0)
            {
                return Unauthorized();
            }

            try
            {
                var success = await _reviewService.ApproveReviewAsync(id, moderatorId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Review approved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to approve review. It may have already been moderated.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review {ReviewId}", id);
                TempData["ErrorMessage"] = "An error occurred while approving the review.";
            }

            return RedirectToAction("Pending");
        }

        [HttpPost("Reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Please provide a reason for rejection.";
                return RedirectToAction("Pending");
            }

            var moderatorId = GetUserIdFromClaims();
            if (moderatorId == 0)
            {
                return Unauthorized();
            }

            try
            {
                var success = await _reviewService.RejectReviewAsync(id, moderatorId, reason);

                if (success)
                {
                    TempData["SuccessMessage"] = "Review rejected successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to reject review. It may have already been moderated.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review {ReviewId}", id);
                TempData["ErrorMessage"] = "An error occurred while rejecting the review.";
            }

            return RedirectToAction("Pending");
        }

        [HttpPost("Flag/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Flag(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Please provide a reason for flagging.";
                return RedirectToAction("Pending");
            }

            var moderatorId = GetUserIdFromClaims();
            if (moderatorId == 0)
            {
                return Unauthorized();
            }

            try
            {
                var success = await _reviewService.FlagReviewAsync(id, moderatorId, reason);

                if (success)
                {
                    TempData["SuccessMessage"] = "Review flagged successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to flag review.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flagging review {ReviewId}", id);
                TempData["ErrorMessage"] = "An error occurred while flagging the review.";
            }

            return RedirectToAction("Pending");
        }

        // AJAX endpoints for unified view
        [HttpPost("Moderate")]
        public async Task<IActionResult> Moderate([FromBody] ModerationRequest request)
        {
            var moderatorId = GetUserIdFromClaims();
            if (moderatorId == 0)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                bool success = false;
                string message = "";

                switch (request.Action.ToLower())
                {
                    case "approve":
                        success = await _reviewService.ApproveReviewAsync(request.ReviewId, moderatorId);
                        message = success ? "Review approved successfully" : "Unable to approve review";
                        break;
                    case "reject":
                        if (string.IsNullOrWhiteSpace(request.Reason))
                        {
                            return Json(new { success = false, message = "Reason is required for rejection" });
                        }
                        success = await _reviewService.RejectReviewAsync(request.ReviewId, moderatorId, request.Reason);
                        message = success ? "Review rejected successfully" : "Unable to reject review";
                        break;
                    case "flag":
                        if (string.IsNullOrWhiteSpace(request.Reason))
                        {
                            return Json(new { success = false, message = "Reason is required for flagging" });
                        }
                        success = await _reviewService.FlagReviewAsync(request.ReviewId, moderatorId, request.Reason);
                        message = success ? "Review flagged successfully" : "Unable to flag review";
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid action" });
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating review {ReviewId} with action {Action}", request.ReviewId, request.Action);
                return Json(new { success = false, message = "An error occurred while processing the request" });
            }
        }

        [HttpPost("BulkModerate")]
        public async Task<IActionResult> BulkModerate([FromBody] BulkModerationRequest request)
        {
            var moderatorId = GetUserIdFromClaims();
            if (moderatorId == 0)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            if (request.ReviewIds == null || !request.ReviewIds.Any())
            {
                return Json(new { success = false, message = "No reviews selected" });
            }

            try
            {
                int successCount = 0;
                int totalCount = request.ReviewIds.Count;

                foreach (var reviewId in request.ReviewIds)
                {
                    bool success = false;

                    switch (request.Action.ToLower())
                    {
                        case "approve":
                            success = await _reviewService.ApproveReviewAsync(reviewId, moderatorId);
                            break;
                        case "reject":
                            if (!string.IsNullOrWhiteSpace(request.Reason))
                            {
                                success = await _reviewService.RejectReviewAsync(reviewId, moderatorId, request.Reason);
                            }
                            break;
                    }

                    if (success) successCount++;
                }

                var message = $"{successCount} of {totalCount} reviews {request.Action}d successfully";
                return Json(new { success = successCount > 0, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk moderating reviews with action {Action}", request.Action);
                return Json(new { success = false, message = "An error occurred while processing the bulk action" });
            }
        }

        [HttpGet("Analytics")]
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var analytics = await _reviewService.GetReviewAnalyticsAsync();
                return View(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review analytics");
                TempData["ErrorMessage"] = "Error loading review analytics.";
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        [HttpGet("TopRated")]
        public async Task<IActionResult> GetTopRatedBooks(int count = 10)
        {
            try
            {
                var topBooks = await _reviewService.GetTopRatedBooksAsync(count);
                return Json(topBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading top rated books");
                return BadRequest("Error loading top rated books");
            }
        }

        [HttpGet("LowestRated")]
        public async Task<IActionResult> GetLowestRatedBooks(int count = 10)
        {
            try
            {
                var lowestBooks = await _reviewService.GetLowestRatedBooksAsync(count);
                return Json(lowestBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lowest rated books");
                return BadRequest("Error loading lowest rated books");
            }
        }

    }

    // Request models for AJAX endpoints
    public class ModerationRequest
    {
        public int ReviewId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class BulkModerationRequest
    {
        public List<int> ReviewIds { get; set; } = new();
        public string Action { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
