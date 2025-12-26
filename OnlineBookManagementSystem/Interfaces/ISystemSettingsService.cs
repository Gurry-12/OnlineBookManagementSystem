using OnlineBookManagementSystem.Controllers;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces;

public interface ISystemSettingsService
{
    Task<SystemSettingsViewModel> GetSystemSettingsAsync();
    Task<bool> UpdateGeneralSettingsAsync(GeneralSettingsRequest request);
    Task<bool> UpdateSecuritySettingsAsync(SecuritySettingsRequest request);
    Task<bool> UpdateEmailSettingsAsync(EmailSettingsRequest request);
    Task<(bool Success, string Message)> TestEmailConfigurationAsync();
    Task ClearCacheAsync();
    Task<(bool Success, string Message)> BackupDatabaseAsync();
}