namespace OnlineBookManagementSystem.Core.Application.DTOs;

public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public DateTime? PublishedDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string? Publisher { get; set; }
    public int? CategoryId { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public string? ImageUrl { get; set; }
    public int? Pages { get; set; }
    public string? ISBN { get; set; }
    public bool IsFeatured { get; set; } = false;
}
