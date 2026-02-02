using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;

namespace OnlineBookManagementSystem.Presentation.Mappers
{
    /// <summary>
    /// Maps Category entities to ViewModels
    /// Prevents entity leakage to views
    /// </summary>
    public static class CategoryViewModelMapper
    {
        public static CategoryManagementViewModel MapToCategoryManagementViewModel(
            IEnumerable<Category> categories)
        {
            var categoryList = categories.ToList();

            return new CategoryManagementViewModel
            {
                Categories = categoryList.Select(MapToCategoryItemViewModel).ToList(),
                TotalCategories = categoryList.Count
            };
        }

        public static CategoryItemViewModel MapToCategoryItemViewModel(Category category)
        {
            return new CategoryItemViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                BookCount = category.Books?.Count ?? 0,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}
