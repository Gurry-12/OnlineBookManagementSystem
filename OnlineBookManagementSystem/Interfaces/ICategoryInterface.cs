using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface ICategoryInterface
    {
        CategoryViewModel GetAllCategories();
        Task<Category> GetCategoryById(int id);
        Category AddCategory(Category category);
        Task<Category> UpdateCategory(Category category);
        Task<bool> DeleteCategory(int id);
        List<CategoryClassifyViewModel> GetAllCategoriesClassified();

        // New methods for enhanced functionality
        Task<List<SelectListItem>> GetCategoriesForDropdownAsync();
        Task<List<Category>> GetAllCategoriesAsync();
        Task<int> GetTotalCategoriesCountAsync();
        Task<List<CategoryWithCount>> GetCategoriesWithCountAsync();
        Task<Category?> GetCategoryByIdAsync(int categoryId);
        Task<bool> CreateCategoryAsync(string name, string description, int userId);
    }
}
