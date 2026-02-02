using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IActivityLogger? _activityLogger;

        public HomeController(IActivityLogger? activityLogger = null)
        {
            _activityLogger = activityLogger;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("SuperAdmin")) return RedirectToAction("Dashboard", "SuperAdmin");
                if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
                if (User.IsInRole("User")) return RedirectToAction("Dashboard", "User");
            }
            // Redirect to new public landing page
            return RedirectToAction("Index", "Public");
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Support(string email, string message, string name = "")
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            try
            {
                // Log the contact form submission
                await LogActivityAsync("ContactForm", $"Contact form submitted by {email}");

                // In a real implementation, you would:
                // 1. Save to database for tracking
                // 2. Send email to admin
                // 3. Send confirmation email to user

                // For now, we'll log it and show success
                var logMessage = $"Contact Form - Name: {name}, Email: {email}, Message: {message.Substring(0, Math.Min(100, message.Length))}...";

                TempData["SuccessMessage"] = "Thank you! Your message has been received. We will contact you shortly.";
                return RedirectToAction(nameof(Support));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Sorry, there was an error sending your message. Please try again.";
                return View();
            }
        }

        private async Task LogActivityAsync(string action, string description)
        {
            if (_activityLogger != null)
            {
                var userId = GetUserIdFromClaims();
                await _activityLogger.LogAsync(action, description, userId);
            }
        }

        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }
}
