using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.Shared;
using System.Security.Claims;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly BookManagementContext _context;
        private readonly ICategoryInterface _categoryInterface;

        public CategoryController(BookManagementContext context, ICategoryInterface categoryInterface)
        {
            _context = context;
            _categoryInterface = categoryInterface;
        }

        [Authorize(Policy = "AdminOrHigher")]
        public IActionResult DisplayCategory()
        {
            CategoryViewModel CategoryList = _categoryInterface.GetAllCategories();
            return View("Admin/DisplayCategory", CategoryList);
        }

        [AllowAnonymous]
        public IActionResult CategoryClassify()
        {
            // Fix: Adjust the type to match the method's return type
            List<CategoryClassifyViewModel> categoryClassification = _categoryInterface.GetAllCategoriesClassified();
            return View("User/CategoryClassify", categoryClassification);
        }

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

