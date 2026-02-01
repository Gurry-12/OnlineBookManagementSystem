using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Presentation.ViewModels.User
{
    public class OrderHistoryItemViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItemViewModel> OrderDetails { get; set; } = new();
    }
}