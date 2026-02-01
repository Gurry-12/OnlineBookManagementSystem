using OnlineBookManagementSystem.Core.Application.DTOs;

namespace OnlineBookManagementSystem.Presentation.ViewModels.User
{
    /// <summary>
    /// ViewModel for user's favorite books page
    /// Prevents entity leakage by using DTOs
    /// </summary>
    public class FavoritesBooksViewModel
    {
        public List<FavoriteBookItemViewModel> FavoriteBooks { get; set; } = new();
        public int TotalFavorites { get; set; }
        public bool HasFavorites => FavoriteBooks.Any();
    }

    public class FavoriteBookItemViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public int StockQuantity { get; set; }
        public DateTime AddedToFavoritesDate { get; set; }
        
        // UI-specific computed properties
        public bool IsAvailable => StockQuantity > 0;
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= 5;
        public string StockBadgeClass => StockQuantity <= 0 ? "badge-danger" : 
                                         StockQuantity <= 5 ? "badge-warning" : "badge-success";
        public string StockBadgeText => StockQuantity <= 0 ? "Out of Stock" : 
                                        StockQuantity <= 5 ? "Low Stock" : "In Stock";
        public string FormattedPrice => $"₹{Price:N2}";
        public string FormattedDate => AddedToFavoritesDate.ToString("MMM dd, yyyy");
    }
}
