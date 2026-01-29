namespace OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin
{
    public class SystemHealthViewModel
    {
        public DatabaseHealthViewModel DatabaseStatus { get; set; } = new();
        public CacheHealthViewModel CacheStatus { get; set; } = new();
        public StorageHealthViewModel StorageStatus { get; set; } = new();
        public PerformanceMetricsViewModel Performance { get; set; } = new();
        public List<SystemAlertViewModel> Alerts { get; set; } = new();
        
        // Overall health status
        public HealthStatus OverallStatus { get; set; }
        public string OverallStatusText => OverallStatus.ToString();
        public string OverallStatusClass => OverallStatus switch
        {
            HealthStatus.Healthy => "success",
            HealthStatus.Warning => "warning",
            HealthStatus.Critical => "danger",
            _ => "secondary"
        };
        
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;
        public TimeSpan Uptime { get; set; }
        public string UptimeText => FormatUptime(Uptime);
        
        // Additional properties for controller compatibility
        public string EmailServiceStatus { get; set; } = "Unknown";
        public TimeSpan SystemUptime 
        { 
            get => Uptime; 
            set => Uptime = value; 
        }
        public double MemoryUsage => Performance?.MemoryUsage ?? 0;
        public int ActiveUsers { get; set; }
        
        private string FormatUptime(TimeSpan uptime)
        {
            if (uptime.TotalDays >= 1)
                return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
            if (uptime.TotalHours >= 1)
                return $"{uptime.Hours}h {uptime.Minutes}m";
            return $"{uptime.Minutes}m {uptime.Seconds}s";
        }
    }
    
    public class DatabaseHealthViewModel
    {
        public HealthStatus Status { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int ActiveConnections { get; set; }
        public int MaxConnections { get; set; }
        public long DatabaseSize { get; set; }
        public string DatabaseSizeFormatted => FormatBytes(DatabaseSize);
        public DateTime LastBackup { get; set; }
        public List<string> Issues { get; set; } = new();
        
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
    
    public class CacheHealthViewModel
    {
        public HealthStatus Status { get; set; }
        public bool IsConnected { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public long MemoryUsed { get; set; }
        public long MemoryLimit { get; set; }
        public double MemoryUsagePercentage => MemoryLimit > 0 ? (double)MemoryUsed / MemoryLimit * 100 : 0;
        public int KeyCount { get; set; }
        public double HitRate { get; set; }
        public List<string> Issues { get; set; } = new();
    }
    
    public class StorageHealthViewModel
    {
        public HealthStatus Status { get; set; }
        public long TotalSpace { get; set; }
        public long UsedSpace { get; set; }
        public long FreeSpace => TotalSpace - UsedSpace;
        public double UsagePercentage => TotalSpace > 0 ? (double)UsedSpace / TotalSpace * 100 : 0;
        public string TotalSpaceFormatted => FormatBytes(TotalSpace);
        public string UsedSpaceFormatted => FormatBytes(UsedSpace);
        public string FreeSpaceFormatted => FormatBytes(FreeSpace);
        public List<string> Issues { get; set; } = new();
        
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
    
    public class PerformanceMetricsViewModel
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double AverageResponseTime { get; set; }
        public int RequestsPerMinute { get; set; }
        public int ErrorRate { get; set; }
        public int ActiveSessions { get; set; }
        public List<string> Issues { get; set; } = new();
    }
    
    public class SystemAlertViewModel
    {
        public int Id { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        
        public string SeverityClass => Severity switch
        {
            AlertSeverity.Info => "info",
            AlertSeverity.Warning => "warning",
            AlertSeverity.Error => "danger",
            AlertSeverity.Critical => "danger",
            _ => "secondary"
        };
        
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                return timeSpan.TotalMinutes switch
                {
                    < 1 => "Just now",
                    < 60 => $"{(int)timeSpan.TotalMinutes} minutes ago",
                    < 1440 => $"{(int)timeSpan.TotalHours} hours ago",
                    _ => CreatedAt.ToString("MMM dd, yyyy HH:mm")
                };
            }
        }
    }
    
    public enum HealthStatus
    {
        Healthy,
        Warning,
        Critical,
        Unknown
    }
    
    public enum AlertSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
}