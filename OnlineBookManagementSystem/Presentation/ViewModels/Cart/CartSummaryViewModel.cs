using OnlineBookManagementSystem.Shared.Utilities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Cart;

public class CartSummaryViewModel
{
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public decimal Shipping { get; set; }
    public decimal Tax { get; set; }
    public decimal TaxRate { get; set; } = 0.10m; // 10% tax rate
    public int ItemCount { get; set; }
    public decimal GrandTotal { get; set; }

    // Computed Properties with Formatting
    public string FormattedSubtotal => FormattingExtensions.FormatCurrency(Subtotal);
    public string FormattedTotal => FormattingExtensions.FormatCurrency(Total);
    public string FormattedShipping => FormattingExtensions.FormatCurrency(Shipping);
    public string FormattedTax => FormattingExtensions.FormatCurrency(Tax);
    public string FormattedGrandTotal => FormattingExtensions.FormatCurrency(GrandTotal);
}