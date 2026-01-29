namespace OnlineBookManagementSystem.Infrastructure.Data.Context.Configuration;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";
    
    public bool EnableRateLimiting { get; set; }
    public int PermitLimit { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public int QueueLimit { get; set; } = 10;
}
