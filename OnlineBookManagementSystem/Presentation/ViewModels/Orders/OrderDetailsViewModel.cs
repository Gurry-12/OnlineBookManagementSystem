using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Shared.Utilities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Orders;

/// <summary>
/// Universal OrderDetailsViewModel - Serves all roles (User, Admin, SuperAdmin)
/// Uses capability flags to control what actions and information are available
/// </summary>
public class OrderDetailsViewModel
{
    // Core Order Information (Always Present)
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;

    // Customer Information (Always Present)
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;

    // Order Items (Always Present)
    public List<OrderItemViewModel> Items { get; set; } = new();

    // Admin-Only Properties (Nullable for other roles)
    public string? CustomerEmail { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? AdminNotes { get; set; }

    // Capability-Based Metadata (NO ROLES)
    public OrderDetailsCapabilities Capabilities { get; set; } = new();

    // Computed Properties using FormattingExtensions
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

    public string FormattedOrderDate => FormattingExtensions.FormatDate(OrderDate, "dddd, MMMM dd, yyyy");
    public string FormattedSubtotal => FormattingExtensions.FormatCurrency(Subtotal);
    public string FormattedTax => FormattingExtensions.FormatCurrency(Tax);
    public string FormattedShipping => ShippingCost == 0 ? "FREE" : FormattingExtensions.FormatCurrency(ShippingCost);
    public string FormattedTotal => FormattingExtensions.FormatCurrency(TotalAmount);
    public string FormattedCreatedAt => FormattingExtensions.FormatDate(CreatedAt, "MMM dd, yyyy HH:mm");
    public string FormattedUpdatedAt => FormattingExtensions.FormatDate(UpdatedAt, "MMM dd, yyyy HH:mm");

    public int TotalItems => Items.Sum(i => i.Quantity);
    public int StatusProgressPercentage => Status switch
    {
        OrderStatus.Pending => 25,
        OrderStatus.Processing => 50,
        OrderStatus.Shipped => 75,
        OrderStatus.Completed => 100,
        _ => 0
    };
}

/// <summary>
/// Capability-based context for conditional rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class OrderDetailsCapabilities
{
    // View Capabilities
    public bool CanViewPaymentDetails { get; set; } = false; // Admin/SuperAdmin see payment transaction details
    public bool CanViewCustomerInfo { get; set; } = false; // Admin/SuperAdmin see customer email, ID
    public bool CanViewTechnicalDetails { get; set; } = false; // Admin/SuperAdmin see created/updated dates, notes

    // Action Capabilities
    public bool CanCancel { get; set; } = false; // Users can cancel their own pending orders
    public bool CanChangeStatus { get; set; } = false; // Admin/SuperAdmin can change order status
    public bool CanRefund { get; set; } = false; // SuperAdmin can process refunds
    public bool CanAddNotes { get; set; } = false; // Admin/SuperAdmin can add internal notes
    public bool CanReorder { get; set; } = false; // Users can reorder completed orders

    // Status Change Capabilities (Granular for different status transitions)
    public bool CanMarkAsProcessing { get; set; } = false;
    public bool CanMarkAsShipped { get; set; } = false;
    public bool CanMarkAsCompleted { get; set; } = false;
    public bool CanMarkAsCancelled { get; set; } = false;

    // UI Context (NOT roles)
    public bool IsAuthenticated { get; set; } = false;
    public bool IsOwnOrder { get; set; } = false; // True if viewing user's own order
    public string BackLinkText { get; set; } = "Back to Orders";
    public string BackLinkUrl { get; set; } = "/Orders";
    public string PageTitle { get; set; } = "Order Details";
    public string LayoutClass { get; set; } = "user-layout";
}

/// <summary>
/// Order item view model for display
/// </summary>
public class OrderItemViewModel
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string? BookImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    // UI-specific computed properties
    public string FormattedUnitPrice => FormattingExtensions.FormatCurrency(UnitPrice);
    public string FormattedSubtotal => FormattingExtensions.FormatCurrency(Subtotal);
}