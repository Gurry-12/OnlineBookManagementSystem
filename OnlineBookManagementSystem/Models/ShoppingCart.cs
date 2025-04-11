using System;
using System.Collections.Generic;

namespace OnlineBookManagementSystem.Models;

public partial class ShoppingCart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public int? Quantity { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
