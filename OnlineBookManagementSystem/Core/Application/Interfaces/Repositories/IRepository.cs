using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Categories;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        void Update(T entity);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        void UpdateRange(IEnumerable<T> entities);
        
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public interface IBookRepository : IRepository<Book>
    {
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

    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetCategoriesWithBookCountAsync(CancellationToken cancellationToken = default);
    }

    public interface IUnitOfWork : IDisposable
    {
        // Legacy interfaces for backward compatibility
        IBookRepository Books { get; }
        ICategoryRepository Categories { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
