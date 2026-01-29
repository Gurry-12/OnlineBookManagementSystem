using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;

namespace OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin
{
    public class SuperAdminAnalyticsViewModel
    {
        public List<UserGrowthDataViewModel> UserGrowthData { get; set; } = new();
        public List<RevenueGrowthDataViewModel> RevenueGrowthData { get; set; } = new();
        public List<BookPopularityDataViewModel> BookPopularityData { get; set; } = new();
        public List<CategoryDistributionViewModel> CategoryDistribution { get; set; } = new();
        
        // Summary statistics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBooks { get; set; }
        public int TotalOrders { get; set; }
        
        // Growth metrics
        public double UserGrowthRate { get; set; }
        public double RevenueGrowthRate { get; set; }
        public double OrderGrowthRate { get; set; }
        
        // Time period
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = "Last 30 Days";
    }
    
    public class UserGrowthDataViewModel
    {
        public DateTime Date { get; set; }
        public int NewUsers { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
    }
    
    public class RevenueGrowthDataViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
    
    public class BookPopularityDataViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int OrderCount { get; set; }
        public int FavoriteCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
    
    public class CategoryDistributionViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BookCount { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public double Percentage { get; set; }
    }
}