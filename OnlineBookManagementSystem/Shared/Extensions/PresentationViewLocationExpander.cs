using Microsoft.AspNetCore.Mvc.Razor;

namespace OnlineBookManagementSystem.Shared.Extensions
{
    public class PresentationViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // No additional values needed
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // Add Presentation folder locations first (higher priority)
            var presentationLocations = new[]
            {
                // Controller-specific views
                "/Presentation/Views/{1}/{0}.cshtml",
                
                // Shared views
                "/Presentation/Views/Shared/{0}.cshtml",
                
                // Area-specific views (if using areas)
                "/Presentation/Views/{2}/{1}/{0}.cshtml",
                "/Presentation/Views/{2}/Shared/{0}.cshtml",
                
                // Additional fallback patterns
                "/Presentation/Views/Shared/Layouts/{0}.cshtml",
                "/Presentation/Views/Shared/Partials/{0}.cshtml"
            };

            // Return Presentation locations first, then fallback to default locations
            return presentationLocations.Concat(viewLocations);
        }
    }
}