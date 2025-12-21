using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

public partial class ShoppingCart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public int Quantity { get; set; } = 1;

    public bool IsDeleted { get; set; } = false;

    [Column(TypeName = "DateTime")]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;  // New

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}