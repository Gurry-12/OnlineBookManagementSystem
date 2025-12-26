using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineBookManagementSystem.Controllers;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Net.Mail;
using System.Reflection;

namespace OnlineBookManagementSystem.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SystemSettingsService> _logger;
    private readonly BookManagementContext _context;
    private readonly IWebHostEnvironment _environment;

    public SystemSettingsService(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<SystemSettingsService> logger,
        BookManagementContext context,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
        _context = context;
        _environment = environment;
    }

    public async Task<SystemSettingsViewModel> GetSystemSettingsAsync()
    {
        return new SystemSettingsViewModel
        {
            // General Settings
            SiteName = _configuration["SiteName"] ?? "Whispering Pages",
            SiteDescription = _configuration["SiteDescription"] ?? "",
            ContactEmail = _configuration["ContactEmail"] ?? "",
            MaintenanceMode = _configuration.GetValue<bool>("MaintenanceMode"),

            // Security Settings
            JwtExpiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60),
            MaxLoginAttempts = _configuration.GetValue<int>("Security:MaxLoginAttempts", 5),
            LockoutDurationMinutes = _configuration.GetValue<int>("Security:LockoutDurationMinutes", 15),
            RequireEmailConfirmation = _configuration.GetValue<bool>("Security:RequireEmailConfirmation"),

            // Email Settings
            SmtpHost = _configuration["Email:SmtpHost"] ?? "",
            SmtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587),
            SmtpUsername = _configuration["Email:SmtpUsername"] ?? "",
            EnableSsl = _configuration.GetValue<bool>("Email:EnableSsl", true),

            // System Information
            AppVersion = GetAppVersion(),
            DatabaseVersion = await GetDatabaseVersionAsync(),
            ServerUptime = GetServerUptime(),
            Environment = _environment.EnvironmentName
        };
    }

    public async Task<bool> UpdateGeneralSettingsAsync(GeneralSettingsRequest request)
    {
        try
        {
            // In a real application, you would save these to a database or configuration file
            // For now, we'll just cache them and log the changes
            
            _cache.Set("SiteName", request.SiteName, TimeSpan.FromHours(24));
            _cache.Set("SiteDescription", request.SiteDescription, TimeSpan.FromHours(24));
            _cache.Set("ContactEmail", request.ContactEmail, TimeSpan.FromHours(24));
            _cache.Set("MaintenanceMode", request.MaintenanceMode, TimeSpan.FromHours(24));

            _logger.LogInformation("General settings updated: SiteName={SiteName}, MaintenanceMode={MaintenanceMode}", 
                request.SiteName, request.MaintenanceMode);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update general settings");
            return false;
        }
    }

    public async Task<bool> UpdateSecuritySettingsAsync(SecuritySettingsRequest request)
    {
        try
        {
            // Validate settings
            if (request.JwtExpiry < 5 || request.JwtExpiry > 1440)
                return false;

            if (request.MaxLoginAttempts < 3 || request.MaxLoginAttempts > 10)
                return false;

            if (request.LockoutDuration < 5 || request.LockoutDuration > 60)
                return false;

            _cache.Set("Jwt:ExpiryMinutes", request.JwtExpiry, TimeSpan.FromHours(24));
            _cache.Set("Security:MaxLoginAttempts", request.MaxLoginAttempts, TimeSpan.FromHours(24));
            _cache.Set("Security:LockoutDurationMinutes", request.LockoutDuration, TimeSpan.FromHours(24));
            _cache.Set("Security:RequireEmailConfirmation", request.RequireEmailConfirmation, TimeSpan.FromHours(24));

            _logger.LogInformation("Security settings updated: JwtExpiry={JwtExpiry}, MaxLoginAttempts={MaxLoginAttempts}", 
                request.JwtExpiry, request.MaxLoginAttempts);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update security settings");
            return false;
        }
    }

    public async Task<bool> UpdateEmailSettingsAsync(EmailSettingsRequest request)
    {
        try
        {
            _cache.Set("Email:SmtpHost", request.SmtpHost, TimeSpan.FromHours(24));
            _cache.Set("Email:SmtpPort", request.SmtpPort, TimeSpan.FromHours(24));
            _cache.Set("Email:SmtpUsername", request.SmtpUsername, TimeSpan.FromHours(24));
            _cache.Set("Email:EnableSsl", request.EnableSsl, TimeSpan.FromHours(24));

            // Don't cache password in plain text - in production, use secure storage
            if (!string.IsNullOrEmpty(request.SmtpPassword))
            {
                _cache.Set("Email:SmtpPassword", request.SmtpPassword, TimeSpan.FromHours(24));
            }

            _logger.LogInformation("Email settings updated: SmtpHost={SmtpHost}, SmtpPort={SmtpPort}", 
                request.SmtpHost, request.SmtpPort);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update email settings");
            return false;
        }
    }

    public async Task<(bool Success, string Message)> TestEmailConfigurationAsync()
    {
        try
        {
            var smtpHost = _cache.Get<string>("Email:SmtpHost") ?? _configuration["Email:SmtpHost"];
            var smtpPort = _cache.Get<int?>("Email:SmtpPort") ?? _configuration.GetValue<int>("Email:SmtpPort", 587);
            var smtpUsername = _cache.Get<string>("Email:SmtpUsername") ?? _configuration["Email:SmtpUsername"];
            var smtpPassword = _cache.Get<string>("Email:SmtpPassword") ?? _configuration["Email:SmtpPassword"];
            var enableSsl = _cache.Get<bool?>("Email:EnableSsl") ?? _configuration.GetValue<bool>("Email:EnableSsl", true);

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername))
            {
                return (false, "SMTP configuration is incomplete");
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword)
            };

            var testMessage = new MailMessage
            {
                From = new MailAddress(smtpUsername, "Whispering Pages"),
                Subject = "Test Email Configuration",
                Body = "This is a test email to verify SMTP configuration.",
                IsBodyHtml = false
            };

            testMessage.To.Add(smtpUsername); // Send to self

            await client.SendMailAsync(testMessage);

            _logger.LogInformation("Test email sent successfully to {Email}", smtpUsername);
            return (true, "Test email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email");
            return (false, $"Failed to send test email: {ex.Message}");
        }
    }

    public async Task ClearCacheAsync()
    {
        try
        {
            // Clear specific cache entries
            var cacheKeys = new[]
            {
                "allBooks", "allCategories", "featuredBooks", "newArrivals",
                "stats_", "user_", "book_", "category_"
            };

            if (_cache is MemoryCache memoryCache)
            {
                // Get all cache entries (this is a simplified approach)
                var field = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field?.GetValue(memoryCache) is object coherentState)
                {
                    var entriesCollection = coherentState.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (entriesCollection?.GetValue(coherentState) is System.Collections.IDictionary entries)
                    {
                        var keysToRemove = new List<object>();
                        foreach (System.Collections.DictionaryEntry entry in entries)
                        {
                            var key = entry.Key.ToString();
                            if (cacheKeys.Any(ck => key?.Contains(ck) == true))
                            {
                                keysToRemove.Add(entry.Key);
                            }
                        }

                        foreach (var key in keysToRemove)
                        {
                            _cache.Remove(key);
                        }
                    }
                }
            }

            _logger.LogInformation("Cache cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache");
            throw;
        }
    }

    public async Task<(bool Success, string Message)> BackupDatabaseAsync()
    {
        try
        {
            var backupDir = Path.Combine(_environment.ContentRootPath, "Backups");
            Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"whisperingpages_backup_{timestamp}.db";
            var backupPath = Path.Combine(backupDir, backupFileName);

            // For SQLite, we can simply copy the database file
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (connectionString?.Contains("Data Source=") == true)
            {
                var dbPath = connectionString.Split("Data Source=")[1].Split(';')[0];
                if (File.Exists(dbPath))
                {
                    File.Copy(dbPath, backupPath, true);
                    
                    _logger.LogInformation("Database backup created: {BackupPath}", backupPath);
                    return (true, $"Database backup created successfully: {backupFileName}");
                }
            }

            return (false, "Database file not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to backup database");
            return (false, $"Failed to backup database: {ex.Message}");
        }
    }

    private string GetAppVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }

    private async Task<string> GetDatabaseVersionAsync()
    {
        try
        {
            // For SQLite, we can get the version from the database
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT sqlite_version()";
            var result = await command.ExecuteScalarAsync();
            return $"SQLite {result}";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string GetServerUptime()
    {
        try
        {
            var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }
        catch
        {
            return "Unknown";
        }
    }
}