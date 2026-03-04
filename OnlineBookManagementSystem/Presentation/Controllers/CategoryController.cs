using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.Categories;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly BookManagementContext _context;
        private readonly ICategoryInterface _categoryInterface;
        private readonly ICategoryInterface _categoryService;

        public CategoryController(BookManagementContext context, ICategoryInterface categoryInterface)
        {
            _context = context;
            _categoryInterface = categoryInterface;
            _categoryService = categoryInterface; // Use the same service for both
        }

        [Authorize(Policy = "AdminOrHigher")]
        public IActionResult DisplayCategory()
        {
            var CategoryList = _categoryInterface.GetAllCategories();
            return View("~/Presentation/Views/Categories/CategoryList.cshtml", CategoryList);
        }

        // [AllowAnonymous]
        // public async Task<IActionResult> CategoryList()
        // {
        //     var categories = await _categoryService.GetAllCategoriesAsync();

        //     // Create unified CategoryViewModel for public browsing
        //     var categoryViewModel = new ViewModels.Categories.CategoryViewModel
        //     {
        //         Categories = categories.Select(c => new CategoryItemViewModel
        //         {
        //             Id = c.Id,
        //             Name = c.Name,
        //             Description = c.Description,
        //             BookCount = c.Books?.Count ?? 0,
        //             CreatedAt = c.CreatedAt,
        //             UpdatedAt = c.UpdatedAt,
        //             Books = c.Books?.Take(6).Select(b => new CategoryBookViewModel
        //             {
        //                 Id = b.Id,
        //                 Title = b.Title,
        //                 Author = b.Author,
        //                 Price = b.Price,
        //                 StockQuantity = b.StockQuantity,
        //                 ImageUrl = b.ImageUrl,
        //                 CategoryId = b.CategoryId ?? 0
        //             }).ToList()
        //         }).ToList(),
        //         TotalCategories = categories.Count(),
        //         Capabilities = new CategoryCapabilities
        //         {
        //             CanViewBookDetails = true,
        //             CanAddToCart = User.Identity?.IsAuthenticated == true,
        //             ViewMode = "list",
        //             PageTitle = "Browse Categories",
        //             IsAuthenticated = User.Identity?.IsAuthenticated == true
        //         }
        //     };

        //     return View("~/Presentation/Views/Categories/CategoryList.cshtml", categoryViewModel);
        // }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public IActionResult CreateCategory([FromBody] Category data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Validation failed" });
                }

                data = _categoryInterface.AddCategory(data);
                return Json(new { success = true, message = "Category successfully added", data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while saving", error = ex.Message });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> DeleteCategory(int Id)
        {
            try
            {
                bool result = await _categoryInterface.DeleteCategory(Id);
                if (result == false)
                {
                    return Json(new { success = false, message = "Category not found" });
                }

                return Json(new { success = true, message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while deleting category", error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> GetCategoryById(int Id)
        {
            try
            {
                var getCategory = await _categoryInterface.GetCategoryById(Id);
                if (getCategory == null)
                {
                    return Json(new { success = false, message = "Category not found" });
                }

                return Json(new { success = true, getCategory, message = "Category found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while retrieving category", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> UpdateCategory([FromBody] Category data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Validation failed" });
                }

                data = await _categoryInterface.UpdateCategory(data);
                if (data == null)
                {
                    return Json(new { success = false, message = "Category not found or update failed" });
                }

                return Json(new { success = true, message = "Update successful", data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while updating category", error = ex.Message });
            }
        }

        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }
}

