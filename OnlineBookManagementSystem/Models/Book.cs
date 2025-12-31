using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

public partial class Book
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string? Author { get; set; }  // Make non-nullable if always required

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 0;

    [StringLength(20)]  // Standard ISBN length
    public string? ISBN { get; set; }  // Uppercase for consistency

    [Column(TypeName = "datetime")]
    public DateTime? PublicationDate { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }  // Renamed from ImgUrl

    public int StockQuantity { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 5; // Default threshold

    [NotMapped]
    public bool IsAvailable => StockQuantity > 0;

    [StringLength(maximumLength: 1000)]
    public string? Description { get; set; }  // New

    public int? CategoryId { get; set; }

    public string ImgUrl { get; set; }
    public bool IsFavorite { get; set; } = false;
    public bool IsFeatured { get; set; } = false;  // New property for featured books
    public double AverageRating { get; set; } = 0.0;  // New property for ratings
    public bool IsDeleted { get; set; } = false;

    // Timestamps (replace CreatedDate)
    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "DateTime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Category? Category { get; set; }


    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
    public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();
}