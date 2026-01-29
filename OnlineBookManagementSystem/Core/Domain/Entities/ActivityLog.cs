namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class ActivityLog : BaseEntity
    {
        private string _action = string.Empty;
        private string _message = string.Empty;

        public int? UserId { get; set; }
        
        public string Action 
        { 
            get => _action;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Action cannot be null or empty", nameof(value));
                if (value.Length > 100)
                    throw new ArgumentException("Action cannot exceed 100 characters", nameof(value));
                _action = value.Trim();
            }
        }

        public string Message 
        { 
            get => _message;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Message cannot be null or empty", nameof(value));
                if (value.Length > 1000)
                    throw new ArgumentException("Message cannot exceed 1000 characters", nameof(value));
                _message = value.Trim();
            }
        }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string Level { get; set; } = "Info";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Additional properties for compatibility
        public string ActionType => Action;
        public string Description => Message;

        // Navigation property
        public virtual User? User { get; set; }

        // Private constructor for EF Core
        public ActivityLog() { }

        public ActivityLog(string action, string message, int? userId = null, string? ipAddress = null, string? userAgent = null, string level = "Info")
        {
            Action = action;
            Message = message;
            UserId = userId;
            IpAddress = ipAddress?.Length > 45 ? ipAddress[..45] : ipAddress;
            UserAgent = userAgent?.Length > 500 ? userAgent[..500] : userAgent;
            Level = level;
            Timestamp = DateTime.UtcNow;
        }

        public void UpdateLevel(string level)
        {
            if (string.IsNullOrWhiteSpace(level))
                throw new ArgumentException("Level cannot be null or empty", nameof(level));
            if (level.Length > 20)
                throw new ArgumentException("Level cannot exceed 20 characters", nameof(level));
            
            Level = level.Trim();
            UpdateTimestamp();
        }
    }
}