namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class CheckOutViewModel
    {
        public List<ShoppingCartViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Subtotal { get; internal set; }
        public decimal Tax { get; internal set; }
        public int Shipping { get; internal set; }
        public decimal GrandTotal { get; internal set; }
        public int UserId { get; internal set; }
    }
}
