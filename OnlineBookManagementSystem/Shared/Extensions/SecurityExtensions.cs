using System.Threading.RateLimiting;

namespace OnlineBookManagementSystem.Shared.Extensions
{
    public static class SecurityExtensions
    {
        public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Rate Limiting
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = configuration.GetValue<int>("RateLimiting:PermitLimit", 100),
                            QueueLimit = configuration.GetValue<int>("RateLimiting:QueueLimit", 10),
                            Window = TimeSpan.Parse(configuration["RateLimiting:Window"] ?? "00:01:00")
                        }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync($"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.", token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
                    }
                };
            });

            // Antiforgery
            services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

            return services;
        }
    }
}