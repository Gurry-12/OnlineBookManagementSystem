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
                "/Presentation/Views/{1}/{0}.cshtml",
                "/Presentation/Views/Shared/{0}.cshtml"
            };

            // Return Presentation locations first, then fallback to default locations
            return presentationLocations.Concat(viewLocations);
        }
    }
}