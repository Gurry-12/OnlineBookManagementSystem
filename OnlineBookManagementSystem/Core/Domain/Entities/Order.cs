using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Core.Domain.Exceptions;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;

namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class Order : BaseEntity
    {
        private readonly List<OrderDetail> _orderDetails = new();
        private Money _totalAmount;
        private OrderStatus _status;
        private PaymentStatus _paymentStatus;

        public int? UserId { get; set; }
        public Money TotalAmount
        {
            get => _totalAmount;
            set => _totalAmount = value ?? throw new ArgumentNullException(nameof(value));
        }

        public DateTime? OrderDate { get; set; }

        public OrderStatus Status
        {
            get => _status;
            set => _status = value;
        }

        public PaymentStatus PaymentStatus
        {
            get => _paymentStatus;
            set => _paymentStatus = value;
        }

        public string PaymentMethod { get; set; } = "Unpaid";
        public Address? ShippingAddress { get; set; }

        // Extended Address Information (for compatibility)
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Computed properties (read-only) - with private setters for EF Core
        public string? FullName { get; private set; }
        public string? Address { get; private set; }
        public string? Phone { get; private set; }
        public string? City { get; private set; }
        public string? State { get; private set; }
        public string? Country { get; private set; }
        public string? ZipCode { get; private set; }

        // Private constructor for EF Core
        public Order()
        {
            _totalAmount = new Money(0);
            _status = OrderStatus.Pending;
            _paymentStatus = PaymentStatus.Unpaid;
            OrderDate = DateTime.UtcNow;
        }

        public Order(int? userId, Address? shippingAddress = null)
        {
            UserId = userId;
            ShippingAddress = shippingAddress;
            _totalAmount = new Money(0);
            _status = OrderStatus.Pending;
            _paymentStatus = PaymentStatus.Unpaid;
            OrderDate = DateTime.UtcNow;
        }

        public void AddOrderDetail(Book book, int quantity, Money unitPrice)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (!book.CanFulfillOrder(quantity))
                throw new InsufficientStockException(book.Title, quantity, book.StockQuantity);

            // Check if book already exists in order
            var existingDetail = _orderDetails.FirstOrDefault(od => od.BookId == book.Id);
            if (existingDetail != null)
            {
                existingDetail.UpdateQuantity(existingDetail.Quantity + quantity);
            }
            else
            {
                var orderDetail = new OrderDetail(Id, book.Id, quantity, unitPrice);
                _orderDetails.Add(orderDetail);
            }

            RecalculateTotal();
            UpdateTimestamp();
        }

        public void RemoveOrderDetail(int bookId)
        {
            var detail = _orderDetails.FirstOrDefault(od => od.BookId == bookId);
            if (detail != null)
            {
                _orderDetails.Remove(detail);
                RecalculateTotal();
                UpdateTimestamp();
            }
        }

        public void UpdateOrderDetailQuantity(int bookId, int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

            var detail = _orderDetails.FirstOrDefault(od => od.BookId == bookId);
            if (detail == null)
                throw new InvalidOperationException($"Order detail for book {bookId} not found");

            detail.UpdateQuantity(newQuantity);
            RecalculateTotal();
            UpdateTimestamp();
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            if (!_status.CanTransitionTo(newStatus))
                throw new InvalidOrderStateException($"Cannot transition from {_status} to {newStatus}");

            _status = newStatus;
            UpdateTimestamp();
        }

        public void UpdatePaymentStatus(PaymentStatus newPaymentStatus, string? paymentMethod = null)
        {
            _paymentStatus = newPaymentStatus;

            if (!string.IsNullOrWhiteSpace(paymentMethod))
            {
                PaymentMethod = paymentMethod;
            }

            UpdateTimestamp();
        }

        public void UpdateShippingAddress(Address address)
        {
            if (_status != OrderStatus.Pending)
                throw new InvalidOrderStateException("Cannot update shipping address after order is confirmed");

            ShippingAddress = address ?? throw new ArgumentNullException(nameof(address));
            UpdateTimestamp();
        }

        public bool CanBeCancelled()
        {
            return _status is OrderStatus.Pending or OrderStatus.Confirmed;
        }

        public void Cancel()
        {
            if (!CanBeCancelled())
                throw new InvalidOrderStateException("Order cannot be cancelled in current state");

            _status = OrderStatus.Cancelled;
            UpdateTimestamp();
        }

        public bool IsCompleted()
        {
            return _status.IsFinalStatus();
        }

        public bool IsPaid()
        {
            return _paymentStatus.IsSuccessful();
        }

        private void RecalculateTotal()
        {
            var total = _orderDetails.Sum(od => od.Subtotal.Amount);
            _totalAmount = new Money(total);
        }

        public Money GetItemsTotal()
        {
            return new Money(_orderDetails.Sum(od => od.Subtotal.Amount));
        }

        public int GetTotalItemCount()
        {
            return _orderDetails.Sum(od => od.Quantity);
        }
    }
}