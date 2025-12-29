using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.Configuration; // Added for EmailSettings
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces;

public interface ISystemSettingsService
{
    Task<SystemSettingsViewModel> GetSystemSettingsAsync();

    // New method for clean EmailSettings retrieval
    Task<EmailSettings> GetEmailSettingsAsync();

    Task<bool> UpdateGeneralSettingsAsync(GeneralSettingsRequest request);
    Task<bool> UpdateSecuritySettingsAsync(SecuritySettingsRequest request);
    Task<bool> UpdateEmailSettingsAsync(EmailSettingsRequest request);
    Task<(bool Success, string Message)> TestEmailConfigurationAsync();
    Task ClearCacheAsync();
    Task<(bool Success, string Message)> BackupDatabaseAsync();
}
