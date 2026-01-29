namespace OnlineBookManagementSystem.Presentation.ViewModels.Cart;

public class CartViewModel
{
    public List<ShoppingCartViewModel> CartItems { get; set; } = new();
    public CartSummaryViewModel Summary { get; set; } = new();
}