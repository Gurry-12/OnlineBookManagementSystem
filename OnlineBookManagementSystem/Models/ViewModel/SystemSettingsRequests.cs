using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class GeneralSettingsRequest
    {
        [Required]
        [StringLength(100)]
        public string SiteName { get; set; } = string.Empty;

        [StringLength(500)]
        public string SiteDescription { get; set; } = string.Empty;

        [StringLength(200)]
        public string SiteKeywords { get; set; } = string.Empty;

        [StringLength(100)]
        public string AdminEmail { get; set; } = string.Empty;

        [StringLength(50)]
        public string TimeZone { get; set; } = "UTC";

        [StringLength(10)]
        public string DateFormat { get; set; } = "MM/dd/yyyy";

        [StringLength(10)]
        public string TimeFormat { get; set; } = "HH:mm";

        public bool MaintenanceMode { get; set; } = false;

        [StringLength(500)]
        public string MaintenanceMessage { get; set; } = string.Empty;
    }

    public class SecuritySettingsRequest
    {
        public bool RequireEmailConfirmation { get; set; } = true;

        public bool EnableTwoFactorAuth { get; set; } = false;

        public int PasswordMinLength { get; set; } = 6;

        public bool RequireUppercase { get; set; } = false;

        public bool RequireLowercase { get; set; } = false;

        public bool RequireNumbers { get; set; } = false;

        public bool RequireSpecialChars { get; set; } = false;

        public int MaxLoginAttempts { get; set; } = 5;

        public int LockoutDurationMinutes { get; set; } = 15;

        public int SessionTimeoutMinutes { get; set; } = 30;

        public bool EnableCaptcha { get; set; } = false;

        public string? CaptchaSiteKey { get; set; }

        public string? CaptchaSecretKey { get; set; }
    }

    public class EmailSettingsRequest
    {
        [Required]
        [StringLength(100)]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        [Required]
        [StringLength(100)]
        public string SmtpUsername { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SmtpPassword { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;

        [Required]
        [StringLength(100)]
        public string FromEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FromName { get; set; } = string.Empty;

        public bool EnableEmailNotifications { get; set; } = true;

        public bool SendWelcomeEmail { get; set; } = true;

        public bool SendOrderConfirmation { get; set; } = true;

        public bool SendPasswordReset { get; set; } = true;

        [StringLength(500)]
        public string EmailSignature { get; set; } = string.Empty;
    }
}