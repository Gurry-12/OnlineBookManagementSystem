namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin
{
    /// <summary>
    /// ViewModel for category management page
    /// Prevents entity leakage
    /// </summary>
    public class CategoryManagementViewModel
    {
        public List<CategoryItemViewModel> Categories { get; set; } = new();
        public int TotalCategories { get; set; }
        public bool HasCategories => Categories.Any();
    }

    public class CategoryItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BookCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // UI-specific computed properties
        public string FormattedCreatedDate => CreatedAt.ToString("MMM dd, yyyy");
        public string FormattedUpdatedDate => UpdatedAt?.ToString("MMM dd, yyyy") ?? "Never";
        public bool HasBooks => BookCount > 0;
        public string BookCountText => BookCount == 1 ? "1 book" : $"{BookCount} books";
        public bool CanDelete => BookCount == 0; // Can only delete empty categories
    }
}
