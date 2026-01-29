

namespace OnlineBookManagementSystem.Shared.Extensions
{
    /// <summary>
    /// Main service collection extension that orchestrates all focused service registrations.
    /// Follows Single Responsibility Principle by delegating to specialized extension classes.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Configuration
            services.AddDatabase(configuration);

            // Authentication & Authorization (JWT, Identity, Policies)
            services.AddAuthentication(configuration);

            // Application Services (Domain Services, Use Cases, Repositories)
            services.AddApplicationServices(configuration);

            // MVC Configuration (Controllers, Views, Session)
            services.AddMvcConfiguration();

            // Security Configuration (Rate Limiting, Antiforgery)
            services.AddSecurityConfiguration(configuration);

            // API Documentation (Swagger)
            services.AddSwaggerConfiguration(configuration);

            // Health Checks
            services.AddHealthCheckConfiguration(configuration);

            return services;
        }
    }
}
