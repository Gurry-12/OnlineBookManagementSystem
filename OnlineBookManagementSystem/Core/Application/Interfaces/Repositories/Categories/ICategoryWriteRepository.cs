using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Categories
{
    /// <summary>
    /// Repository interface for write operations on Category entities
    /// </summary>
    public interface ICategoryWriteRepository
    {
        Task<Category> AddAsync(Category entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> AddRangeAsync(IEnumerable<Category> entities, CancellationToken cancellationToken = default);
        void Update(Category entity);
        void UpdateRange(IEnumerable<Category> entities);
        void Remove(Category entity);
        void RemoveRange(IEnumerable<Category> entities);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}