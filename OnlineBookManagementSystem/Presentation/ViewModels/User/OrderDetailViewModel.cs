namespace OnlineBookManagementSystem.Presentation.ViewModels.User
{
    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string BookISBN { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string BookImageUrl { get; set; } = string.Empty;
        
        // Navigation property for compatibility
        public BookViewModel? Book { get; set; }
        
        // Computed properties
        public decimal TotalPrice => UnitPrice * Quantity;
        public string FormattedUnitPrice => UnitPrice.ToString("C");
        public string FormattedSubtotal => Subtotal.ToString("C");
        public string FormattedTotalPrice => TotalPrice.ToString("C");
    }
    
    public class BookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}