namespace OnlineBookManagementSystem.Presentation.ViewModels.Analytics;

public class UsersAnalyticsData
{
    public List<UserGrowthDataViewModel> UserGrowthData { get; set; } = new();
    public List<UserActivityTrend> ActivityTrends { get; set; } = new();
    public List<RoleDistribution> RoleDistribution { get; set; } = new();

    // Summary metrics (sensitive)
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int InactiveUsers { get; set; }

    // Growth metrics (sensitive)
    public double UserGrowthRate { get; set; }
    public double ActivityRate { get; set; }
    public double RetentionRate { get; set; }
}

public class UserGrowthDataViewModel
{
    public DateTime Date { get; set; }
    public int NewUsers { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
}

public class UserActivityTrend
{
    public DateTime Date { get; set; }
    public int ActiveUsers { get; set; }
    public int LoginCount { get; set; }
    public int OrderCount { get; set; }
}

public class RoleDistribution
{
    public string Role { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}