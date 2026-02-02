namespace OnlineBookManagementSystem.Presentation.ViewModels.Books;

/// <summary>
/// Capability-based context for book list rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class BookListCapabilities
{
    // View Capabilities
    public bool CanViewBooks { get; set; } = true;
    public bool CanViewBookDetails { get; set; } = true;
    public bool CanViewTechnicalInfo { get; set; } = false;

    // Action Capabilities per Book
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanAddToCart { get; set; } = false;
    public bool CanFavorite { get; set; } = false;
    public bool CanCreate { get; set; } = false;

    // List Management Capabilities
    public bool CanFilter { get; set; } = true;
    public bool CanSearch { get; set; } = true;
    public bool CanSort { get; set; } = true;
    public bool CanPaginate { get; set; } = true;

    // UI Context (NOT roles)
    public bool IsAuthenticated { get; set; } = false;
    public string PageTitle { get; set; } = "Browse Books";
    public string CreateButtonText { get; set; } = "Add New Book";
    public string DetailsActionName { get; set; } = "Details";
    public string DetailsControllerName { get; set; } = "Books";
    public string LayoutClass { get; set; } = "public-layout";
}