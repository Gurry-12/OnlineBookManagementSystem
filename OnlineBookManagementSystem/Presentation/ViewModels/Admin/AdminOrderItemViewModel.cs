using OnlineBookManagementSystem.Presentation.ViewModels.User;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin
{
    public class AdminOrderItemViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string FullName => CustomerName; // Alias for compatibility
        public Core.Domain.Entities.User? User { get; set; } // Navigation property for user details
        public List<OrderItemViewModel> OrderDetails { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }
}