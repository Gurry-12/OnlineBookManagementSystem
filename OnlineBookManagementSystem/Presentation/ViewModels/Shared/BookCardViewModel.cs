using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Shared;

public class BookCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? CategoryName { get; set; }
    public double AverageRating { get; set; }

    public bool IsInStock => StockQuantity > 0;
    public string FormattedPrice => Price.ToString("C");

    // Capabilities
    public bool CanViewDetails { get; set; }
    public bool CanAddToCart { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanFavorite { get; set; }
    public bool IsFavorite { get; set; }
    public bool CanViewTechnicalInfo { get; set; }

    // Routing
    public string DetailsControllerName { get; set; } = "Books";
    public string DetailsActionName { get; set; } = "Details";
    
    // UI Helpers
    public string CustomCssClass { get; set; } = "";

    public static BookCardViewModel FromEntity(Book book, BookCardCapabilities capabilities)
    {
        return new BookCardViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ImageUrl = book.ImageUrl,
            Price = book.Price.Amount,
            StockQuantity = book.StockQuantity,
            CategoryName = book.Category?.Name,
            AverageRating = book.AverageRating,
            CanViewDetails = capabilities.CanViewDetails,
            CanAddToCart = capabilities.CanAddToCart,
            CanEdit = capabilities.CanEdit,
            CanDelete = capabilities.CanDelete,
            CanFavorite = capabilities.CanFavorite,
            CanViewTechnicalInfo = capabilities.CanViewTechnicalInfo,
            DetailsControllerName = capabilities.DetailsControllerName,
            DetailsActionName = capabilities.DetailsActionName,
            CustomCssClass = capabilities.CustomCssClass
        };
    }

    public static BookCardViewModel FromCategoryBookViewModel(OnlineBookManagementSystem.Presentation.ViewModels.Categories.CategoryBookViewModel book, BookCardCapabilities capabilities)
    {
        return new BookCardViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ImageUrl = book.ImageUrl,
            Price = book.Price,
            StockQuantity = book.StockQuantity,
            CategoryName = null, // Can map if we have it
            AverageRating = 0, // Not available here
            CanViewDetails = capabilities.CanViewDetails,
            CanAddToCart = capabilities.CanAddToCart,
            CanEdit = capabilities.CanEdit,
            CanDelete = capabilities.CanDelete,
            CanFavorite = capabilities.CanFavorite,
            CanViewTechnicalInfo = capabilities.CanViewTechnicalInfo,
            DetailsControllerName = capabilities.DetailsControllerName,
            DetailsActionName = capabilities.DetailsActionName,
            CustomCssClass = capabilities.CustomCssClass
        };
    }
}

public class BookCardCapabilities
{
    public bool CanViewDetails { get; set; } = true;
    public bool CanAddToCart { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanFavorite { get; set; }
    public bool CanViewTechnicalInfo { get; set; }
    public string DetailsControllerName { get; set; } = "Books";
    public string DetailsActionName { get; set; } = "Details";
    public string CustomCssClass { get; set; } = "";
}
