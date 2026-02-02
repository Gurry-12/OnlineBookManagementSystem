using OnlineBookManagementSystem.Shared.Utilities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Cart;

/// <summary>
/// Universal CartViewModel - Serves all roles (User, Admin)
/// Uses capability flags to control what actions are available
/// </summary>
public class UnifiedCartViewModel
{
    // Core Cart Data (Always Present)
    public List<CartItemViewModel> CartItems { get; set; } = new();
    public CartSummaryViewModel Summary { get; set; } = new();
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }

    // Capability-Based Metadata (NO ROLES)
    public CartCapabilities Capabilities { get; set; } = new();

    // Computed Properties
    public bool HasItems => CartItems.Any();
    public int TotalItems => CartItems.Sum(x => x.Quantity);
    public string FormattedLastUpdated => FormattingExtensions.FormatDate(LastUpdated, "MMM dd, yyyy HH:mm");
}

/// <summary>
/// Universal CheckoutViewModel - Serves authenticated users only
/// Uses capability flags to control checkout flow
/// </summary>
public class UnifiedCheckoutViewModel
{
    // Core Checkout Data (Always Present)
    public List<CartItemViewModel> CartItems { get; set; } = new();
    public CartSummaryViewModel Summary { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal GrandTotal { get; set; }
    public int UserId { get; set; }

    // Shipping Information
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string PaymentMethod { get; set; } = "COD";

    // Capability-Based Metadata (NO ROLES)
    public CheckoutCapabilities Capabilities { get; set; } = new();

    // Computed Properties
    public bool HasItems => CartItems.Any();
    public string FormattedGrandTotal => FormattingExtensions.FormatCurrency(GrandTotal);
    public bool QualifiesForFreeShipping => GrandTotal > 1000;
}

/// <summary>
/// Capability-based context for cart rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class CartCapabilities
{
    // View Capabilities
    public bool CanViewCart { get; set; } = true; // Everyone can view their cart
    public bool CanViewCartDetails { get; set; } = true; // Everyone can see cart details
    public bool CanViewUserInfo { get; set; } = false; // Admin can see user info for other carts

    // Action Capabilities
    public bool CanModifyCart { get; set; } = false; // Users can modify their own cart
    public bool CanUpdateQuantity { get; set; } = false; // Users can update quantities
    public bool CanRemoveItems { get; set; } = false; // Users can remove items
    public bool CanCheckout { get; set; } = false; // Users can proceed to checkout
    public bool CanClearCart { get; set; } = false; // Users can clear entire cart

    // UI Context (NOT roles)
    public bool IsReadOnly { get; set; } = false; // Admin viewing user cart
    public bool IsAuthenticated { get; set; } = false;
    public string PageTitle { get; set; } = "Shopping Cart";
    public string BackLinkText { get; set; } = "Continue Shopping";
    public string BackLinkUrl { get; set; } = "/Books/BookList";
    public string CheckoutButtonText { get; set; } = "Proceed to Checkout";
}

/// <summary>
/// Capability-based context for checkout rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class CheckoutCapabilities
{
    // View Capabilities
    public bool CanViewOrderSummary { get; set; } = true; // Everyone can view order summary
    public bool CanViewShippingForm { get; set; } = true; // Users can see shipping form
    public bool CanViewPaymentOptions { get; set; } = true; // Users can see payment options

    // Action Capabilities
    public bool CanConfirmCheckout { get; set; } = false; // Users can confirm checkout
    public bool CanModifyOrder { get; set; } = false; // Users can modify before checkout
    public bool CanSelectPaymentMethod { get; set; } = true; // Users can select payment

    // Payment Options
    public bool ShowCODOption { get; set; } = true; // Cash on delivery available
    public bool ShowOnlinePayment { get; set; } = true; // Online payment available

    // UI Context (NOT roles)
    public bool IsAuthenticated { get; set; } = false;
    public string PageTitle { get; set; } = "Checkout";
    public string BackLinkText { get; set; } = "Back to Cart";
    public string BackLinkUrl { get; set; } = "/Cart";
    public string ConfirmButtonText { get; set; } = "Place Order";
}