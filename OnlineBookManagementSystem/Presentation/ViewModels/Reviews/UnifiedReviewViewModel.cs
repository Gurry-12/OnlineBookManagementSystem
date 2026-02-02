using OnlineBookManagementSystem.Shared.Utilities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Reviews;

/// <summary>
/// Universal ReviewViewModel - Serves all roles (Public, User, Admin, SuperAdmin)
/// Uses capability flags to control what actions are available
/// </summary>
public class UnifiedReviewViewModel
{
    // Core Review Data (Always Present)
    public int Id { get; set; }
    public int BookId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsEdited { get; set; }

    // Book Information (for moderation views)
    public string? BookTitle { get; set; }
    public string? BookAuthor { get; set; }

    // Moderation Information (Admin/SuperAdmin only)
    public string? Status { get; set; } // Pending, Approved, Rejected, Flagged
    public string? ModerationReason { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public string? ModeratedBy { get; set; }

    // Capability-Based Metadata (NO ROLES)
    public ReviewCapabilities Capabilities { get; set; } = new();

    // Computed Properties
    public string FormattedCreatedAt => FormattingExtensions.FormatDate(CreatedAt, "MMM dd, yyyy");
    public string FormattedUpdatedAt => FormattingExtensions.FormatDate(UpdatedAt, "MMM dd, yyyy");
    public string FormattedModeratedAt => FormattingExtensions.FormatDate(ModeratedAt, "MMM dd, yyyy HH:mm");
    public bool IsPending => Status == "Pending";
    public bool IsApproved => Status == "Approved";
    public bool IsRejected => Status == "Rejected";
    public bool IsFlagged => Status == "Flagged";
}

/// <summary>
/// Collection of reviews with pagination
/// </summary>
public class UnifiedReviewListViewModel
{
    public List<UnifiedReviewViewModel> Reviews { get; set; } = new();
    public int TotalReviews { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    // Book context (when showing reviews for a specific book)
    public int? BookId { get; set; }
    public string? BookTitle { get; set; }

    // Capability-Based Metadata (NO ROLES)
    public ReviewListCapabilities Capabilities { get; set; } = new();
}

/// <summary>
/// Capability-based context for individual review rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class ReviewCapabilities
{
    // View Capabilities
    public bool CanView { get; set; } = true; // Everyone can view approved reviews
    public bool CanViewModerationDetails { get; set; } = false; // Admin/SuperAdmin see moderation info
    public bool CanViewUserDetails { get; set; } = false; // Admin/SuperAdmin see user ID, email

    // Action Capabilities
    public bool CanEditOwn { get; set; } = false; // Users can edit their own reviews
    public bool CanDeleteOwn { get; set; } = false; // Users can delete their own reviews
    public bool CanApprove { get; set; } = false; // Admin/SuperAdmin can approve reviews
    public bool CanReject { get; set; } = false; // Admin/SuperAdmin can reject reviews
    public bool CanFlag { get; set; } = false; // Admin/SuperAdmin can flag reviews

    // UI Context (NOT roles)
    public bool IsOwnReview { get; set; } = false; // True if viewing user's own review
    public bool IsAuthenticated { get; set; } = false;
    public string ViewMode { get; set; } = "public"; // "public", "moderation"
}

/// <summary>
/// Capability-based context for review list rendering - NO ROLE CHECKS IN VIEWS
/// </summary>
public class ReviewListCapabilities
{
    // View Capabilities
    public bool CanViewReviews { get; set; } = true; // Everyone can view approved reviews
    public bool CanViewAllStatuses { get; set; } = false; // Admin/SuperAdmin see all review statuses
    public bool CanViewModerationQueue { get; set; } = false; // Admin/SuperAdmin see pending reviews

    // Action Capabilities
    public bool CanCreate { get; set; } = false; // Authenticated users can create reviews
    public bool CanModerate { get; set; } = false; // Admin/SuperAdmin can moderate reviews
    public bool CanBulkApprove { get; set; } = false; // Admin/SuperAdmin can bulk approve
    public bool CanBulkReject { get; set; } = false; // Admin/SuperAdmin can bulk reject

    // Filtering & Sorting
    public bool CanFilterByStatus { get; set; } = false; // Admin/SuperAdmin can filter by status
    public bool CanFilterByRating { get; set; } = true; // Everyone can filter by rating
    public bool CanSortReviews { get; set; } = true; // Everyone can sort reviews

    // UI Context (NOT roles)
    public bool IsAuthenticated { get; set; } = false;
    public string ViewMode { get; set; } = "public"; // "public", "moderation"
    public string PageTitle { get; set; } = "Reviews";
    public string BackLinkText { get; set; } = "Back";
    public string BackLinkUrl { get; set; } = "/";
}