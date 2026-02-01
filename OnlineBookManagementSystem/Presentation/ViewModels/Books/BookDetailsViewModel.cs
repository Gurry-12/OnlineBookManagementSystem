using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;
using OnlineBookManagementSystem.Shared.Utilities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Books;

/// <summary>
/// Universal BookDetailsViewModel - Serves all roles (Admin, User, Public)
/// Uses nullable properties and role-based metadata to handle optional fields
/// </summary>
public class BookDetailsViewModel
{
    // Core Book Information (Always Present)
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ISBN { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? PublicationDate { get; set; }
    
    // Admin-Only Properties (Nullable for other roles)
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsDeleted { get; set; }
    
    // User-Specific Properties (Nullable for non-authenticated users)
    public bool? IsFavorite { get; set; }
    public bool? CanReview { get; set; }
    
    // Review Information (Always Present but may be empty)
    public BookRatingViewModel Rating { get; set; } = new();
    public ReviewSubmissionViewModel? ReviewForm { get; set; }
    
    // Role-Based Metadata
    public RoleContext RoleContext { get; set; } = new();
    
    // Computed Properties using FormattingExtensions
    public string FormattedPrice => FormattingExtensions.FormatCurrency(Price);
    public bool IsInStock => StockQuantity > 0;
    public string StockStatus => IsInStock ? $"In Stock ({StockQuantity} available)" : "Out of Stock";
    public string StockBadgeClass => IsInStock ? "badge bg-success" : "badge bg-danger";
    public string FormattedPublicationDate => FormattingExtensions.FormatDate(PublicationDate, "MMM dd, yyyy");
    public string FormattedCreatedAt => FormattingExtensions.FormatDate(CreatedAt, "MMM dd, yyyy");
    public string FormattedUpdatedAt => FormattingExtensions.FormatDate(UpdatedAt, "MMM dd, yyyy");
    public string DeletedStatus => IsDeleted == true ? "Deleted" : "Active";
    public string DeletedBadgeClass => IsDeleted == true ? "badge bg-danger" : "badge bg-success";

    public bool IsPublicView { get; internal set; }
}

/// <summary>
/// Role-based context metadata for conditional rendering
/// </summary>
public class RoleContext
{
    public string UserRole { get; set; } = "Public";
    public bool IsAuthenticated { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanAddToCart { get; set; } = false;
    public bool CanToggleFavorite { get; set; } = false;
    public bool ShowAdminMetadata { get; set; } = false;
    public bool ShowTechnicalDetails { get; set; } = false;
    public string ViewMode { get; set; } = "Browse"; // Browse, Edit, Admin, Demo
}