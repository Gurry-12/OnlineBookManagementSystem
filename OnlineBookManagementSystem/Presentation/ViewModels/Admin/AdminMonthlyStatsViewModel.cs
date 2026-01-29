using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin;

public class AdminMonthlyStatsViewModel
{
    public int Year { get; set; }
    public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new Dictionary<string, decimal>();
    public Dictionary<string, int> MonthlyBookUploads { get; set; } = new Dictionary<string, int>();
    public decimal TotalRevenue { get; set; }
    public int TotalBooks { get; set; }
    public List<MonthlyBookUploadViewModel> MonthlyUploads { get; set; } = new();
    public List<CategoryBookCountViewModel> CategoryDistribution { get; set; } = new();
    public List<AuthorBookCountViewModel> AuthorDistribution { get; set; } = new();
    public FavoriteStatsViewModel FavoriteStats { get; set; } = new();
}
