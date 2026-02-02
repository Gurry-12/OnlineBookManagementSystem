namespace OnlineBookManagementSystem.Core.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Refunded = 7,
        Completed = 8
    }

    public static class OrderStatusExtensions
    {
        public static string ToDisplayString(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pending",
                OrderStatus.Confirmed => "Confirmed",
                OrderStatus.Processing => "Processing",
                OrderStatus.Shipped => "Shipped",
                OrderStatus.Delivered => "Delivered",
                OrderStatus.Cancelled => "Cancelled",
                OrderStatus.Refunded => "Refunded",
                _ => status.ToString()
            };
        }

        public static bool CanTransitionTo(this OrderStatus currentStatus, OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => newStatus is OrderStatus.Confirmed or OrderStatus.Cancelled,
                OrderStatus.Confirmed => newStatus is OrderStatus.Processing or OrderStatus.Cancelled,
                OrderStatus.Processing => newStatus is OrderStatus.Shipped or OrderStatus.Cancelled,
                OrderStatus.Shipped => newStatus is OrderStatus.Delivered,
                OrderStatus.Delivered => newStatus is OrderStatus.Refunded,
                OrderStatus.Cancelled => false,
                OrderStatus.Refunded => false,
                _ => false
            };
        }

        public static bool IsFinalStatus(this OrderStatus status)
        {
            return status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded or OrderStatus.Completed;
        }

        public static OrderStatus Parse(string value)
        {
            if (Enum.TryParse<OrderStatus>(value, true, out var result))
                return result;

            throw new ArgumentException($"Invalid OrderStatus value: {value}");
        }

        public static OrderStatus? TryParse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Enum.TryParse<OrderStatus>(value, true, out var result))
                return result;

            return null;
        }
    }
}