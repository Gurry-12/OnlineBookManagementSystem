using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.User;
using OnlineBookManagementSystem.Presentation.ViewModels.Cart;

namespace OnlineBookManagementSystem.Core.Application.Mappings
{
    /// <summary>
    /// Extension methods for mapping Order entities to ViewModels
    /// </summary>
    public static class OrderMappingExtensions
    {
        /// <summary>
        /// Maps Order entity to OrderHistoryViewModel for user views
        /// </summary>
        public static OrderHistoryViewModel ToOrderHistoryViewModel(this IEnumerable<Order> orders,
            int currentPage, int totalPages, int totalOrders)
        {
            return new OrderHistoryViewModel
            {
                Orders = orders?.Select(o => o.ToOrderHistoryItem()).ToList() ?? new List<OrderHistoryItemViewModel>(),
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalOrders = totalOrders
            };
        }

        /// <summary>
        /// Maps Order collection to AdminOrderListViewModel for admin views
        /// </summary>
        public static AdminOrderListViewModel ToAdminOrderListViewModel(this IEnumerable<Order> orders,
            int currentPage, int totalPages, int totalOrders,
            string searchTerm = null, string statusFilter = null)
        {
            return new AdminOrderListViewModel
            {
                Orders = orders?.Select(o => o.ToAdminOrderItem()).ToList() ?? new List<AdminOrderItemViewModel>(),
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalOrders = totalOrders,
                SearchTerm = searchTerm,
                StatusFilter = statusFilter
            };
        }

        /// <summary>
        /// Maps Order entity to CheckOutViewModel for checkout process
        /// </summary>
        public static CheckOutViewModel ToCheckOutViewModel(this Order order, IEnumerable<ShoppingCart> cartItems = null)
        {
            if (order == null) return null;

            return new CheckOutViewModel
            {
                Order = order,
                CartItems = cartItems?.Select(ci => new ShoppingCartViewModel
                {
                    Id = ci.Id,
                    BookId = ci.BookId,
                    BookTitle = ci.Book?.Title ?? string.Empty,
                    Quantity = ci.Quantity,
                    BookPrice = ci.Book?.Price?.Amount ?? 0,
                    CategoryName = ci.Book?.Category?.Name ?? string.Empty,
                    BookImage = ci.Book?.ImageUrl
                }).ToList() ?? new List<ShoppingCartViewModel>(),
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                State = order.State,
                ZipCode = order.ZipCode,
                Country = order.Country,
                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber
            };
        }

        /// <summary>
        /// Creates Order entity from CheckOutRequestViewModel
        /// </summary>
        public static Order ToOrderEntity(this CheckOutRequestViewModel request, int userId, IEnumerable<ShoppingCart> cartItems)
        {
            if (request == null) return null;

            var shippingAddress = new Address(
                request.FullName ?? string.Empty,
                request.ShippingAddress ?? string.Empty,
                request.City ?? string.Empty,
                request.State ?? string.Empty,
                request.ZipCode ?? string.Empty,
                request.Country ?? string.Empty
            );

            var order = new Order
            {
                UserId = userId,
                PhoneNumber = request.PhoneNumber,
                ShippingAddress = shippingAddress,
                PaymentMethod = request.PaymentMethod,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cartItems?.Aggregate(new Money(0), (sum, ci) => sum + (ci.Book.Price * ci.Quantity)) ?? new Money(0)
            };

            // Map cart items to order details
            if (cartItems != null)
            {
                foreach (var cartItem in cartItems)
                {
                    order.AddOrderDetail(cartItem.Book, cartItem.Quantity, cartItem.Book.Price);
                }
            }

            return order;
        }

        /// <summary>
        /// Maps grouped order data to MonthlyRevenueViewModel
        /// </summary>
        public static MonthlyRevenueViewModel ToMonthlyRevenueViewModel(this IGrouping<object, Order> group, string monthKey)
        {
            return new MonthlyRevenueViewModel
            {
                Month = monthKey,
                Revenue = group.Sum(o => o.TotalAmount.Amount),
                OrderCount = group.Count()
            };
        }

        /// <summary>
        /// Maps grouped order data to OrderStatusViewModel
        /// </summary>
        public static OrderStatusViewModel ToOrderStatusViewModel(this IGrouping<string, Order> group)
        {
            return new OrderStatusViewModel
            {
                Status = group.Key,
                Count = group.Count(),
                Percentage = 0 // This should be calculated by the calling service
            };
        }

        /// <summary>
        /// Updates Order entity from status change
        /// </summary>
        public static void UpdateStatus(this Order order, OrderStatus newStatus)
        {
            if (order == null) return;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // Set specific timestamps based on status
            switch (newStatus)
            {
                case OrderStatus.Shipped:
                    order.ShippedDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredDate = DateTime.UtcNow;
                    break;
            }
        }

        /// <summary>
        /// Maps Order entity to OrderHistoryItem for user order history
        /// </summary>
        public static OrderHistoryItemViewModel ToOrderHistoryItem(this Order order)
        {
            if (order == null) return null;

            return new OrderHistoryItemViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate ?? DateTime.MinValue,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount.Amount,
                ItemCount = order.OrderDetails?.Count ?? 0,
                PaymentMethod = order.PaymentMethod ?? "Unknown",
                ShippingAddress = order.ShippingAddress?.ToString() ?? "No address",
                OrderDetails = order.OrderDetails?.Select(od => new OrderDetailViewModel
                {
                    Id = od.Id,
                    BookId = od.BookId,
                    Book = od.Book != null ? new BookViewModel
                    {
                        Id = od.Book.Id,
                        Title = od.Book.Title,
                        Author = od.Book.Author,
                        ImageUrl = od.Book.ImageUrl,
                        Price = od.Book.Price.Amount
                    } : null,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice.Amount,
                    Subtotal = od.Subtotal.Amount
                }).ToList() ?? new List<OrderDetailViewModel>()
            };
        }

        /// <summary>
        /// Maps Order entity to AdminOrderItem for admin order management
        /// </summary>
        public static AdminOrderItemViewModel ToAdminOrderItem(this Order order)
        {
            if (order == null) return null;

            return new AdminOrderItemViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate ?? DateTime.MinValue,
                CustomerName = order.User?.Name ?? "Unknown Customer",
                CustomerEmail = order.User?.Email ?? "Unknown Email",
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount.Amount,
                ItemCount = order.OrderDetails?.Count ?? 0,
                PaymentMethod = order.PaymentMethod ?? "Unknown",
                PaymentStatus = order.PaymentStatus.ToString()
            };
        }
    }
}