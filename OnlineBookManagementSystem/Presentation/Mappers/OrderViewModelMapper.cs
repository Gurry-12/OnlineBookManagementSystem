using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.User;

namespace OnlineBookManagementSystem.Presentation.Mappers
{
    /// <summary>
    /// Maps Order entities to ViewModels
    /// Prevents entity leakage to views
    /// </summary>
    public static class OrderViewModelMapper
    {
        public static OrderDetailViewModel MapToOrderDetailViewModel(Order order)
        {
            var itemsTotal = order.OrderDetails.Sum(od => od.Subtotal.Amount);
            var tax = itemsTotal * 0.18m; // 18% tax
            var shippingCost = itemsTotal > 500 ? 0 : 50; // Free shipping over 500

            return new OrderDetailViewModel
            {
                OrderId = order.Id,
                OrderNumber = $"#{order.Id:D6}",
                OrderDate = order.OrderDate ?? DateTime.UtcNow,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Subtotal = itemsTotal,
                Tax = tax,
                ShippingCost = shippingCost,
                TotalAmount = order.TotalAmount.Amount,
                FullName = order.FullName ?? string.Empty,
                PhoneNumber = order.PhoneNumber ?? string.Empty,
                ShippingAddress = order.ShippingAddress?.Street ?? string.Empty,
                City = order.ShippingAddress?.City ?? string.Empty,
                PinCode = order.ZipCode ?? string.Empty,
                Items = order.OrderDetails.Select(MapToOrderItemViewModel).ToList()
            };
        }

        public static OrderHistoryItemViewModel MapToOrderHistoryItem(Order order)
        {
            return new OrderHistoryItemViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate ?? DateTime.UtcNow,
                Status = order.Status,
                TotalAmount = order.TotalAmount.Amount,
                ItemCount = order.OrderDetails.Sum(od => od.Quantity),
                PaymentMethod = order.PaymentMethod ?? "N/A",
                ShippingAddress = order.ShippingAddress?.Street ?? string.Empty,
                OrderDetails = order.OrderDetails.Select(MapToOrderItemViewModel).ToList()
            };
        }

        private static OrderItemViewModel MapToOrderItemViewModel(OrderDetail orderDetail)
        {
            return new OrderItemViewModel
            {
                BookId = orderDetail.BookId,
                BookTitle = orderDetail.Book?.Title ?? "Unknown Book",
                BookImageUrl = orderDetail.Book?.ImageUrl,
                Quantity = orderDetail.Quantity,
                UnitPrice = orderDetail.UnitPrice.Amount,
                Subtotal = orderDetail.Subtotal.Amount
            };
        }

        public static List<OrderHistoryItemViewModel> MapToOrderHistoryList(IEnumerable<Order> orders)
        {
            return orders.Select(MapToOrderHistoryItem).ToList();
        }
    }
}
