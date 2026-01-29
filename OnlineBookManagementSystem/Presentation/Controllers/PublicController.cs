using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Showcase;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication;
using OnlineBookManagementSystem.Presentation.ViewModels.Showcase;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    /// <summary>
    /// Public-facing controller for unauthenticated users to browse books and view project showcase
    /// </summary>
    public class PublicController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookAnalyticsService _bookAnalyticsService;
        private readonly IPublicDemoService _publicDemoService;
        private readonly IRoleBasedRedirectionService _redirectionService;
        private readonly IMemoryCache _cache;

        public PublicController(
            IBookQueryService bookQueryService,
            IBookAnalyticsService bookAnalyticsService,
            IPublicDemoService publicDemoService,
            IRoleBasedRedirectionService redirectionService,
            IMemoryCache cache)
        {
            _bookQueryService = bookQueryService;
            _bookAnalyticsService = bookAnalyticsService;
            _publicDemoService = publicDemoService;
            _redirectionService = redirectionService;
            _cache = cache;
        }

        public IActionResult Index() => View();


        /// <summary>
        /// Landing page with featured books and categories - redirects authenticated users to their dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Check if user is authenticated and should bypass public area
            if (_redirectionService.ShouldBypassPublicArea(User))
            {
                var redirectUrl = await _redirectionService.GetRedirectUrlForClaimsAsync(User);
                return Redirect(redirectUrl);
            }

            // Show showcase content for unauthenticated users
            var showcaseContent = await _publicDemoService.GetShowcaseContentAsync();

            ViewBag.Title = "Online Book Management System - Clean Architecture Showcase";
            ViewBag.MetaDescription = "A comprehensive book management system showcasing Clean Architecture, SOLID principles, and modern web development practices.";

            return View(showcaseContent);
        }

        /// <summary>
        /// Main showcase page with comprehensive project portfolio
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Showcase()
        {
            // Redirect authenticated users to their dashboard
            if (_redirectionService.ShouldBypassPublicArea(User))
            {
                var redirectUrl = await _redirectionService.GetRedirectUrlForClaimsAsync(User);
                return Redirect(redirectUrl);
            }

            var showcaseContent = await _publicDemoService.GetShowcaseContentAsync();

            ViewBag.Title = "Project Showcase - Clean Architecture Implementation";
            ViewBag.MetaDescription = "Comprehensive showcase of Clean Architecture principles, SOLID design patterns, and modern web development techniques.";

            return View(showcaseContent);
        }

        /// <summary>
        /// Technical details and architecture documentation
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TechnicalDetails()
        {
            // Redirect authenticated users to their dashboard
            if (_redirectionService.ShouldBypassPublicArea(User))
            {
                var redirectUrl = await _redirectionService.GetRedirectUrlForClaimsAsync(User);
                return Redirect(redirectUrl);
            }

            var technicalHighlights = await _publicDemoService.GetTechnicalHighlightsAsync();
            var showcaseContent = await _publicDemoService.GetShowcaseContentAsync();

            var viewModel = new TechnicalDetailsViewModel
            {
                Architecture = showcaseContent.Architecture,
                TechnicalStack = showcaseContent.TechnicalStack,
                TechnicalHighlights = technicalHighlights,
                PerformanceMetrics = await _publicDemoService.GetPerformanceMetricsAsync()
            };

            ViewBag.Title = "Technical Details - Architecture & Implementation";
            ViewBag.MetaDescription = "Deep dive into Clean Architecture implementation, SOLID principles, and technical decisions behind the book management system.";

            return View(viewModel);
        }

        /// <summary>
        /// Interactive demo of the live system with read-only access
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> InteractiveDemo()
        {
            // Redirect authenticated users to their dashboard
            if (_redirectionService.ShouldBypassPublicArea(User))
            {
                var redirectUrl = await _redirectionService.GetRedirectUrlForClaimsAsync(User);
                return Redirect(redirectUrl);
            }

            var featuredBooks = await _publicDemoService.GetFeaturedBooksAsync(12);
            var categories = await _publicDemoService.GetCategoriesWithCountsAsync();
            var statistics = await _publicDemoService.GetSystemStatisticsAsync();
            var featureShowcases = await _publicDemoService.GetFeatureShowcasesAsync();

            var viewModel = new InteractiveDemoViewModel
            {
                FeaturedBooks = featuredBooks,
                Categories = categories,
                Statistics = statistics,
                FeatureShowcases = featureShowcases
            };

            ViewBag.Title = "Interactive Demo - Live System Showcase";
            ViewBag.MetaDescription = "Experience the book management system with live data, search functionality, and feature demonstrations.";

            return View(viewModel);
        }

        /// <summary>
        /// Developer story and project journey narrative
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeveloperStory()
        {
            // Redirect authenticated users to their dashboard
            if (_redirectionService.ShouldBypassPublicArea(User))
            {
                var redirectUrl = await _redirectionService.GetRedirectUrlForClaimsAsync(User);
                return Redirect(redirectUrl);
            }

            var showcaseContent = await _publicDemoService.GetShowcaseContentAsync();

            var viewModel = new DeveloperStoryPageViewModel
            {
                DeveloperStory = showcaseContent.DeveloperStory,
                ProjectOverview = showcaseContent.ProjectOverview,
                ContactInfo = showcaseContent.ContactInfo,
                TechnicalAchievements = showcaseContent.FeatureHighlights
            };

            ViewBag.Title = "Developer Story - Project Journey & Vision";
            ViewBag.MetaDescription = "Learn about the motivation, challenges, and technical decisions behind this comprehensive book management system.";

            return View(viewModel);
        }

        /// <summary>
        /// Browse all books with filters (enhanced with portfolio context)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Browse(
            int page = 1,
            string? search = null,
            int? categoryId = null,
            string? sortBy = "title",
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            // Redirect authenticated users to their dashboard
            if (_redirectionService.ShouldBypassPublicArea(User))
            {
                var redirectUrl = await _redirectionService.GetRedirectUrlForClaimsAsync(User);
                return Redirect(redirectUrl);
            }

            // Use the public demo service for read-only access
            var books = categoryId.HasValue
                ? await _publicDemoService.GetBooksByCategoryAsync(categoryId.Value, page, 12)
                : !string.IsNullOrEmpty(search)
                    ? await _publicDemoService.SearchBooksAsync(search, page, 12)
                    : await _publicDemoService.GetFeaturedBooksAsync(12);

            var categories = await _publicDemoService.GetCategoriesWithCountsAsync();

            ViewBag.Categories = categories;
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CurrentPage = page;
            ViewBag.Title = "Browse Books - Interactive Demo";
            ViewBag.MetaDescription = "Browse our comprehensive book collection with advanced search and filtering capabilities.";

            return View(books);
        }

        /// <summary>
        /// Book details page (public view with technical implementation notes)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BookDetails(int id)
        {
            // Redirect authenticated users to their dashboard
            if (_redirectionService.ShouldBypassPublicArea(User))
            {
                var redirectUrl = await _redirectionService.GetRedirectUrlForClaimsAsync(User);
                return Redirect(redirectUrl);
            }

            var bookDetails = await _publicDemoService.GetBookDetailsAsync(id);
            if (bookDetails == null)
            {
                return NotFound();
            }

            ViewBag.Title = $"{bookDetails.Book.Title} - Book Details";
            ViewBag.MetaDescription = $"View details for {bookDetails.Book.Title} by {bookDetails.Book.Author} in our interactive book management demo.";

            return View(bookDetails);
        }

        /// <summary>
        /// Search books (AJAX endpoint) - enhanced with caching
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchBooks(string query, int page = 1)
        {
            try
            {
                var cacheKey = $"search_{query}_{page}";

                if (_cache.TryGetValue(cacheKey, out object? cachedResult))
                {
                    return Json(cachedResult);
                }

                var books = await _publicDemoService.SearchBooksAsync(query, page, 12);
                var result = new { success = true, data = books };

                // Cache for 5 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

                return Json(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error searching books with query: {Query}", query);
                return Json(new { success = false, message = "Search temporarily unavailable" });
            }
        }

        /// <summary>
        /// Get books by category (AJAX endpoint) - enhanced with caching
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBooksByCategory(int categoryId, int page = 1)
        {
            try
            {
                var cacheKey = $"category_{categoryId}_{page}";

                if (_cache.TryGetValue(cacheKey, out object? cachedResult))
                {
                    return Json(cachedResult);
                }

                var books = await _publicDemoService.GetBooksByCategoryAsync(categoryId, page, 12);
                var result = new { success = true, data = books };

                // Cache for 10 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

                return Json(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting books for category: {CategoryId}", categoryId);
                return Json(new { success = false, message = "Category data temporarily unavailable" });
            }
        }

        /// <summary>
        /// Get system statistics (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSystemStatistics()
        {
            try
            {
                var statistics = await _publicDemoService.GetSystemStatisticsAsync();
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting system statistics");
                return Json(new { success = false, message = "Statistics temporarily unavailable" });
            }
        }

        /// <summary>
        /// Get featured books for showcase (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFeaturedBooks(int count = 8)
        {
            try
            {
                var books = await _publicDemoService.GetFeaturedBooksAsync(count);
                return Json(new { success = true, data = books });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting featured books");
                return Json(new { success = false, message = "Featured books temporarily unavailable" });
            }
        }

        /// <summary>
        /// Submit collaboration inquiry form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCollaborationInquiry([FromBody] CollaborationInquiryViewModel inquiry)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Please fill in all required fields correctly." });
                }

                // Log the inquiry for follow-up (in a real application, this would be saved to database or sent via email)
                Logger.LogInformation("Collaboration inquiry received from {Email}: {Subject}", inquiry.Email, inquiry.Subject);

                // In a real application, you would:
                // 1. Save to database
                // 2. Send email notification
                // 3. Add to CRM system
                // For now, we'll just log and return success

                return Json(new
                {
                    success = true,
                    message = "Thank you for your inquiry! I'll get back to you within 24-48 hours."
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing collaboration inquiry from {Email}", inquiry?.Email);
                return Json(new
                {
                    success = false,
                    message = "Sorry, there was an error processing your inquiry. Please try again or contact me directly via email."
                });
            }
        }
    }
}
