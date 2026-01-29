namespace OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;

public class FavoriteStatsViewModel
{
    public Dictionary<string, int> FavoriteData { get; set; } = new Dictionary<string, int>();
    public int TotalFavorites { get; set; }
    public string MostFavoritedBook { get; set; } = string.Empty;
    public int UniqueFavoritedBooks { get; set; }
    public int UsersWithFavorites { get; set; }
    public int FavoriteCount { get; set; } // Separate property for compatibility
    public int NonFavoriteCount { get; set; } // For charts showing favorite vs non-favorite
}
