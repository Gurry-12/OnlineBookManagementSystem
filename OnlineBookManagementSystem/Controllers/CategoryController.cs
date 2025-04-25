using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly BookManagementContext _context;

        private readonly ICategoryInterface _categoryInterface;

        public CategoryController(BookManagementContext context, ICategoryInterface categoryInterface)
        {
            _context = context;
            _categoryInterface = categoryInterface;

        }

        public IActionResult DisplayCategory()
        {
            CategoryViewModel CategoryList = _categoryInterface.GetAllCategories();

            return View("Admin/DisplayCategory", CategoryList);
        }

        public IActionResult CategoryClassify()
        {
            // Fix: Adjust the type to match the method's return type
            List<CategoryClassifyViewModel> categoryClassification = _categoryInterface.GetAllCategoriesClassified();
            return View("User/CategoryClassify", categoryClassification);
        }

        [HttpPost]
        public IActionResult CreateCategory([FromBody] Category data)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    return Json(new { message = "Validation failed", });
                }

                data = _categoryInterface.AddCategory(data);

                return Json(new { success = true, message = "Category successfully added", data });
            }
            catch (Exception ex)
            {
                return Json(new { message = "Error while saving", error = ex.Message });
            }
        }



        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int Id)
        {

            bool result = await _categoryInterface.DeleteCategory(Id);
            if (result == false)
            {
               
                return Json("Not Found");
            }
                       
            return Json("Delete SuccessFully");
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryById(int Id)
        {
            var getCategory = await _categoryInterface.GetCategoryById(Id);
            
           return Json(new { success = true, getCategory, message = "Category Found" });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromBody] Category data)
        {      
            if (!ModelState.IsValid)
            {
                return Json(new { message = "Validation failed" });
            }

            data = await _categoryInterface.UpdateCategory(data);
            if (data == null)
            {
                return Json("Not Updated");
            }
          
            return Json(new { success = true, message = "Update Successful ", data });
        }

    }

}

