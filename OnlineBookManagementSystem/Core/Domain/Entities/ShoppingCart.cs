namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class ShoppingCart : BaseEntity
    {
        private int _quantity = 1;

        public int UserId { get; set; }
        public int BookId { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Quantity must be positive", nameof(value));
                _quantity = value;
            }
        }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Book Book { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        // Private constructor for EF Core
        public ShoppingCart() { }

        public ShoppingCart(int userId, int bookId, int quantity = 1)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be positive", nameof(userId));
            if (bookId <= 0)
                throw new ArgumentException("BookId must be positive", nameof(bookId));

            UserId = userId;
            BookId = bookId;
            Quantity = quantity;
            AddedAt = DateTime.UtcNow;
        }

        public void UpdateQuantity(int quantity)
        {
            Quantity = quantity;
            UpdateTimestamp();
        }

        public void IncreaseQuantity(int amount = 1)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            Quantity += amount;
            UpdateTimestamp();
        }

        public void DecreaseQuantity(int amount = 1)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));
            if (amount >= Quantity)
                throw new ArgumentException("Cannot decrease quantity below 1", nameof(amount));

            Quantity -= amount;
            UpdateTimestamp();
        }

        public decimal GetSubtotal()
        {
            return Book?.Price.Amount * Quantity ?? 0;
        }
    }
}