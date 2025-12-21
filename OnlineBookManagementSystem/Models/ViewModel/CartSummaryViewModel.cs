namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class CartSummaryViewModel
    {
        public decimal Subtotal { get; internal set; }
        public int Shipping { get; internal set; }
        public decimal Tax { get; internal set; }
        public int ItemCount { get; internal set; }
        public decimal GrandTotal { get; internal set; }
    }
}