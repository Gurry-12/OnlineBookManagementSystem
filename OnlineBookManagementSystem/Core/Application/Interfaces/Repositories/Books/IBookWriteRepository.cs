using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Books
{
    /// <summary>
    /// Repository interface for write operations on Book entities
    /// </summary>
    public interface IBookWriteRepository
    {
        Task<Book> AddAsync(Book entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> AddRangeAsync(IEnumerable<Book> entities, CancellationToken cancellationToken = default);
        void Update(Book entity);
        void UpdateRange(IEnumerable<Book> entities);
        void Remove(Book entity);
        void RemoveRange(IEnumerable<Book> entities);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}