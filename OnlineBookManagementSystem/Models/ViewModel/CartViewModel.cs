namespace OnlineBookManagementSystem.Models.ViewModel
{
    internal class CartViewModel
    {
        public List<ShoppingCartViewModel> CartItems { get; set; }
        public CartSummaryViewModel Summary { get; set; }
    }
}