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

    // Capability-Based Metadata (NO ROLES)
    public BookDetailsCapabilities Capabilities { get; set; } = new();

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
/// Capability-based context for conditional rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class BookDetailsCapabilities
{
    // View Capabilities
    public bool CanView { get; set; } = true;
    public bool CanViewTechnicalDetails { get; set; } = false;
    public bool CanViewMetadata { get; set; } = false;

    // Action Capabilities  
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanAddToCart { get; set; } = false;
    public bool CanFavorite { get; set; } = false;
    public bool CanReview { get; set; } = false;

    // Navigation Capabilities
    public bool CanViewReviews { get; set; } = true;
    public bool CanModerateReviews { get; set; } = false;

    // UI Context (NOT roles)
    public bool IsAuthenticated { get; set; } = false;
    public string BackLinkText { get; set; } = "Back to Books";
    public string BackLinkUrl { get; set; } = "/Public/Browse";
    public string PageTitle { get; set; } = "Book Details";
    public string LayoutClass { get; set; } = "public-layout";
}