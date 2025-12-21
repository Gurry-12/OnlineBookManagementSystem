using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int BookId { get; set; }

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; } = 0;  // New: Quantity * Price

    public virtual Book Book { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}