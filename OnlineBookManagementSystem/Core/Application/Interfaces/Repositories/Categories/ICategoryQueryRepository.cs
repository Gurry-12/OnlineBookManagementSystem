using OnlineBookManagementSystem.Core.Domain.Entities;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Categories
{
    /// <summary>
    /// Repository interface for query-specific operations on Category entities
    /// </summary>
    public interface ICategoryQueryRepository
    {
        Task<bool> ExistsAsync(Expression<Func<Category, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<Category, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<int> GetBookCountByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    }
}