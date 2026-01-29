using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Cart;

public class CheckOutViewModel
{
    public List<ShoppingCartViewModel> CartItems { get; set; } = new();
    public CartSummaryViewModel Summary { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public int Shipping { get; set; }
    public decimal GrandTotal { get; set; }
    public int UserId { get; set; }
    
    // Extended address fields
    public string FullName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public Order? Order { get; set; }
}