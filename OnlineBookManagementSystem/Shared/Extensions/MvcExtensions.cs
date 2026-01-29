namespace OnlineBookManagementSystem.Shared.Extensions
{
    public static class MvcExtensions
    {
        public static IServiceCollection AddMvcConfiguration(this IServiceCollection services)
        {
            // MVC with custom view locations for Clean Architecture
            services.AddControllersWithViews()
                .AddRazorOptions(options =>
                {
                    // Add custom view location expander for Presentation folder
                    options.ViewLocationExpanders.Add(new PresentationViewLocationExpander());
                });

            // Session & Cache
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddHttpContextAccessor();

            return services;
        }
    }
}