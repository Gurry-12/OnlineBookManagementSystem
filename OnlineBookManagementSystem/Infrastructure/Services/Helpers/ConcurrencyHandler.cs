using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Services.Helpers
{
    public interface IConcurrencyHandler
    {
        Task<T> HandleConcurrencyAsync<T>(Func<Task<T>> operation, int maxRetries = 3);
        Task HandleConcurrencyAsync(Func<Task> operation, int maxRetries = 3);
    }

    public class ConcurrencyHandler : IConcurrencyHandler
    {
        private readonly ILogger<ConcurrencyHandler> _logger;

        public ConcurrencyHandler(ILogger<ConcurrencyHandler> logger)
        {
            _logger = logger;
        }

        public async Task<T> HandleConcurrencyAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            var attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    return await operation();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    attempt++;
                    _logger.LogWarning("Concurrency conflict occurred on attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

                    if (attempt >= maxRetries)
                    {
                        _logger.LogError(ex, "Max retry attempts reached for concurrency conflict");
                        throw new InvalidOperationException(
                            "The operation could not be completed due to concurrent modifications. Please refresh and try again.");
                    }

                    // Handle the concurrency conflict
                    await HandleConcurrencyConflict(ex);

                    // Wait a bit before retrying
                    await Task.Delay(100 * attempt);
                }
            }

            throw new InvalidOperationException("Unexpected error in concurrency handling");
        }

        public async Task HandleConcurrencyAsync(Func<Task> operation, int maxRetries = 3)
        {
            await HandleConcurrencyAsync(async () =>
            {
                await operation();
                return true;
            }, maxRetries);
        }

        private async Task HandleConcurrencyConflict(DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is BaseEntity entity)
                {
                    // Get the current values from the database
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        // Entity was deleted by another user
                        _logger.LogWarning("Entity {EntityType} with ID {EntityId} was deleted by another user",
                            entity.GetType().Name, entity.Id);
                        throw new InvalidOperationException("The record has been deleted by another user.");
                    }

                    // Reload the entity with current database values
                    entry.OriginalValues.SetValues(databaseValues);

                    // Update timestamp to reflect the reload
                    entity.UpdateTimestamp();

                    _logger.LogInformation("Resolved concurrency conflict for {EntityType} with ID {EntityId}",
                        entity.GetType().Name, entity.Id);
                }
                else
                {
                    // For non-BaseEntity types, use database values
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues != null)
                    {
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                }
            }
        }
    }
}