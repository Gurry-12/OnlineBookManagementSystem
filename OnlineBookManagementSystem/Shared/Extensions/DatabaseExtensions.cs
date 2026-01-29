using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Shared.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BookManagementContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);
            });

            return services;
        }
    }
}