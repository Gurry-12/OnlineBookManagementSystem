using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Presentation.ViewModels.User
{
    /// <summary>
    /// ViewModel for order details page
    /// Prevents Order entity leakage to views
    /// </summary>
    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }

        // Shipping Information
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;

        // Order Items
        public List<OrderItemViewModel> Items { get; set; } = new();

        // UI-specific computed properties
        public string StatusText => Status.ToString();
        public string StatusBadgeClass => Status switch
        {
            OrderStatus.Pending => "badge bg-warning text-dark",
            OrderStatus.Processing => "badge bg-info",
            OrderStatus.Shipped => "badge bg-primary",
            OrderStatus.Completed => "badge bg-success",
            OrderStatus.Cancelled => "badge bg-danger",
            _ => "badge bg-secondary"
        };

        public string PaymentStatusText => PaymentStatus.ToString();
        public string PaymentStatusBadgeClass => PaymentStatus switch
        {
            PaymentStatus.Pending => "badge bg-warning",
            PaymentStatus.Paid => "badge bg-success",
            PaymentStatus.Failed => "badge bg-danger",
            PaymentStatus.Refunded => "badge bg-info",
            _ => "badge bg-secondary"
        };

        public string FormattedOrderDate => OrderDate.ToString("dddd, MMMM dd, yyyy");
        public string FormattedSubtotal => $"₹{Subtotal:N2}";
        public string FormattedTax => $"₹{Tax:N2}";
        public string FormattedShipping => ShippingCost == 0 ? "FREE" : $"₹{ShippingCost:N2}";
        public string FormattedTotal => $"₹{TotalAmount:N2}";

        public int TotalItems => Items.Sum(i => i.Quantity);
        public bool CanCancel => Status == OrderStatus.Pending || Status == OrderStatus.Processing;
        public bool IsCompleted => Status == OrderStatus.Completed;
        public bool IsCancelled => Status == OrderStatus.Cancelled;
    }

    public class OrderItemViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string? BookImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        // UI-specific
        public string FormattedUnitPrice => $"₹{UnitPrice:N2}";
        public string FormattedSubtotal => $"₹{Subtotal:N2}";
    }
}
