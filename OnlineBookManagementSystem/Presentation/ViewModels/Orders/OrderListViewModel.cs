using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Orders;

/// <summary>
/// Universal OrderListViewModel - Serves all roles (User, Admin, SuperAdmin)
/// Uses capability flags to control what actions are available
/// </summary>
public class OrderListViewModel
{
    // Core Order Data (Always Present)
    public List<Order> Orders { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalOrders { get; set; }

    // Filtering & Search
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    // Statistics (Nullable for roles that don't need them)
    public int? PendingOrders { get; set; }
    public int? ProcessingOrders { get; set; }
    public int? CompletedOrders { get; set; }
    public decimal? TotalRevenue { get; set; }

    // Capability-Based Metadata (NO ROLES)
    public OrderListCapabilities Capabilities { get; set; } = new();
}

/// <summary>
/// Capability-based context for order list rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class OrderListCapabilities
{
    // View Capabilities
    public bool CanViewAllOrders { get; set; } = false; // Admin/SuperAdmin see all, Users see only their own
    public bool CanViewPaymentSummary { get; set; } = false; // Admin/SuperAdmin see payment details
    public bool CanViewCustomerInfo { get; set; } = false; // Admin/SuperAdmin see customer details
    public bool CanViewStatistics { get; set; } = false; // Admin/SuperAdmin see order statistics

    // Action Capabilities per Order
    public bool CanChangeStatus { get; set; } = false; // Admin/SuperAdmin can change order status
    public bool CanViewPaymentDetails { get; set; } = false; // Admin/SuperAdmin see payment info
    public bool CanRefund { get; set; } = false; // SuperAdmin can process refunds
    public bool CanCancel { get; set; } = false; // Users can cancel their own pending orders

    // List Management Capabilities
    public bool CanFilter { get; set; } = true;
    public bool CanSearch { get; set; } = true;
    public bool CanSort { get; set; } = true;
    public bool CanPaginate { get; set; } = true;

    // UI Context (NOT roles)
    public bool IsAuthenticated { get; set; } = false;
    public string PageTitle { get; set; } = "Orders";
    public string BackLinkText { get; set; } = "Back to Dashboard";
    public string BackLinkUrl { get; set; } = "/";
    public string DetailsActionName { get; set; } = "Details";
    public string DetailsControllerName { get; set; } = "Orders";
    public string LayoutClass { get; set; } = "user-layout";
}