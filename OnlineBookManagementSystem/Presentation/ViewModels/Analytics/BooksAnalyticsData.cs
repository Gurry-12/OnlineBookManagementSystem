using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Analytics;

public class BooksAnalyticsData
{
    public List<MonthlyBookUploadViewModel> MonthlyUploads { get; set; } = new();
    public List<CategoryBookCountViewModel> CategoryDistribution { get; set; } = new();
    public List<AuthorBookCountViewModel> AuthorDistribution { get; set; } = new();
    public FavoriteStatsViewModel FavoriteStats { get; set; } = new();

    // Summary metrics
    public int TotalBooks { get; set; }
    public int ActiveBooks { get; set; }
    public int BooksThisMonth { get; set; }
    public int TotalCategories { get; set; }
    public int TotalAuthors { get; set; }

    // Popularity metrics
    public List<PopularBookViewModel> MostPopularBooks { get; set; } = new();
    public List<PopularBookViewModel> MostFavoritedBooks { get; set; } = new();
}

public class PopularBookViewModel
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public int OrderCount { get; set; }
    public double AverageRating { get; set; }
}