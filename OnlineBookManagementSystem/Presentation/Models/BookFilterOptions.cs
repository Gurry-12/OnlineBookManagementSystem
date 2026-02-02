namespace OnlineBookManagementSystem.Presentation.Models
{
    /// <summary>
    /// Encapsulates book filtering and pagination options.
    /// Used to avoid parameter explosion in controller actions.
    /// </summary>
    public class BookFilterOptions
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? SortBy { get; set; }
        public bool? InStock { get; set; }

        /// <summary>
        /// Checks if any filters are applied
        /// </summary>
        public bool HasFilters =>
            !string.IsNullOrEmpty(Search) ||
            CategoryId.HasValue ||
            !string.IsNullOrEmpty(SortBy) ||
            InStock.HasValue;
    }
}
