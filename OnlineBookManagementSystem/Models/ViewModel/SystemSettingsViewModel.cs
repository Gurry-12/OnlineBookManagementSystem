namespace OnlineBookManagementSystem.Models.ViewModel;

public class SystemSettingsViewModel
{
    // General Settings
    public string SiteName { get; set; } = "Whispering Pages";
    public string SiteDescription { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool MaintenanceMode { get; set; }

    // Security Settings
    public int JwtExpiryMinutes { get; set; } = 60;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public bool RequireEmailConfirmation { get; set; }

    // Email Settings
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;

    // System Information
    public string AppVersion { get; set; } = "1.0.0";
    public string DatabaseVersion { get; set; } = string.Empty;
    public string ServerUptime { get; set; } = string.Empty;
    public string Environment { get; set; } = "Development";
}