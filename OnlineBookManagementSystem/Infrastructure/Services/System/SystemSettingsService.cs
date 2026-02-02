using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MimeKit;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Infrastructure.Data.Context.Configuration;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace OnlineBookManagementSystem.Infrastructure.Services.System;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SystemSettingsService> _logger;
    private readonly BookManagementContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly EmailSettings _fallbackSettings;
    private readonly IDataProtector _protector;

    public SystemSettingsService(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<SystemSettingsService> logger,
        BookManagementContext context,
        IWebHostEnvironment environment,
        IOptions<EmailSettings> fallbackSettings,
        IDataProtectionProvider dataProtectionProvider)
    {
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
        _context = context;
        _environment = environment;
        _fallbackSettings = fallbackSettings.Value;
        _protector = dataProtectionProvider.CreateProtector("OnlineBookManagementSystem.EmailSettings.SmtpPassword");
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

    public async Task<EmailSettings> GetEmailSettingsAsync()
    {
        // 1. Try to get from Cache
        if (_cache.TryGetValue("EmailSettings", out EmailSettings? cachedSettings) && cachedSettings != null)
        {
            return cachedSettings;
        }

        // 2. Try to get from DB
        try
        {
            var dbSettings = await _context.SystemSettings.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (dbSettings != null)
            {
                var settings = new EmailSettings
                {
                    SmtpHost = dbSettings.SmtpHost,
                    SmtpPort = dbSettings.SmtpPort,
                    SmtpUsername = dbSettings.SmtpUsername,
                    SmtpPassword = TryDecrypt(dbSettings.SmtpPassword),
                    EnableSsl = dbSettings.EnableSsl,
                    SenderName = dbSettings.SenderName,
                    SenderEmail = dbSettings.SenderEmail
                };

                _cache.Set("EmailSettings", settings, TimeSpan.FromHours(1));
                return settings;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings from DB");
        }

        // 3. Fallback to AppSettings / Environment / IOptions defaults
        var fallback = new EmailSettings
        {
            SmtpHost = _configuration["Email:SmtpHost"] ?? _fallbackSettings.SmtpHost,
            SmtpPort = _configuration.GetValue<int?>("Email:SmtpPort") ?? _fallbackSettings.SmtpPort,
            SmtpUsername = _configuration["Email:SmtpUsername"] ?? _fallbackSettings.SmtpUsername,
            SmtpPassword = _configuration["Email:SmtpPassword"] ?? _fallbackSettings.SmtpPassword,
            EnableSsl = _configuration.GetValue<bool?>("Email:EnableSsl") ?? _fallbackSettings.EnableSsl,
            SenderName = _configuration["SiteName"] ?? _fallbackSettings.SenderName,
            SenderEmail = _configuration["ContactEmail"] ?? _fallbackSettings.SenderEmail
        };

        if (string.IsNullOrEmpty(fallback.SenderEmail)) fallback.SenderEmail = fallback.SmtpUsername;

        return fallback;
    }

    public async Task<bool> UpdateGeneralSettingsAsync(GeneralSettingsRequest request)
    {
        try
        {
            // Update database
            var dbSettings = await _context.SystemSettings.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (dbSettings == null)
            {
                dbSettings = new SystemSettings();
                _context.SystemSettings.Add(dbSettings);
            }

            dbSettings.SiteName = request.SiteName;
            dbSettings.SiteDescription = request.SiteDescription;
            dbSettings.AdminEmail = request.AdminEmail;
            dbSettings.MaintenanceMode = request.MaintenanceMode;
            dbSettings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update cache
            _cache.Set("SiteName", request.SiteName, TimeSpan.FromHours(24));
            _cache.Set("SiteDescription", request.SiteDescription, TimeSpan.FromHours(24));
            _cache.Set("AdminEmail", request.AdminEmail, TimeSpan.FromHours(24));
            _cache.Set("MaintenanceMode", request.MaintenanceMode, TimeSpan.FromHours(24));

            _logger.LogInformation("General settings updated in database and cache");
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
            if (request.PasswordMinLength < 6 || request.PasswordMinLength > 50) return false;
            if (request.MaxLoginAttempts < 3 || request.MaxLoginAttempts > 10) return false;
            if (request.LockoutDurationMinutes < 5 || request.LockoutDurationMinutes > 60) return false;

            // Update database
            var dbSettings = await _context.SystemSettings.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (dbSettings == null)
            {
                dbSettings = new SystemSettings();
                _context.SystemSettings.Add(dbSettings);
            }

            dbSettings.PasswordMinLength = request.PasswordMinLength;
            dbSettings.MaxLoginAttempts = request.MaxLoginAttempts;
            dbSettings.LockoutDurationMinutes = request.LockoutDurationMinutes;
            dbSettings.RequireEmailConfirmation = request.RequireEmailConfirmation;
            dbSettings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update cache
            _cache.Set("Security:PasswordMinLength", request.PasswordMinLength, TimeSpan.FromHours(24));
            _cache.Set("Security:MaxLoginAttempts", request.MaxLoginAttempts, TimeSpan.FromHours(24));
            _cache.Set("Security:LockoutDurationMinutes", request.LockoutDurationMinutes, TimeSpan.FromHours(24));
            _cache.Set("Security:RequireEmailConfirmation", request.RequireEmailConfirmation, TimeSpan.FromHours(24));

            _logger.LogInformation("Security settings updated in database and cache");
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
            // Update DB
            var dbSettings = await _context.SystemSettings.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (dbSettings == null)
            {
                dbSettings = new SystemSettings();
                _context.SystemSettings.Add(dbSettings);
            }

            dbSettings.SmtpHost = request.SmtpHost;
            dbSettings.SmtpPort = request.SmtpPort;
            dbSettings.SmtpUsername = request.SmtpUsername;
            if (!string.IsNullOrEmpty(request.SmtpPassword))
            {
                dbSettings.SmtpPassword = _protector.Protect(request.SmtpPassword);
            }
            dbSettings.EnableSsl = request.EnableSsl;
            dbSettings.SenderEmail = request.FromEmail;
            dbSettings.SenderName = request.FromName;
            dbSettings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Invalidate Cache
            _cache.Remove("EmailSettings");

            _logger.LogInformation("Email settings updated in DB: SmtpHost={SmtpHost}", request.SmtpHost);

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
            var settings = await GetEmailSettingsAsync();

            if (string.IsNullOrEmpty(settings.SmtpHost) || string.IsNullOrEmpty(settings.SmtpUsername))
            {
                return (false, "SMTP configuration is incomplete");
            }

            using var client = new SmtpClient();

            SecureSocketOptions socketOptions = settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            if (settings.SmtpPort == 465) socketOptions = SecureSocketOptions.SslOnConnect;

            await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, socketOptions);
            await client.AuthenticateAsync(settings.SmtpUsername, settings.SmtpPassword);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.SenderName, settings.SmtpUsername));
            message.To.Add(MailboxAddress.Parse(settings.SmtpUsername));
            message.Subject = "Test Email Configuration";
            message.Body = new TextPart("plain") { Text = "This is a test email to verify SMTP configuration." };

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Test email sent successfully to {Email}", settings.SmtpUsername);
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
            var cacheKeys = new[]
            {
                "allBooks", "allCategories", "featuredBooks", "newArrivals",
                "stats_", "user_", "book_", "category_"
            };

            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field?.GetValue(memoryCache) is object coherentState)
                {
                    var entriesCollection = coherentState.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (entriesCollection?.GetValue(coherentState) is IDictionary entries)
                    {
                        var keysToRemove = new List<object>();
                        foreach (DictionaryEntry entry in entries)
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

    private string TryDecrypt(string cipherText)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;
            return _protector.Unprotect(cipherText);
        }
        catch
        {
            // If decryption fails (e.g., old format or key change), return original or empty
            return cipherText;
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
        catch { return "1.0.0"; }
    }

    private async Task<string> GetDatabaseVersionAsync()
    {
        try
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT sqlite_version()";
            var result = await command.ExecuteScalarAsync();
            return $"SQLite {result}";
        }
        catch { return "Unknown"; }
    }

    private string GetServerUptime()
    {
        try
        {
            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }
        catch { return "Unknown"; }
    }
}
