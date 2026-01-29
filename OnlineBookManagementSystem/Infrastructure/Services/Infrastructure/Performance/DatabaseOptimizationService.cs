using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using System.Data.Common;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Performance
{
    /// <summary>
    /// Service for optimizing database queries and connections
    /// </summary>
    public interface IDatabaseOptimizationService
    {
        Task OptimizeConnectionPoolAsync();
        Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, int timeoutSeconds = 30);
        Task<List<T>> ExecuteBatchQueryAsync<T>(IQueryable<T> query, int batchSize = 1000);
        Task WarmupConnectionPoolAsync();
        Task AnalyzeQueryPerformanceAsync();
        Task OptimizeIndexesAsync();
    }

    public class DatabaseOptimizationService : IDatabaseOptimizationService
    {
        private readonly BookManagementContext _context;
        private readonly ILogger<DatabaseOptimizationService> _logger;
        private readonly IConfiguration _configuration;

        public DatabaseOptimizationService(
            BookManagementContext context,
            ILogger<DatabaseOptimizationService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task OptimizeConnectionPoolAsync()
        {
            try
            {
                _logger.LogInformation("Optimizing database connection pool...");

                // Configure connection pool settings
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogWarning("No connection string found for optimization");
                    return;
                }

                // For SQLite, we can optimize WAL mode and other pragmas
                await using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                
                // Optimize SQLite settings for performance
                var optimizations = new[]
                {
                    "PRAGMA journal_mode=WAL;",           // Write-Ahead Logging for better concurrency
                    "PRAGMA synchronous=NORMAL;",         // Balance between safety and performance
                    "PRAGMA cache_size=10000;",           // Increase cache size (10MB)
                    "PRAGMA temp_store=MEMORY;",          // Store temporary tables in memory
                    "PRAGMA mmap_size=268435456;",        // Enable memory-mapped I/O (256MB)
                    "PRAGMA optimize;"                     // Run SQLite optimizer
                };

                foreach (var pragma in optimizations)
                {
                    command.CommandText = pragma;
                    await command.ExecuteNonQueryAsync();
                    _logger.LogDebug("Executed optimization: {Pragma}", pragma);
                }

                _logger.LogInformation("Database connection pool optimization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing database connection pool");
            }
        }

        public async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, int timeoutSeconds = 30)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                
                var originalTimeout = _context.Database.GetCommandTimeout();
                _context.Database.SetCommandTimeout(timeoutSeconds);

                try
                {
                    return await operation();
                }
                finally
                {
                    _context.Database.SetCommandTimeout(originalTimeout);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Database operation timed out after {Timeout} seconds", timeoutSeconds);
                throw new TimeoutException($"Database operation timed out after {timeoutSeconds} seconds");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing database operation with timeout");
                throw;
            }
        }

        public async Task<List<T>> ExecuteBatchQueryAsync<T>(IQueryable<T> query, int batchSize = 1000)
        {
            try
            {
                var results = new List<T>();
                var skip = 0;

                while (true)
                {
                    var batch = await query
                        .Skip(skip)
                        .Take(batchSize)
                        .ToListAsync();

                    if (!batch.Any())
                        break;

                    results.AddRange(batch);
                    skip += batchSize;

                    _logger.LogDebug("Processed batch: {Skip}-{End}", skip - batchSize, skip);

                    // Small delay to prevent overwhelming the database
                    if (results.Count > batchSize)
                    {
                        await Task.Delay(10);
                    }
                }

                _logger.LogInformation("Batch query completed: {TotalRecords} records processed", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing batch query");
                throw;
            }
        }

        public async Task WarmupConnectionPoolAsync()
        {
            try
            {
                _logger.LogInformation("Warming up database connection pool...");

                // Execute a simple query to initialize the connection pool
                var warmupTasks = new List<Task>();

                for (int i = 0; i < 5; i++) // Create 5 concurrent connections
                {
                    warmupTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await using var connection = _context.Database.GetDbConnection();
                            await connection.OpenAsync();
                            
                            await using var command = connection.CreateCommand();
                            command.CommandText = "SELECT 1;";
                            await command.ExecuteScalarAsync();
                            
                            _logger.LogDebug("Connection pool warmup connection {Index} completed", i);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in connection pool warmup for connection {Index}", i);
                        }
                    }));
                }

                await Task.WhenAll(warmupTasks);
                _logger.LogInformation("Database connection pool warmup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up database connection pool");
            }
        }

        public async Task AnalyzeQueryPerformanceAsync()
        {
            try
            {
                _logger.LogInformation("Analyzing database query performance...");

                await using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                // For SQLite, we can analyze query plans and statistics
                var analysisQueries = new[]
                {
                    "ANALYZE;", // Update SQLite statistics
                    "PRAGMA optimize;", // Run SQLite query planner optimizer
                };

                foreach (var query in analysisQueries)
                {
                    await using var command = connection.CreateCommand();
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();
                    _logger.LogDebug("Executed analysis query: {Query}", query);
                }

                // Check for missing indexes on frequently queried columns
                await CheckForMissingIndexesAsync(connection);

                _logger.LogInformation("Database query performance analysis completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing database query performance");
            }
        }

        public async Task OptimizeIndexesAsync()
        {
            try
            {
                _logger.LogInformation("Optimizing database indexes...");

                await using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                // Check if commonly queried columns have indexes
                var indexOptimizations = new[]
                {
                    // Books table optimizations
                    "CREATE INDEX IF NOT EXISTS IX_Books_CategoryId_IsDeleted ON Books(CategoryId, IsDeleted);",
                    "CREATE INDEX IF NOT EXISTS IX_Books_Title_IsDeleted ON Books(Title, IsDeleted);",
                    "CREATE INDEX IF NOT EXISTS IX_Books_Author_IsDeleted ON Books(Author, IsDeleted);",
                    "CREATE INDEX IF NOT EXISTS IX_Books_Price_IsDeleted ON Books(Price, IsDeleted);",
                    "CREATE INDEX IF NOT EXISTS IX_Books_CreatedAt_IsDeleted ON Books(CreatedAt, IsDeleted);",
                    
                    // Orders table optimizations
                    "CREATE INDEX IF NOT EXISTS IX_Orders_UserId_Status ON Orders(UserId, Status);",
                    "CREATE INDEX IF NOT EXISTS IX_Orders_OrderDate_Status ON Orders(OrderDate, Status);",
                    
                    // UserFavorites table optimizations
                    "CREATE INDEX IF NOT EXISTS IX_UserFavorites_UserId_BookId ON UserFavorites(UserId, BookId);",
                    
                    // BookReviews table optimizations
                    "CREATE INDEX IF NOT EXISTS IX_BookReviews_BookId_IsDeleted ON BookReviews(BookId, IsDeleted);",
                    "CREATE INDEX IF NOT EXISTS IX_BookReviews_UserId_IsDeleted ON BookReviews(UserId, IsDeleted);",
                    
                    // Users table optimizations
                    "CREATE INDEX IF NOT EXISTS IX_Users_Email_IsDeleted ON Users(Email, IsDeleted);",
                    "CREATE INDEX IF NOT EXISTS IX_Users_Role_IsDeleted ON Users(Role, IsDeleted);"
                };

                foreach (var indexSql in indexOptimizations)
                {
                    try
                    {
                        await using var command = connection.CreateCommand();
                        command.CommandText = indexSql;
                        await command.ExecuteNonQueryAsync();
                        _logger.LogDebug("Created/verified index: {IndexSql}", indexSql.Split(' ')[5]); // Extract index name
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not create index: {IndexSql}", indexSql);
                    }
                }

                // Update statistics after index creation
                await using var analyzeCommand = connection.CreateCommand();
                analyzeCommand.CommandText = "ANALYZE;";
                await analyzeCommand.ExecuteNonQueryAsync();

                _logger.LogInformation("Database index optimization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing database indexes");
            }
        }

        private async Task CheckForMissingIndexesAsync(DbConnection connection)
        {
            try
            {
                // Query to check for tables without proper indexes
                var checkQuery = @"
                    SELECT name, sql 
                    FROM sqlite_master 
                    WHERE type='table' 
                    AND name NOT LIKE 'sqlite_%'
                    AND name NOT LIKE '__EFMigrationsHistory'";

                await using var command = connection.CreateCommand();
                command.CommandText = checkQuery;
                
                await using var reader = await command.ExecuteReaderAsync();
                var tables = new List<string>();
                
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0)); // Use index instead of column name
                }

                _logger.LogInformation("Found {TableCount} tables for index analysis", tables.Count);

                // Check indexes for each table
                foreach (var table in tables)
                {
                    await CheckTableIndexesAsync(connection, table);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for missing indexes");
            }
        }

        private async Task CheckTableIndexesAsync(DbConnection connection, string tableName)
        {
            try
            {
                var indexQuery = $"PRAGMA index_list('{tableName}');";
                
                await using var command = connection.CreateCommand();
                command.CommandText = indexQuery;
                
                await using var reader = await command.ExecuteReaderAsync();
                var indexCount = 0;
                
                while (await reader.ReadAsync())
                {
                    indexCount++;
                }

                if (indexCount == 0)
                {
                    _logger.LogWarning("Table {TableName} has no indexes - consider adding indexes for frequently queried columns", tableName);
                }
                else
                {
                    _logger.LogDebug("Table {TableName} has {IndexCount} indexes", tableName, indexCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking indexes for table {TableName}", tableName);
            }
        }
    }
}