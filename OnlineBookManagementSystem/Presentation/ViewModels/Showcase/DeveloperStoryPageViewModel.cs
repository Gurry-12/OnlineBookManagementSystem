namespace OnlineBookManagementSystem.Presentation.ViewModels.Showcase
{
    /// <summary>
    /// View model for the developer story page showcasing project journey and vision
    /// </summary>
    public class DeveloperStoryPageViewModel
    {
        public DeveloperStoryViewModel DeveloperStory { get; set; } = new();
        public ProjectOverviewViewModel ProjectOverview { get; set; } = new();
        public ContactInformationViewModel ContactInfo { get; set; } = new();
        public List<FeatureHighlightViewModel> TechnicalAchievements { get; set; } = new();
    }
}