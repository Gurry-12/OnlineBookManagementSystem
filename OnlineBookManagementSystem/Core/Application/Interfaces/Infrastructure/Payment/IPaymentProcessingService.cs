using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Payment
{
    /// <summary>
    /// Service interface for payment processing operations
    /// </summary>
    public interface IPaymentProcessingService
    {
        // Payment processing
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount);
        Task<PaymentResult> CapturePaymentAsync(string transactionId);
        Task<PaymentResult> VoidPaymentAsync(string transactionId);

        // Payment validation
        Task<bool> ValidatePaymentMethodAsync(string paymentMethod, PaymentDetails details);
        Task<PaymentResult> VerifyPaymentAsync(string transactionId);

        // Payment status
        Task<PaymentStatus> GetPaymentStatusAsync(string transactionId);
        Task<List<PaymentTransaction>> GetPaymentHistoryAsync(int orderId);
    }

    /// <summary>
    /// Request model for payment processing
    /// </summary>
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } = string.Empty;
        public PaymentDetails PaymentDetails { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payment details for different payment methods
    /// </summary>
    public class PaymentDetails
    {
        // Credit Card Details
        public string? CardNumber { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? CVV { get; set; }
        public string? CardHolderName { get; set; }

        // Digital Wallet Details
        public string? WalletType { get; set; }
        public string? WalletToken { get; set; }

        // Bank Transfer Details
        public string? BankAccount { get; set; }
        public string? RoutingNumber { get; set; }
    }

    /// <summary>
    /// Result of a payment operation
    /// </summary>
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? AuthorizationCode { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string? ErrorCode { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }



    /// <summary>
    /// Payment transaction record
    /// </summary>
    public class PaymentTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}