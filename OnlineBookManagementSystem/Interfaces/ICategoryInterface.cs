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
      
    }
}
