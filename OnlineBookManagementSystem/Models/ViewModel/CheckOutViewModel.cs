namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class CheckOutViewModel
    {
        public List<ShoppingCart> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
    }
}
