using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Showcase
{
    /// <summary>
    /// View model for the interactive demo page showcasing live system functionality
    /// </summary>
    public class InteractiveDemoViewModel
    {
        public BookListViewModel FeaturedBooks { get; set; } = new();
        public List<CategoryWithCountViewModel> Categories { get; set; } = new();
        public SystemStatisticsViewModel Statistics { get; set; } = new();
        public List<OnlineBookManagementSystem.Core.Domain.Entities.FeatureShowcase> FeatureShowcases { get; set; } = new();
    }
}