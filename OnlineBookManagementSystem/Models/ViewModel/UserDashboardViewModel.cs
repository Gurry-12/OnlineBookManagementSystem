namespace OnlineBookManagementSystem.Models.ViewModel;

public class UserDashboardViewModel
{
    public int TotalBooks { get; set; }
    public int FavoritesCount { get; set; }
    public int OrdersCount { get; set; }
    public int CartItemsCount { get; set; }
    public decimal TotalSpent { get; set; }
    
    public List<Book> FeaturedBooks { get; set; } = new();
    public List<Order> RecentOrders { get; set; } = new();
    public List<CategoryWithCount> Categories { get; set; } = new();
    public List<Book> RecommendedBooks { get; set; } = new();
    public List<Book> NewArrivals { get; set; } = new();
}