using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.Shared;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Categories
{
    public class CategoryServices : ICategoryInterface
    {
        private readonly BookManagementContext _context;
        public CategoryServices(BookManagementContext context)
        {
            _context = context;
        }

        //Display the Details - Admin Priviledge
        public CategoryViewModel GetAllCategories()
        {
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.Books.Where(b => !b.IsDeleted).Take(3))
                .ToList();

            return new CategoryViewModel
            {
                CategoryList = categories,
                NewCategory = new Category()
            };
        }



        //Add Categories
        public Category AddCategory(Category data)
        {
            _context.Categories.Add(data);
            _context.SaveChanges();
            return data;
        }

        //Soft Delete the Category
        public async Task<bool> DeleteCategory(int id)
        {
            var checkCategory = await _context.Categories.Where(c => (bool)!c.IsDeleted).FirstOrDefaultAsync(c => c.Id == id);
            if (checkCategory == null)
            {
                return false;
            }
            checkCategory.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;

        }


        //Get Category Details via ID
        public async Task<Category> GetCategoryById(int id)
        {
            try
            {
                return await _context.Categories.Where(c => !c.IsDeleted && c.Id == id).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {

                throw new Exception("Something went wrong while fetching the category.", ex);
            }
        }

        //update Category
        public async Task<Category> UpdateCategory(Category category)
        {
            var checkCategory = await _context.Categories
                .Where(c => (bool)!c.IsDeleted)
                .FirstOrDefaultAsync(s => s.Id == category.Id);

            if (checkCategory == null)
            {
                throw new InvalidOperationException("Category not found or has been deleted.");
            }

            checkCategory.Name = category.Name;
            await _context.SaveChangesAsync();
            return checkCategory;
        }


        //user - priviledge 
        public List<CategoryClassifyViewModel> GetAllCategoriesClassified()
        {
            return _context.Books.Where(b => b.IsDeleted == false).GroupBy(b => b.Category.Name)
                .Select(s => new CategoryClassifyViewModel
                {
                    CategoryName = s.Key,
                    Books = s.ToList()
                }).ToList();
        }

        // New methods for enhanced functionality
        public async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetCategoriesForDropdownAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<int> GetTotalCategoriesCountAsync()
        {
            return await _context.Categories.CountAsync(c => !c.IsDeleted);
        }

        public async Task<List<CategoryWithCount>> GetCategoriesWithCountAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new CategoryWithCount
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    BookCount = c.Books.Count(b => !b.IsDeleted),
                    CreatedAt = c.CreatedAt
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted);
        }

        public async Task<bool> CreateCategoryAsync(string name, string description, int userId)
        {
            try
            {
                var category = new Category
                {
                    Name = name,
                    Description = description,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(int id, string name, string description, int userId)
        {
            try
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
                if (category == null)
                    return false;

                category.Name = name;
                category.Description = description;
                category.UpdatedAt = DateTime.UtcNow;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id, int userId)
        {
            try
            {
                return await DeleteCategory(id); // Use existing method
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}


