using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.Configuration; // Added for EmailSettings
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces;

public interface ISystemSettingsService
{
    Task<SystemSettingsViewModel> GetSystemSettingsAsync();

    // New method for clean EmailSettings retrieval
    Task<EmailSettings> GetEmailSettingsAsync();

    Task<bool> UpdateGeneralSettingsAsync(Models.ViewModel.GeneralSettingsRequest request);
    Task<bool> UpdateSecuritySettingsAsync(Models.ViewModel.SecuritySettingsRequest request);
    Task<bool> UpdateEmailSettingsAsync(Models.ViewModel.EmailSettingsRequest request);
    Task<(bool Success, string Message)> TestEmailConfigurationAsync();
    Task ClearCacheAsync();
    Task<(bool Success, string Message)> BackupDatabaseAsync();
}
