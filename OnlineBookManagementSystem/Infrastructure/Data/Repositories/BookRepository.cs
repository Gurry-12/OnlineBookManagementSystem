using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Books;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Infrastructure.Data.Repositories
{
    public class BookRepository : Repository<Book>, IBookRepository, IBookReadRepository, IBookWriteRepository, IBookQueryRepository
    {
        public BookRepository(BookManagementContext context) : base(context)
        {
        }

        // IBookRepository methods (existing functionality)
        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(b => b.CategoryId == categoryId && !b.IsDeleted)
                .OrderBy(b => b.Title)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Book>> GetFeaturedBooksAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(b => b.IsFeatured && !b.IsDeleted)
                .OrderByDescending(b => b.AverageRating)
                .ThenByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Book>> GetLowStockBooksAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(b => !b.IsDeleted && b.StockQuantity <= b.LowStockThreshold && b.StockQuantity > 0)
                .OrderBy(b => b.StockQuantity)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(b => !b.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(lowerSearchTerm) ||
                    b.Author.ToLower().Contains(lowerSearchTerm) ||
                    (b.Description != null && b.Description.ToLower().Contains(lowerSearchTerm)));
            }

            return await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Book> Books, int TotalCount)> GetPagedBooksAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            int? categoryId = null,
            string? sortBy = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(b => !b.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(lowerSearchTerm) ||
                    b.Author.ToLower().Contains(lowerSearchTerm) ||
                    (b.Description != null && b.Description.ToLower().Contains(lowerSearchTerm)));
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "title" => query.OrderBy(b => b.Title),
                "author" => query.OrderBy(b => b.Author),
                "price" => query.OrderBy(b => b.Price.Amount),
                "pricedesc" => query.OrderByDescending(b => b.Price.Amount),
                "rating" => query.OrderByDescending(b => b.AverageRating),
                "stock" => query.OrderBy(b => b.StockQuantity),
                "stockdesc" => query.OrderByDescending(b => b.StockQuantity),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            // Apply pagination
            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (books, totalCount);
        }

        // IBookQueryRepository methods
        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        }

        public async Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(b => b.ISBN == isbn && !b.IsDeleted, cancellationToken);
        }

        public async Task<int> CountByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(b => b.CategoryId == categoryId && !b.IsDeleted, cancellationToken);
        }

        public async Task<decimal> GetAveragePriceAsync(CancellationToken cancellationToken = default)
        {
            var books = await _dbSet.Where(b => !b.IsDeleted).ToListAsync(cancellationToken);
            return books.Any() ? books.Average(b => b.Price.Amount) : 0;
        }

        public async Task<decimal> GetAveragePriceByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            var books = await _dbSet.Where(b => b.CategoryId == categoryId && !b.IsDeleted).ToListAsync(cancellationToken);
            return books.Any() ? books.Average(b => b.Price.Amount) : 0;
        }
    }
}