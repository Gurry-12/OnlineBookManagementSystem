using OnlineBookManagementSystem.Core.Domain.Entities;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Books
{
    /// <summary>
    /// Repository interface for read operations on Book entities
    /// </summary>
    public interface IBookReadRepository
    {
        Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> FindAsync(Expression<Func<Book, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Book?> FirstOrDefaultAsync(Expression<Func<Book, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> GetFeaturedBooksAsync(int count, CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> GetLowStockBooksAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Book> Books, int TotalCount)> GetPagedBooksAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null, 
            int? categoryId = null, 
            string? sortBy = null,
            CancellationToken cancellationToken = default);
    }
}