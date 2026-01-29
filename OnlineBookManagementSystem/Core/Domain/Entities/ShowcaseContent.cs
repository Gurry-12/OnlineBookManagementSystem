using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    /// <summary>
    /// Represents showcase content sections for the public portfolio
    /// </summary>
    public class ShowcaseContent : BaseEntity
    {
        public string SectionName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? MetaData { get; set; } // JSON for additional properties
    }

    /// <summary>
    /// Represents technical highlights and implementation details
    /// </summary>
    public class TechnicalHighlight : BaseEntity
    {
        public string Category { get; set; } = string.Empty; // "Architecture", "Performance", "Security", etc.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CodeExample { get; set; }
        public string? DocumentationUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? TechnicalDetails { get; set; } // JSON for structured technical info
    }

    /// <summary>
    /// Represents feature showcases with screenshots and demos
    /// </summary>
    public class FeatureShowcase : BaseEntity
    {
        public string FeatureName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ScreenshotUrl { get; set; }
        public string? DemoUrl { get; set; }
        public bool IsInteractive { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? TechnicalDetails { get; set; } // JSON for technical implementation details
        public string Category { get; set; } = string.Empty; // "User Features", "Admin Features", "Technical Features"
    }
}