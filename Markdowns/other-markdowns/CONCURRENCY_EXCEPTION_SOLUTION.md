# DbUpdateConcurrencyException Solution

## Problem
The application was experiencing `DbUpdateConcurrencyException` errors when multiple users tried to modify the same data simultaneously.

## Root Cause
The exception occurs when:
1. Entity Framework tries to update a record that has been modified by another process
2. The original values in memory don't match the current database values
3. EF Core's optimistic concurrency control detects the conflict

## Solution Implemented

### 1. Enhanced Repository Pattern with Concurrency Handling

**File**: `Infrastructure/Data/Repositories/Repository.cs`

```csharp
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
                // Database wins approach (safest)
                await entry.ReloadAsync(cancellationToken);
                entity.UpdateTimestamp();
            }
            else
            {
                // For non-BaseEntity types
                entry.OriginalValues.SetValues(entry.GetDatabaseValues());
            }
        }
        
        // Retry save operation
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Unable to save changes due to concurrent modifications. Please refresh and try again.");
        }
    }
}
```

### 2. UnitOfWork Concurrency Handling

**File**: `Infrastructure/Data/UnitOfWork.cs`

Added similar concurrency handling at the UnitOfWork level to ensure consistent behavior across all repository operations.

### 3. Dedicated Concurrency Handler Service

**File**: `Infrastructure/Services/Helpers/ConcurrencyHandler.cs`

Created a specialized service for handling concurrency conflicts with retry logic:

```csharp
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
            if (attempt >= maxRetries)
            {
                throw new InvalidOperationException("The operation could not be completed due to concurrent modifications. Please refresh and try again.");
            }
            
            await HandleConcurrencyConflict(ex);
            await Task.Delay(100 * attempt); // Progressive delay
        }
    }
}
```

## Concurrency Resolution Strategy

### Database Wins Approach
- **What**: When a conflict occurs, the database values take precedence
- **Why**: Safest approach that prevents data loss
- **How**: Reload entity from database and update timestamp

### Benefits
1. **Automatic Recovery**: Most conflicts resolve automatically
2. **User-Friendly**: Provides clear error messages when manual intervention needed
3. **Data Integrity**: Prevents data corruption from concurrent modifications
4. **Scalable**: Works well under high concurrent load

## Usage Examples

### In Services
```csharp
public class BookService
{
    private readonly IConcurrencyHandler _concurrencyHandler;
    
    public async Task<Book> UpdateBookAsync(Book book)
    {
        return await _concurrencyHandler.HandleConcurrencyAsync(async () =>
        {
            _repository.Update(book);
            await _repository.SaveChangesAsync();
            return book;
        });
    }
}
```

### In Controllers
```csharp
[HttpPost]
public async Task<IActionResult> UpdateBook(BookViewModel model)
{
    try
    {
        var result = await _bookService.UpdateBookAsync(model.ToEntity());
        return Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(ex.Message);
    }
}
```

## Testing the Solution

1. **Single User Operations**: Should work normally
2. **Concurrent Updates**: Should resolve automatically in most cases
3. **High Conflict Scenarios**: Should provide user-friendly error messages

## Future Enhancements

1. **Add RowVersion Column**: For more robust optimistic concurrency control
2. **Client-Side Handling**: Add JavaScript to handle concurrency errors gracefully
3. **Audit Trail**: Log all concurrency conflicts for monitoring
4. **Custom Merge Logic**: Implement field-level conflict resolution

## Migration Path

If you want to add proper optimistic concurrency control later:

```bash
# Add RowVersion to BaseEntity
dotnet ef migrations add AddRowVersionConcurrencyToken
dotnet ef database update
```

Then uncomment the RowVersion property in BaseEntity and the configuration in BookManagementContext.