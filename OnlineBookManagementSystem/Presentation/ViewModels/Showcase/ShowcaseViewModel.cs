using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Showcase
{
    /// <summary>
    /// Main view model for the enhanced public showcase landing page
    /// </summary>
    public class ShowcaseViewModel
    {
        public ProjectOverviewViewModel ProjectOverview { get; set; } = new();
        public TechnicalStackViewModel TechnicalStack { get; set; } = new();
        public ArchitectureViewModel Architecture { get; set; } = new();
        public SystemStatisticsViewModel Statistics { get; set; } = new();
        public DeveloperStoryViewModel DeveloperStory { get; set; } = new();
        public List<FeatureHighlightViewModel> FeatureHighlights { get; set; } = new();
        public BookListViewModel FeaturedBooks { get; set; } = new();
        public List<CategoryWithCountViewModel> Categories { get; set; } = new();
        public ContactInformationViewModel ContactInfo { get; set; } = new();
    }

    /// <summary>
    /// Project overview and vision presentation
    /// </summary>
    public class ProjectOverviewViewModel
    {
        public string Vision { get; set; } = string.Empty;
        public string ValueProposition { get; set; } = string.Empty;
        public List<string> KeyFeatures { get; set; } = new();
        public string ProjectStatus { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string HeroTitle { get; set; } = string.Empty;
        public string HeroSubtitle { get; set; } = string.Empty;
    }

    /// <summary>
    /// Technical stack and technology showcase
    /// </summary>
    public class TechnicalStackViewModel
    {
        public List<TechnologyViewModel> BackendTechnologies { get; set; } = new();
        public List<TechnologyViewModel> FrontendTechnologies { get; set; } = new();
        public List<TechnologyViewModel> DatabaseTechnologies { get; set; } = new();
        public List<TechnologyViewModel> DevOpsTechnologies { get; set; } = new();
        public List<TechnologyViewModel> TestingTechnologies { get; set; } = new();
    }

    /// <summary>
    /// Individual technology information
    /// </summary>
    public class TechnologyViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string DocumentationUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Architecture principles and implementation showcase
    /// </summary>
    public class ArchitectureViewModel
    {
        public string ArchitectureType { get; set; } = "Clean Architecture";
        public List<LayerViewModel> Layers { get; set; } = new();
        public List<PrincipleViewModel> SOLIDPrinciples { get; set; } = new();
        public string DiagramUrl { get; set; } = string.Empty;
        public string ArchitectureDescription { get; set; } = string.Empty;
        public List<string> ArchitectureBenefits { get; set; } = new();
    }

    /// <summary>
    /// Architecture layer information
    /// </summary>
    public class LayerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Responsibilities { get; set; } = string.Empty;
        public List<string> Components { get; set; } = new();
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// SOLID principle implementation showcase
    /// </summary>
    public class PrincipleViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Implementation { get; set; } = string.Empty;
        public string CodeExample { get; set; } = string.Empty;
        public string Benefit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Live system statistics
    /// </summary>
    public class SystemStatisticsViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public DateTime LastUpdated { get; set; }
        public PerformanceStatsViewModel Performance { get; set; } = new();
        public TechnicalStatsViewModel Technical { get; set; } = new();
    }

    /// <summary>
    /// Performance metrics
    /// </summary>
    public class PerformanceStatsViewModel
    {
        public double PageLoadTime { get; set; }
        public int PerformanceScore { get; set; }
        public double DatabaseResponseTime { get; set; }
        public int CacheHitRate { get; set; }
    }

    /// <summary>
    /// Technical implementation statistics
    /// </summary>
    public class TechnicalStatsViewModel
    {
        public int LinesOfCode { get; set; }
        public int TestCoverage { get; set; }
        public string ArchitectureCompliance { get; set; } = string.Empty;
        public int CodeQualityScore { get; set; }
        public int SecurityScore { get; set; }
    }

    /// <summary>
    /// Developer story and project journey
    /// </summary>
    public class DeveloperStoryViewModel
    {
        public string Motivation { get; set; } = string.Empty;
        public string ChallengesSolved { get; set; } = string.Empty;
        public List<string> TechnicalDecisions { get; set; } = new();
        public List<string> LessonsLearned { get; set; } = new();
        public string FutureVision { get; set; } = string.Empty;
        public List<MilestoneViewModel> ProjectTimeline { get; set; } = new();
    }

    /// <summary>
    /// Project milestone information
    /// </summary>
    public class MilestoneViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Achievements { get; set; } = new();
    }

    /// <summary>
    /// Feature highlight showcase
    /// </summary>
    public class FeatureHighlightViewModel
    {
        public string FeatureName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ScreenshotUrl { get; set; } = string.Empty;
        public List<string> TechnicalDetails { get; set; } = new();
        public string DemoUrl { get; set; } = string.Empty;
        public bool IsInteractive { get; set; }
        public string Category { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Category with book count for public display
    /// </summary>
    public class CategoryWithCountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BookCount { get; set; }
        public string IconClass { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contact and collaboration information
    /// </summary>
    public class ContactInformationViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string LinkedIn { get; set; } = string.Empty;
        public string GitHub { get; set; } = string.Empty;
        public string Portfolio { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<string> AvailableForRoles { get; set; } = new();
        public bool OpenToCollaboration { get; set; }

        // Technical documentation and repository links
        public string TechnicalDocumentationUrl { get; set; } = string.Empty;
        public string ArchitectureDocumentationUrl { get; set; } = string.Empty;
        public string ApiDocumentationUrl { get; set; } = string.Empty;
        public List<SocialMediaLinkViewModel> SocialMediaLinks { get; set; } = new();
        public List<TechnicalResourceViewModel> TechnicalResources { get; set; } = new();
    }

    /// <summary>
    /// Social media link information
    /// </summary>
    public class SocialMediaLinkViewModel
    {
        public string Platform { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Technical resource link information
    /// </summary>
    public class TechnicalResourceViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Documentation", "Repository", "Demo", etc.
    }

    /// <summary>
    /// Technical highlight entity
    /// </summary>
    public class TechnicalHighlight
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CodeExample { get; set; } = string.Empty;
        public string DocumentationUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Feature showcase entity
    /// </summary>
    public class FeatureShowcase
    {
        public int Id { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ScreenshotUrl { get; set; } = string.Empty;
        public List<string> TechnicalDetails { get; set; } = new();
        public string DemoUrl { get; set; } = string.Empty;
        public bool IsInteractive { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contact form view model for developer inquiries
    /// </summary>
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an inquiry type")]
        public string InquiryType { get; set; } = string.Empty; // "Job Opportunity", "Collaboration", "Technical Question", "Other"

        public string PreferredContactMethod { get; set; } = "Email"; // "Email", "Phone", "LinkedIn"

        public string Timeline { get; set; } = string.Empty; // "Immediate", "Within 1 week", "Within 1 month", "Flexible"
    }

    /// <summary>
    /// Collaboration inquiry view model for developer contact
    /// </summary>
    public class CollaborationInquiryViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = string.Empty;

        public string InquiryType { get; set; } = string.Empty; // "Job Opportunity", "Collaboration", "Technical Question", "Other"
        public string Company { get; set; } = string.Empty;
        public string PreferredContactMethod { get; set; } = "Email";
    }
}