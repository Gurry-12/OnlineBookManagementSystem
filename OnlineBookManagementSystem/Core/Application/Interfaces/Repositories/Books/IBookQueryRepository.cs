using OnlineBookManagementSystem.Core.Domain.Entities;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Books
{
    /// <summary>
    /// Repository interface for query-specific operations on Book entities
    /// </summary>
    public interface IBookQueryRepository
    {
        Task<bool> ExistsAsync(Expression<Func<Book, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<Book, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<int> CountByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<decimal> GetAveragePriceAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetAveragePriceByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    }
}