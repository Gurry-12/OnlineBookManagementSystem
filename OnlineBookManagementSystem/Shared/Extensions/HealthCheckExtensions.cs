using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Shared.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("Features:EnableHealthChecks"))
            {
                services.AddHealthChecks()
                    .AddDbContextCheck<BookManagementContext>();
            }

            return services;
        }
    }
}