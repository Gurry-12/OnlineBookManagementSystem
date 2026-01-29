using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Categories;

namespace OnlineBookManagementSystem.Infrastructure.Data.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository, ICategoryReadRepository, ICategoryWriteRepository, ICategoryQueryRepository
    {
        public CategoryRepository(BookManagementContext context) : base(context)
        {
        }

        // ICategoryRepository methods (existing functionality)
        public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && !c.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithBookCountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted)
                .Include(c => c.Books.Where(b => !b.IsDeleted))
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public override async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        // ICategoryQueryRepository methods
        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
        }

        public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower() && !c.IsDeleted, cancellationToken);
        }

        public async Task<int> GetBookCountByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Books.CountAsync(b => b.CategoryId == categoryId && !b.IsDeleted, cancellationToken);
        }
    }
}