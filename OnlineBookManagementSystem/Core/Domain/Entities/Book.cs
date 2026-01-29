using OnlineBookManagementSystem.Core.Domain.Exceptions;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;

namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class Book : BaseEntity
    {
        private string _title = string.Empty;
        private string _author = string.Empty;
        private Money _price;
        private int _stockQuantity;
        private int _lowStockThreshold = 5;

        public string Title 
        { 
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be null or empty", nameof(value));
                if (value.Length > 200)
                    throw new ArgumentException("Title cannot exceed 200 characters", nameof(value));
                _title = value.Trim();
            }
        }

        public string Author 
        { 
            get => _author;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Author cannot be null or empty", nameof(value));
                if (value.Length > 100)
                    throw new ArgumentException("Author cannot exceed 100 characters", nameof(value));
                _author = value.Trim();
            }
        }

        public Money Price 
        { 
            get => _price;
            set => _price = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ISBN? ISBN { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string? ImageUrl { get; set; }
        
        public int StockQuantity 
        { 
            get => _stockQuantity;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Stock quantity cannot be negative", nameof(value));
                _stockQuantity = value;
            }
        }

        public int LowStockThreshold 
        { 
            get => _lowStockThreshold;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Low stock threshold cannot be negative", nameof(value));
                _lowStockThreshold = value;
            }
        }

        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public bool IsFeatured { get; set; }
        public double AverageRating { get; set; }

        // Review Statistics
        public int TotalReviews { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

        // Non-persisted properties (for UI/DTO purposes)
        public bool IsFavorite { get; set; }

        // Computed properties
        public bool IsAvailable => StockQuantity > 0 && !IsDeleted;
        public bool IsLowStock => StockQuantity <= LowStockThreshold && StockQuantity > 0;
        public bool IsOutOfStock => StockQuantity == 0;
        public bool IsInStock => StockQuantity > 0;

        // Public parameterless constructor for EF Core and serialization
        public Book() 
        {
            _price = new Money(0);
            _title = string.Empty;
            _author = string.Empty;
        }

        public Book(string title, string author, Money price, int? categoryId = null)
        {
            Title = title;
            Author = author;
            Price = price;
            CategoryId = categoryId;
            _stockQuantity = 0;
            AverageRating = 0.0;
            IsFeatured = false;
        }

        public void UpdateBasicInfo(string title, string author, Money price, string? description = null)
        {
            Title = title;
            Author = author;
            Price = price;
            Description = description?.Length > 1000 
                ? throw new ArgumentException("Description cannot exceed 1000 characters") 
                : description?.Trim();
            UpdateTimestamp();
        }

        public void SetISBN(string isbn)
        {
            ISBN = new ISBN(isbn);
            UpdateTimestamp();
        }

        public void SetPublicationDate(DateTime publicationDate)
        {
            if (publicationDate > DateTime.UtcNow)
                throw new ArgumentException("Publication date cannot be in the future", nameof(publicationDate));
            
            PublicationDate = publicationDate;
            UpdateTimestamp();
        }

        public void SetImageUrl(string imageUrl)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl.Length > 500)
                throw new ArgumentException("Image URL cannot exceed 500 characters", nameof(imageUrl));
            
            ImageUrl = imageUrl?.Trim();
            UpdateTimestamp();
        }

        public void UpdateStock(int quantity)
        {
            StockQuantity = quantity;
            UpdateTimestamp();
        }

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity to add must be positive", nameof(quantity));
            
            StockQuantity += quantity;
            UpdateTimestamp();
        }

        public void ReduceStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity to reduce must be positive", nameof(quantity));
            
            if (quantity > StockQuantity)
                throw new InsufficientStockException(Title, quantity, StockQuantity);
            
            StockQuantity -= quantity;
            UpdateTimestamp();
        }

        public void RestoreStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity to restore must be positive", nameof(quantity));
            
            StockQuantity += quantity;
            UpdateTimestamp();
        }

        public void SetLowStockThreshold(int threshold)
        {
            LowStockThreshold = threshold;
            UpdateTimestamp();
        }

        public void SetCategory(int? categoryId)
        {
            CategoryId = categoryId;
            UpdateTimestamp();
        }

        public void SetFeatured(bool isFeatured)
        {
            IsFeatured = isFeatured;
            UpdateTimestamp();
        }

        public void UpdateRating(double averageRating)
        {
            if (averageRating < 0 || averageRating > 5)
                throw new ArgumentException("Rating must be between 0 and 5", nameof(averageRating));
            
            AverageRating = Math.Round(averageRating, 2);
            UpdateTimestamp();
        }

        public bool CanFulfillOrder(int requestedQuantity)
        {
            return IsAvailable && StockQuantity >= requestedQuantity;
        }
    }
}