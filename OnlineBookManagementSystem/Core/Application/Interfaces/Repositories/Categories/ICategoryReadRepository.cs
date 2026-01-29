using OnlineBookManagementSystem.Core.Domain.Entities;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Categories
{
    /// <summary>
    /// Repository interface for read operations on Category entities
    /// </summary>
    public interface ICategoryReadRepository
    {
        Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> FindAsync(Expression<Func<Category, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Category?> FirstOrDefaultAsync(Expression<Func<Category, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetCategoriesWithBookCountAsync(CancellationToken cancellationToken = default);
    }
}