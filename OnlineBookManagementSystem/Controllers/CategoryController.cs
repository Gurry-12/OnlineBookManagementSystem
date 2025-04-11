using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Linq;

namespace OnlineBookManagementSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly BookManagementContext _context;

        public CategoryController(BookManagementContext context)
        {
            _context = context;

        }

        public IActionResult DisplayCategory()
        {
            var viewModel = new CategoryViewModel
            {
                CategoryList = _context.Categories.ToList(),
                NewCategory = new Category()
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateCategory([FromBody] Category data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Validation failed";
                    return Json(new { message = "Validation failed", });
                }

                _context.Categories.Add(data);
                _context.SaveChanges();
                TempData["success"] = "Create SuccessFully";
                return Json(new { success = true, message = "Category successfully added", data });
            }
            catch (Exception ex)
            {
                return Json(new { message = "Error while saving", error = ex.Message });
            }
        }



        [HttpGet]
        public IActionResult DeleteCategory(int Id)
        {
            var checkCategory = _context.Categories.FirstOrDefault(s => s.Id == Id);
            if (checkCategory == null)
            {
                TempData["error"] = "Not Found";
                return Json("Not Found");
            }
            _context.Categories.Remove(checkCategory);
            _context.SaveChanges();
            TempData["success"] = "Delete SuccessFully";
            return Json("Delete SuccessFully");
        }

        [HttpGet]
        public IActionResult GetCategoryById(int Id)
        {
            var checkCategory = _context.Categories.FirstOrDefault(s => s.Id == Id);
            if (checkCategory == null)
            {
                TempData["error"] = "Not Found";
                return Json("Not Found");
            }
            return Json(new { success = true, checkCategory, message = "Category Found" });
        }


        [HttpPost]
        public IActionResult UpdateCategory([FromBody] Category data)
        {
            var checkCategory = _context.Categories.FirstOrDefault(s => s.Id == data.Id);
            if (checkCategory == null)
            {
                TempData["error"] = "Not Found";
                return Json("Not Found");
            }
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Validation failed";
                 return Json(new {  message = "Validation failed"});
            }
            checkCategory.Name = data.Name;
            _context.SaveChanges();
            TempData["success"] = "Update SuccessFully";
            return Json(new { success = true, message = "Update Successful ", data });
        }

    }

}

