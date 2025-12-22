using OnlineBookManagementSystem.Models;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Interfaces.Repositories
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(int id);
        Task<List<Book>> GetAllAsync();
        Task<List<Book>> GetPaginatedAsync(int skip, int take, Expression<Func<Book, bool>>? filter = null, string? sortBy = null);
        Task<int> CountAsync(Expression<Func<Book, bool>>? filter = null);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(Book book); // Usually soft delete logic in service, but repository handles db update
        Task SaveChangesAsync();
        IQueryable<Book> Query(); // For advanced filtering if needed within services
    }
}
