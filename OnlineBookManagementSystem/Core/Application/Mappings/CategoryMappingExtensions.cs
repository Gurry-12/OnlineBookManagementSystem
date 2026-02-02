using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Shared;

namespace OnlineBookManagementSystem.Core.Application.Mappings
{
    /// <summary>
    /// Extension methods for mapping Category entities to DTOs and ViewModels
    /// </summary>
    public static class CategoryMappingExtensions
    {
        /// <summary>
        /// Maps Category entity to CategoryDto
        /// </summary>
        public static CategoryDto ToDto(this Category category)
        {
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        /// <summary>
        /// Maps collection of Category entities to CategoryDto collection
        /// </summary>
        public static IEnumerable<CategoryDto> ToDto(this IEnumerable<Category> categories)
        {
            return categories?.Select(category => category.ToDto()) ?? Enumerable.Empty<CategoryDto>();
        }

        /// <summary>
        /// Maps CategoryDto to Category entity
        /// </summary>
        public static Category ToEntity(this CategoryDto dto)
        {
            if (dto == null) return null;

            return new Category
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description
            };
        }

        /// <summary>
        /// Updates existing Category entity with values from CategoryDto
        /// </summary>
        public static void UpdateFromDto(this Category category, CategoryDto dto)
        {
            if (category == null || dto == null) return;

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Maps Category entity to CategoryViewModel
        /// </summary>
        public static CategoryViewModel ToViewModel(this Category category, int bookCount = 0)
        {
            if (category == null) return null;

            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                BookCount = bookCount
            };
        }

        /// <summary>
        /// Maps Category entity to CategoryWithCount
        /// </summary>
        public static CategoryWithCount ToCategoryWithCount(this Category category, int bookCount = 0)
        {
            if (category == null) return null;

            return new CategoryWithCount
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                BookCount = bookCount
            };
        }

        /// <summary>
        /// Maps Category entity to SelectListItem for dropdowns
        /// </summary>
        public static SelectListItem ToSelectListItem(this Category category, bool selected = false)
        {
            if (category == null) return null;

            return new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name,
                Selected = selected
            };
        }

        /// <summary>
        /// Maps collection of Categories to SelectListItem collection
        /// </summary>
        public static IEnumerable<SelectListItem> ToSelectListItems(this IEnumerable<Category> categories, int? selectedId = null)
        {
            return categories?.Select(c => c.ToSelectListItem(c.Id == selectedId)) ?? Enumerable.Empty<SelectListItem>();
        }

        /// <summary>
        /// Maps collection of Categories to CategoryViewModel collection
        /// </summary>
        public static IEnumerable<CategoryViewModel> ToViewModels(this IEnumerable<Category> categories)
        {
            return categories?.Select(c => c.ToViewModel()) ?? Enumerable.Empty<CategoryViewModel>();
        }

        /// <summary>
        /// Creates a Category entity from name and description
        /// </summary>
        public static Category CreateCategory(string name, string description = null)
        {
            return new Category
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }
    }
}