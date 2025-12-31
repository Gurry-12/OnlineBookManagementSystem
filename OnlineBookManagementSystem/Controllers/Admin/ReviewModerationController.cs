using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Controllers;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers.Admin
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

        [HttpGet("Pending")]
        public async Task<IActionResult> Pending(int page = 1, int pageSize = 20)
        {
            try
            {
                var pendingReviews = await _reviewService.GetPendingReviewsAsync(page, pageSize);
                return View(pendingReviews);
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
            var moderatorId = GetCurrentUserId();
            if (moderatorId == null)
            {
                return Unauthorized();
            }

            try
            {
                var success = await _reviewService.ApproveReviewAsync(id, moderatorId.Value);
                
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

            var moderatorId = GetCurrentUserId();
            if (moderatorId == null)
            {
                return Unauthorized();
            }

            try
            {
                var success = await _reviewService.RejectReviewAsync(id, moderatorId.Value, reason);
                
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

            var moderatorId = GetCurrentUserId();
            if (moderatorId == null)
            {
                return Unauthorized();
            }

            try
            {
                var success = await _reviewService.FlagReviewAsync(id, moderatorId.Value, reason);
                
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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}