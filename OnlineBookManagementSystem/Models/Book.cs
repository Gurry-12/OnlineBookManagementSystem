using System;
using System.Collections.Generic;

namespace OnlineBookManagementSystem.Models;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Author { get; set; }

    public decimal? Price { get; set; }

    public string? Isbn { get; set; }

    public string? ImgUrl { get; set; }

    public string? Stock { get; set; }

    public int? CategoryId { get; set; }

    public bool? IsFavorite { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Category? Category { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}
