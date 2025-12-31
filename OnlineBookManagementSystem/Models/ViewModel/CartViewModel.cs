namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class CartViewModel
    {
        public List<ShoppingCartViewModel> CartItems { get; set; } = new();
        public CartSummaryViewModel Summary { get; set; } = new();
    }
}