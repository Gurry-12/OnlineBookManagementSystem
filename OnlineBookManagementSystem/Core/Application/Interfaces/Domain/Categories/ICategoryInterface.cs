using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.Shared;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories
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
        Task<bool> UpdateCategoryAsync(int id, string name, string description, int userId);
        Task<bool> DeleteCategoryAsync(int id, int userId);
    }
}
