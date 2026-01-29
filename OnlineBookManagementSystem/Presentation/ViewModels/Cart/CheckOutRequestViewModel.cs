namespace OnlineBookManagementSystem.Presentation.ViewModels.Cart;

public class CheckOutRequestViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    
    // Extended address fields
    public string FullName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
}