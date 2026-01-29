using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;

namespace OnlineBookManagementSystem.Presentation.Controllers.Admin
{
    /// <summary>
    /// Handles admin category management functionality following SRP.
    /// Responsible only for category administration tasks.
    /// </summary>
    [Authorize(Policy = "AdminOrHigher")]
    public class AdminCategoryManagementController : BaseController
    {
        private readonly ICategoryInterface _categoryService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<AdminCategoryManagementController> _logger;

        public AdminCategoryManagementController(
            ICategoryInterface categoryService,
            IActivityLogger activityLogger,
            ILogger<AdminCategoryManagementController> logger)
        {
            _categoryService = categoryService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> CategoryManagement()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                await _activityLogger.LogAsync("ViewCategories", "Admin category management accessed", userId);
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category management for admin user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load categories.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Category name is required" });
            }

            try
            {
                var success = await _categoryService.CreateCategoryAsync(request.Name, request.Description, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("CreateCategory", $"Category '{request.Name}' created", userId);
                    return Json(new { success = true, message = "Category created successfully" });
                }
                return Json(new { success = false, message = "Failed to create category" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category '{CategoryName}' by admin {UserId}", request.Name, userId);
                return Json(new { success = false, message = "An error occurred while creating category" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (request.Id <= 0 || string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Invalid category data" });
            }

            try
            {
                var success = await _categoryService.UpdateCategoryAsync(request.Id, request.Name, request.Description, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateCategory", $"Category '{request.Name}' updated", userId);
                    return Json(new { success = true, message = "Category updated successfully" });
                }
                return Json(new { success = false, message = "Failed to update category" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId} by admin {UserId}", request.Id, userId);
                return Json(new { success = false, message = "An error occurred while updating category" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid category ID" });
            }

            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("DeleteCategory", $"Category with ID {id} deleted", userId);
                    return Json(new { success = true, message = "Category deleted successfully" });
                }
                return Json(new { success = false, message = "Failed to delete category or category not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId} by admin {UserId}", id, userId);
                return Json(new { success = false, message = "An error occurred while deleting category" });
            }
        }

        public async Task<IActionResult> CategoryDetails(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction(nameof(CategoryManagement));
                }

                await _activityLogger.LogAsync("ViewCategoryDetails", $"Admin viewed details for category {id}", userId);
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category details for category {CategoryId}", id);
                TempData["ErrorMessage"] = "Failed to load category details.";
                return RedirectToAction(nameof(CategoryManagement));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryStats()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var categoriesWithCount = await _categoryService.GetCategoriesWithCountAsync();
                return Json(new { success = true, data = categoriesWithCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category statistics");
                return Json(new { success = false, message = "Failed to load category statistics" });
            }
        }
    }

    // Request models
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateCategoryRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}