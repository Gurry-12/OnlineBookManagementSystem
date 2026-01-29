using OnlineBookManagementSystem.Infrastructure.Data.Context.Configuration;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;

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
