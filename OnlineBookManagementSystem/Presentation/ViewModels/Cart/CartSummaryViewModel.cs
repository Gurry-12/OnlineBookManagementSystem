namespace OnlineBookManagementSystem.Presentation.ViewModels.Cart;

public class CartSummaryViewModel
{
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public int Shipping { get; set; }
    public decimal Tax { get; set; }
    public int ItemCount { get; set; }
    public decimal GrandTotal { get; set; }
}