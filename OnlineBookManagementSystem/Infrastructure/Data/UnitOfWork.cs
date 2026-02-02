using Microsoft.EntityFrameworkCore.Storage;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Infrastructure.Data.Repositories;

namespace OnlineBookManagementSystem.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BookManagementContext _context;
        private IDbContextTransaction? _transaction;

        // Legacy repositories
        private IBookRepository? _books;
        private ICategoryRepository? _categories;

        public UnitOfWork(BookManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Legacy properties for backward compatibility
        public IBookRepository Books => _books ??= new BookRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts at the UnitOfWork level
                foreach (var entry in ex.Entries)
                {
                    // Reload the entity from database to get current values
                    await entry.ReloadAsync(cancellationToken);
                }

                // Try to save again after resolving conflicts
                try
                {
                    return await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    // If it fails again, provide a user-friendly message
                    throw new InvalidOperationException("The data has been modified by another user. Please refresh the page and try again.");
                }
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started");

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction started");

            try
            {
                await _transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction started");

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}