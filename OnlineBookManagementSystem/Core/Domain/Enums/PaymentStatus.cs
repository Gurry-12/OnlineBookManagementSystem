namespace OnlineBookManagementSystem.Core.Domain.Enums
{
    public enum PaymentStatus
    {
        Unpaid = 1,
        Pending = 2,
        Paid = 3,
        Failed = 4,
        Refunded = 5,
        PartiallyRefunded = 6,
        Completed = 7,  // Alias for Paid
        Captured = 8,   // Payment captured
        Voided = 9,      // Payment voided
    }

    public static class PaymentStatusExtensions
    {
        public static string ToDisplayString(this PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Unpaid => "Unpaid",
                PaymentStatus.Pending => "Pending",
                PaymentStatus.Paid => "Paid",
                PaymentStatus.Failed => "Failed",
                PaymentStatus.Refunded => "Refunded",
                PaymentStatus.PartiallyRefunded => "Partially Refunded",
                PaymentStatus.Completed => "Completed",
                PaymentStatus.Captured => "Captured",
                PaymentStatus.Voided => "Voided",
                
                _ => status.ToString()
            };
        }

        public static bool IsSuccessful(this PaymentStatus status)
        {
            return status is PaymentStatus.Paid or PaymentStatus.Completed or PaymentStatus.Captured;
        }

        public static bool IsFinal(this PaymentStatus status)
        {
            return status is PaymentStatus.Paid or PaymentStatus.Completed or PaymentStatus.Failed or PaymentStatus.Refunded or PaymentStatus.Voided;
        }

        public static PaymentStatus Parse(string value)
        {
            if (Enum.TryParse<PaymentStatus>(value, true, out var result))
                return result;
            
            throw new ArgumentException($"Invalid PaymentStatus value: {value}");
        }

        public static PaymentStatus? TryParse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            
            if (Enum.TryParse<PaymentStatus>(value, true, out var result))
                return result;
            
            return null;
        }
    }
}