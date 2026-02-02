using OnlineBookManagementSystem.Shared.Utilities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Categories;

/// <summary>
/// Universal CategoryViewModel - Serves all roles (Public, User, Admin, SuperAdmin)
/// Uses capability flags to control what actions are available
/// </summary>
public class CategoryViewModel
{
    // Core Category Data (Always Present)
    public List<CategoryItemViewModel> Categories { get; set; } = new();
    public int TotalCategories { get; set; }
    public bool HasCategories => Categories.Any();

    // Capability-Based Metadata (NO ROLES)
    public CategoryCapabilities Capabilities { get; set; } = new();
}

/// <summary>
/// Individual category item for display
/// </summary>
public class CategoryItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BookCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Books in this category (for public display)
    public List<CategoryBookViewModel>? Books { get; set; }

    // UI-specific computed properties
    public string FormattedCreatedDate => FormattingExtensions.FormatDate(CreatedAt, "MMM dd, yyyy");
    public string FormattedUpdatedDate => FormattingExtensions.FormatDate(UpdatedAt, "MMM dd, yyyy") ?? "Never";
    public bool HasBooks => BookCount > 0;
    public string BookCountText => BookCount == 1 ? "1 book" : $"{BookCount} books";
    public bool CanDelete => BookCount == 0; // Can only delete empty categories
}

/// <summary>
/// Book information for category display
/// </summary>
public class CategoryBookViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }

    public string FormattedPrice => FormattingExtensions.FormatCurrency(Price);
    public bool IsInStock => StockQuantity > 0;
}

/// <summary>
/// Capability-based context for category rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class CategoryCapabilities
{
    // View Capabilities
    public bool CanViewCategories { get; set; } = true; // Everyone can view categories
    public bool CanViewCategoryDetails { get; set; } = true; // Everyone can see category details
    public bool CanViewBookCount { get; set; } = true; // Everyone can see book counts
    public bool CanViewTechnicalDetails { get; set; } = false; // Admin/SuperAdmin see created/updated dates

    // Action Capabilities
    public bool CanCreate { get; set; } = false; // Admin/SuperAdmin can create categories
    public bool CanEdit { get; set; } = false; // Admin/SuperAdmin can edit categories
    public bool CanDelete { get; set; } = false; // Admin/SuperAdmin can delete empty categories

    // Book Interaction Capabilities
    public bool CanViewBookDetails { get; set; } = true; // Everyone can view book details
    public bool CanAddToCart { get; set; } = false; // Authenticated users can add to cart

    // UI Context (NOT roles)
    public bool IsAuthenticated { get; set; } = false;
    public string PageTitle { get; set; } = "Categories";
    public string ViewMode { get; set; } = "list"; // "list" for public browse, "manage" for admin
    public string BackLinkText { get; set; } = "Back";
    public string BackLinkUrl { get; set; } = "/";
    public string LayoutClass { get; set; } = "public-layout";
}