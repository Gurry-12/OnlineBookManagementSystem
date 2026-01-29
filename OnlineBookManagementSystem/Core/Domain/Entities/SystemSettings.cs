namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class SystemSettings : BaseEntity
    {
        private string _smtpHost = string.Empty;
        private string _smtpUsername = string.Empty;
        private string _smtpPassword = string.Empty;
        private string _senderName = "Whispering Pages";
        private string _senderEmail = string.Empty;
        private string _siteName = "Whispering Pages";
        private string _contactEmail = string.Empty;

        public string SmtpHost 
        { 
            get => _smtpHost;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("SMTP Host cannot be null or empty", nameof(value));
                _smtpHost = value.Trim();
            }
        }

        public int SmtpPort { get; set; } = 587;

        public string SmtpUsername 
        { 
            get => _smtpUsername;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("SMTP Username cannot be null or empty", nameof(value));
                _smtpUsername = value.Trim();
            }
        }

        public string SmtpPassword 
        { 
            get => _smtpPassword;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("SMTP Password cannot be null or empty", nameof(value));
                _smtpPassword = value;
            }
        }

        public bool EnableSsl { get; set; } = true;

        public string SenderName 
        { 
            get => _senderName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Sender Name cannot be null or empty", nameof(value));
                _senderName = value.Trim();
            }
        }

        public string SenderEmail 
        { 
            get => _senderEmail;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Sender Email cannot be null or empty", nameof(value));
                _senderEmail = value.Trim();
            }
        }

        public string SiteName 
        { 
            get => _siteName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Site Name cannot be null or empty", nameof(value));
                _siteName = value.Trim();
            }
        }

        public string ContactEmail 
        { 
            get => _contactEmail;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Contact Email cannot be null or empty", nameof(value));
                _contactEmail = value.Trim();
            }
        }

        public bool MaintenanceMode { get; set; } = false;

        // Private constructor for EF Core
        public SystemSettings() { }

        public SystemSettings(string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, 
                            string senderName, string senderEmail, string siteName, string contactEmail)
        {
            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            SmtpUsername = smtpUsername;
            SmtpPassword = smtpPassword;
            SenderName = senderName;
            SenderEmail = senderEmail;
            SiteName = siteName;
            ContactEmail = contactEmail;
        }

        public void UpdateSmtpSettings(string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, bool enableSsl = true)
        {
            if (smtpPort <= 0 || smtpPort > 65535)
                throw new ArgumentException("SMTP Port must be between 1 and 65535", nameof(smtpPort));

            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            SmtpUsername = smtpUsername;
            SmtpPassword = smtpPassword;
            EnableSsl = enableSsl;
            UpdateTimestamp();
        }

        public void UpdateSenderInfo(string senderName, string senderEmail)
        {
            SenderName = senderName;
            SenderEmail = senderEmail;
            UpdateTimestamp();
        }

        public void UpdateSiteInfo(string siteName, string contactEmail)
        {
            SiteName = siteName;
            ContactEmail = contactEmail;
            UpdateTimestamp();
        }

        public void SetMaintenanceMode(bool maintenanceMode)
        {
            MaintenanceMode = maintenanceMode;
            UpdateTimestamp();
        }
    }
}