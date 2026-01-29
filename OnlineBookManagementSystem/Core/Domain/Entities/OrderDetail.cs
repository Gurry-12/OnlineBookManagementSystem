using OnlineBookManagementSystem.Core.Domain.ValueObjects;

namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class OrderDetail : BaseEntity
    {
        private int _quantity;
        private Money _unitPrice;
        private Money _subtotal;

        public int OrderId { get; set; }
        public int BookId { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Quantity must be positive", nameof(value));
                _quantity = value;
                RecalculateSubtotal();
            }
        }

        public Money UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value ?? throw new ArgumentNullException(nameof(value));
                RecalculateSubtotal();
            }
        }

        public Money Subtotal 
        { 
            get => _subtotal;
            set => _subtotal = value ?? new Money(0);
        }

        // Compatibility property
        public Money Price => _unitPrice;

        // Calculated Properties
        public decimal TotalPrice => Quantity * (Price?.Amount ?? 0);

        // Navigation properties
        public virtual Order Order { get; set; } = null!;




        public virtual Book Book { get; set; } = null!;

        // Private constructor for EF Core
        public OrderDetail()
        {
            _unitPrice = new Money(0);
            _subtotal = new Money(0);
        }

        public OrderDetail(int orderId, int bookId, int quantity, Money unitPrice)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be positive", nameof(orderId));
            if (bookId <= 0)
                throw new ArgumentException("Book ID must be positive", nameof(bookId));

            OrderId = orderId;
            BookId = bookId;
            UnitPrice = unitPrice;
            Quantity = quantity; // This will trigger subtotal calculation
        }

        public void UpdateQuantity(int newQuantity)
        {
            Quantity = newQuantity;
        }

        public void UpdateUnitPrice(Money newUnitPrice)
        {
            UnitPrice = newUnitPrice;
        }

        private void RecalculateSubtotal()
        {
            _subtotal = _unitPrice.Multiply(_quantity);
        }

        public Money GetTotalPrice()
        {
            return _subtotal;
        }
    }
}