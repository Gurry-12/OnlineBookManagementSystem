using OnlineBookManagementSystem.Presentation.ViewModels.Cart;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin;

public class AdminCartViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public DateTime AddedAt { get; set; }
    public int Quantity { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<CartItemViewModel> Items { get; set; } = new();
}