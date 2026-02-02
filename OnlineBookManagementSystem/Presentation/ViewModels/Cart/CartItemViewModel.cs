using OnlineBookManagementSystem.Shared.Utilities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Cart
{
    /// <summary>
    /// View model for individual cart items in admin views
    /// </summary>
    public class CartItemViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty; // Keep for backward compatibility
        public int Quantity { get; set; }
        public decimal BookPrice { get; set; }
        public decimal Price { get; set; } // Keep for backward compatibility
        public decimal Subtotal { get; set; }
        public string? BookImage { get; set; }
        public string ImageUrl { get; set; } = "/images/default-book.png"; // Keep for backward compatibility
        public string CategoryName { get; set; } = "Uncategorized";
        public bool IsAvailable { get; set; } = true;

        // Computed Properties with Formatting
        public string FormattedBookPrice => FormattingExtensions.FormatCurrency(BookPrice != 0 ? BookPrice : Price);
        public string FormattedSubtotal => FormattingExtensions.FormatCurrency(Subtotal);
        public string DisplayAuthor => !string.IsNullOrEmpty(BookAuthor) ? BookAuthor : Author;
        public string DisplayImage => !string.IsNullOrEmpty(BookImage) ? BookImage : ImageUrl;
    }
}