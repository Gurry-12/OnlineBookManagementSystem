namespace OnlineBookManagementSystem.Shared.Utilities;

/// <summary>
/// Centralized formatting utilities to eliminate duplicate formatting logic across ViewModels
/// Follows DRY principle and provides consistent formatting across the application
/// </summary>
public static class FormattingExtensions
{
    // Currency Formatting
    public static string FormatCurrency(decimal amount, string currency = "₹")
        => $"{currency}{amount:N2}";
    
    public static string FormatCurrency(decimal? amount, string currency = "₹", string fallback = "Not Available")
        => amount.HasValue ? FormatCurrency(amount.Value, currency) : fallback;
    
    // Date Formatting
    public static string FormatDate(DateTime? date, string format = "MMM dd, yyyy", string fallback = "Not Available")
        => date?.ToString(format) ?? fallback;
    
    public static string FormatDate(DateTime date, string format = "MMM dd, yyyy")
        => date.ToString(format);
    
    public static string FormatDateLong(DateTime? date, string fallback = "Not Available")
        => date?.ToString("dddd, MMMM dd, yyyy") ?? fallback;
    
    // Status Badge Classes
    public static string GetStatusBadgeClass(bool isActive, string activeClass = "badge bg-success", string inactiveClass = "badge bg-danger")
        => isActive ? activeClass : inactiveClass;
    
    public static string GetOrderStatusBadgeClass(string status) => status.ToLower() switch
    {
        "pending" => "badge bg-warning text-dark",
        "processing" => "badge bg-info",
        "shipped" => "badge bg-primary",
        "completed" => "badge bg-success",
        "cancelled" => "badge bg-danger",
        _ => "badge bg-secondary"
    };
    
    public static string GetPaymentStatusBadgeClass(string status) => status.ToLower() switch
    {
        "pending" => "badge bg-warning",
        "paid" => "badge bg-success",
        "failed" => "badge bg-danger",
        "refunded" => "badge bg-info",
        _ => "badge bg-secondary"
    };
    
    // Stock Status
    public static string GetStockStatus(int quantity)
        => quantity > 0 ? $"In Stock ({quantity} available)" : "Out of Stock";
    
    public static string GetStockBadgeClass(int quantity)
        => quantity > 0 ? "badge bg-success" : "badge bg-danger";
    
    // Text Truncation
    public static string TruncateText(string? text, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text ?? string.Empty;
        
        return text[..maxLength] + suffix;
    }
    
    // Quantity Formatting
    public static string FormatQuantity(int quantity, string singular = "item", string plural = "items")
        => quantity == 1 ? $"{quantity} {singular}" : $"{quantity} {plural}";
    
    // File Size Formatting
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
    
    // Percentage Formatting
    public static string FormatPercentage(decimal value, int decimals = 1)
        => value.ToString($"F{decimals}") + "%";
    
    public static string FormatPercentage(double value, int decimals = 1)
        => value.ToString($"F{decimals}") + "%";
}