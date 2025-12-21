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

    [StringLength(500)]
    public string? ImageUrl { get; set; }  // Renamed from ImgUrl

    public int StockQuantity { get; set; } = 0;  // Changed from string Stock

    [StringLength(maximumLength: 1000)]
    public string? Description { get; set; }  // New

    public int? CategoryId { get; set; }

    public string ImgUrl { get; set; }
    public bool IsFavorite { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    // Timestamps (replace CreatedDate)
    [Column(TypeName = "datetimeoffset")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Column(TypeName = "datetimeoffset")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Category? Category { get; set; }


    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}