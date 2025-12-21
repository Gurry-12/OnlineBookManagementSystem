using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; } = 0;

    public DateTime? OrderDate { get; set; } = DateTime.UtcNow;  // Changed to DateTime

    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }  // Renamed from ShippingAddress for brevity

    [StringLength(50)]
    public string PaymentMethod { get; set; } = "Unpaid";  // Default

    [StringLength(50)]
    public string PaymentStatus { get; set; } = "Unpaid";  // New

    public bool IsDeleted { get; set; } = false;  // New

    // Timestamps
    [Column(TypeName = "DateTime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "DateTime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual User? User { get; set; }
}