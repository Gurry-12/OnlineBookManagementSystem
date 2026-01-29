namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin
{
    public class OrderStatusViewModel
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }
}