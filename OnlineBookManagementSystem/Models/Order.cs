using System;
using System.Collections.Generic;

namespace OnlineBookManagementSystem.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateOnly? OrderDate { get; set; }

    public string? Status { get; set; }

    public string? FullName { get; set; }

    public string? Address { get; set; }

    public string? PaymentMethod { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User? User { get; set; }
}
