namespace OnlineBookManagementSystem.Presentation.ViewModels.Cart;

public class ShoppingCartViewModel
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? BookAuthor { get; set; }
    public decimal Price { get; set; }
    public decimal BookPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? BookImage { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public decimal Subtotal { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}