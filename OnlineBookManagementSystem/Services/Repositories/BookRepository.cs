using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces.Repositories;
using OnlineBookManagementSystem.Models;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Services.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly BookManagementContext _context;

        public BookRepository(BookManagementContext context)
        {
            _context = context;
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<List<Book>> GetAllAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => !b.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<Book>> GetPaginatedAsync(int skip, int take, Expression<Func<Book, bool>>? filter = null, string? sortBy = null)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Where(b => !b.IsDeleted);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = sortBy switch
            {
                "priceAsc" => query.OrderBy(b => b.Price),
                "priceDesc" => query.OrderByDescending(b => b.Price),
                "title" => query.OrderBy(b => b.Title),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            return await query.Skip(skip).Take(take).ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Book, bool>>? filter = null)
        {
             var query = _context.Books.Where(b => !b.IsDeleted);
             if (filter != null) query = query.Where(filter);
             return await query.CountAsync();
        }

        public async Task AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
        }

        public Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Book book)
        {
             _context.Books.Update(book); // Usually we set IsDeleted = true before calling this
             return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IQueryable<Book> Query()
        {
            return _context.Books.Where(b => !b.IsDeleted).AsQueryable();
        }
    }
}
