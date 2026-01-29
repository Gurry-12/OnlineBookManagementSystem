# DbUpdateConcurrencyException Fix

## Issue Description
`Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException: 'The database operation was expected to affect 1 row(s), but actually affected 0 row(s); data may have been modified or deleted since entities were loaded.'`

## Root Cause
The issue occurs because:

1. **Optimistic Concurrency Control**: All entities inheriting from `BaseEntity` have a `ConcurrencyToken` property configured as `IsConcurrencyToken()`
2. **Manual Data Insertion**: You manually inserted RefreshToken data into the database with ID 4
3. **Token Mismatch**: When EF Core tries to update/save changes, the `ConcurrencyToken` in memory doesn't match the one in the database
4. **Concurrency Conflict**: EF Core throws `DbUpdateConcurrencyException` to prevent data corruption

## Data Inserted
```
Id: 4
UserId: 2  
Token: 6T7jQn2P8Uo4TTVo2dYmTMFkYXNkA0ld9ZZ6CVGhflk=
ExpiryDate: 2026-02-04 13:32:55.3165928
IsRevoked: 0
Created: 2026-01-28 13:32:55.2935299
CreatedByIp: ::1
CreatedAt: 2026-01-28 13:32:55.2936513
ConcurrencyToken: 273B445E-2A02-4E54-8CDC-2D216D9C3BCE
UpdatedAt: 2026-01-28 13:32:55.2937211
IsDeleted: 0
```

## Solutions Applied

### 1. Enhanced AuthService with Concurrency Handling
**Files Modified:**
- `OnlineBookManagementSystem/Infrastructure/Services/Infrastructure/Authentication/AuthService.cs`

**Changes:**
- Added retry logic with exponential backoff for `GenerateTokensAsync()`
- Added retry logic with exponential backoff for `RefreshTokenAsync()`
- Added proper error logging for concurrency conflicts
- Added entity reload mechanism to handle stale data

### 2. Retry Logic Implementation
```csharp
var maxRetries = 3;
var retryCount = 0;

while (retryCount < maxRetries)
{
    try
    {
        await _context.SaveChangesAsync();
        break; // Success
    }
    catch (DbUpdateConcurrencyException ex)
    {
        retryCount++;
        // Log warning and reload entities
        foreach (var entry in ex.Entries)
        {
            await entry.ReloadAsync();
        }
        await Task.Delay(100 * retryCount); // Exponential backoff
    }
}
```

### 3. Database Cleanup Script
**File:** `fix-refresh-token-concurrency.sql`

**Purpose:** Remove manually inserted data causing conflicts

## Recommended Actions

### Immediate Fix
1. **Delete the problematic record:**
   ```sql
   DELETE FROM RefreshTokens WHERE Id = 4;
   ```

2. **Or clean all refresh tokens:**
   ```sql
   DELETE FROM RefreshTokens;
   ```

### Long-term Prevention
1. **Use the application's API** to create refresh tokens instead of manual insertion
2. **Test the retry logic** by attempting refresh token operations
3. **Monitor logs** for concurrency conflict warnings

## Expected Results
- No more `DbUpdateConcurrencyException` errors
- Automatic retry and recovery from concurrency conflicts
- Proper logging of concurrency issues for monitoring
- Robust refresh token functionality

## Testing Steps
1. Delete the manually inserted refresh token
2. Login to generate new refresh tokens through the application
3. Test refresh token functionality
4. Verify no concurrency exceptions occur
5. Check logs for any concurrency warnings