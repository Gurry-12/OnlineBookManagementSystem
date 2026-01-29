using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Showcase
{
    /// <summary>
    /// View model for the technical details page showcasing architecture and implementation
    /// </summary>
    public class TechnicalDetailsViewModel
    {
        public ArchitectureViewModel Architecture { get; set; } = new();
        public TechnicalStackViewModel TechnicalStack { get; set; } = new();
        public List<OnlineBookManagementSystem.Core.Domain.Entities.TechnicalHighlight> TechnicalHighlights { get; set; } = new();
        public PerformanceStatsViewModel PerformanceMetrics { get; set; } = new();
    }
}