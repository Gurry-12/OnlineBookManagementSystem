using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Models
{
    public class SystemSettings
    {
        [Key]
        public int Id { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty; // Encrypted ideally
        public bool EnableSsl { get; set; } = true;
        public string SenderName { get; set; } = "Whispering Pages";
        public string SenderEmail { get; set; } = string.Empty;

        // General Settings could also go here
        public string SiteName { get; set; } = "Whispering Pages";
        public string ContactEmail { get; set; } = string.Empty;
        public bool MaintenanceMode { get; set; } = false;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
