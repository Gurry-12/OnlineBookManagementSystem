namespace OnlineBookManagementSystem.Core.Application.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Publisher { get; set; }
    public int? CategoryId { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public int? Pages { get; set; }
    public string? ISBN { get; set; }
    public bool IsAvailable { get; set; }
    public double AverageRating { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int LowStockThreshold { get; set; }
    public DateTime? PublicationDate { get; set; }
    public int TotalReviews { get; set; }
    public bool IsFavorite { get; set; }
    public string? CategoryName { get; set; }
    public bool IsLowStock { get; set; }

    // Parameterless constructor for serialization
    public BookDto() { }

    // Constructor for use cases
    public BookDto(
        int id,
        string title,
        string author,
        decimal price,
        string? isbn,
        DateTime? publicationDate,
        string? imageUrl,
        int stockQuantity,
        int lowStockThreshold,
        string? description,
        int? categoryId,
        string? categoryName,
        bool isFeatured,
        double averageRating,
        bool isAvailable,
        bool isLowStock,
        DateTime createdAt,
        DateTime updatedAt)
    {
        Id = id;
        Title = title;
        Author = author;
        Price = price;
        ISBN = isbn;
        PublicationDate = publicationDate;
        ImageUrl = imageUrl;
        StockQuantity = stockQuantity;
        LowStockThreshold = lowStockThreshold;
        Description = description;
        CategoryId = categoryId;
        CategoryName = categoryName;
        IsFeatured = isFeatured;
        AverageRating = averageRating;
        IsAvailable = isAvailable;
        IsLowStock = isLowStock;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        TotalReviews = 0;
        IsFavorite = false;
        IsDeleted = false;
    }
}
