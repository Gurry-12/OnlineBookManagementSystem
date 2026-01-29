using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using System.Linq.Expressions;

namespace OnlineBookManagementSystem.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly BookManagementContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(BookManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(e => !e.IsDeleted).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
                return await _dbSet.CountAsync(e => !e.IsDeleted, cancellationToken);
            
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.UpdateTimestamp();
            _dbSet.Update(entity);
            return entity;
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public virtual void Remove(T entity)
        {
            entity.MarkAsDeleted();
            _dbSet.Update(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.MarkAsDeleted();
            }
            _dbSet.UpdateRange(entities);
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts gracefully
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is BaseEntity entity)
                    {
                        // For concurrency conflicts, we have several options:
                        // 1. Client wins - use current values
                        // 2. Database wins - reload from database
                        // 3. Merge - combine changes intelligently
                        
                        // Option 2: Database wins (safest approach)
                        await entry.ReloadAsync(cancellationToken);
                        
                        // Update the timestamp to reflect the reload
                        entity.UpdateTimestamp();
                        
                        // Log the concurrency conflict for monitoring
                        // Note: We can't use ILogger here as it would create circular dependency
                        System.Diagnostics.Debug.WriteLine($"Concurrency conflict resolved for {entity.GetType().Name} with ID {entity.Id}");
                    }
                    else
                    {
                        // For non-BaseEntity types, use the proposed values
                        entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                    }
                }
                
                // Try to save again after resolving conflicts
                try
                {
                    return await _context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If it fails again, let the caller handle it
                    throw new InvalidOperationException("Unable to save changes due to concurrent modifications. Please refresh and try again.");
                }
            }
        }
    }
}