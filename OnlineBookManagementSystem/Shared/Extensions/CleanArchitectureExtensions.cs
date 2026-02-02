using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Cart;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Users;
using OnlineBookManagementSystem.Core.Application.UseCases.Books;
using OnlineBookManagementSystem.Infrastructure.Data;
using OnlineBookManagementSystem.Infrastructure.Data.Repositories;
using OnlineBookManagementSystem.Infrastructure.Data.Repositories.Analytics;
using OnlineBookManagementSystem.Infrastructure.Data.Repositories.Cart;
using OnlineBookManagementSystem.Infrastructure.Data.Repositories.Orders;
using OnlineBookManagementSystem.Infrastructure.Data.Repositories.Users;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Analytics;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Cart;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Orders;

namespace OnlineBookManagementSystem.Shared.Extensions
{
    public static class CleanArchitectureExtensions
    {
        public static IServiceCollection AddCleanArchitecture(this IServiceCollection services)
        {
            // Register repositories - following Repository pattern with proper abstraction
            // Scoped: Repositories work with database context and should be per-request
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

            // Scoped: Unit of Work manages database transactions per request
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register refactored services - following SRP and DIP
            // These services only handle business logic, delegate data access to repositories
            services.AddScoped<IOrderQueryService, RefactoredOrderQueryService>();
            services.AddScoped<IOrderCommandService, RefactoredOrderCommandService>();
            services.AddScoped<ICartService, RefactoredCartService>();
            services.AddScoped<IBookAnalyticsService, RefactoredAnalyticsService>();

            // Register use cases - Clean Architecture application layer
            // Scoped: Use cases orchestrate business operations and work with repositories
            services.AddScoped<ICreateBookUseCase, CreateBookUseCase>();
            services.AddScoped<IGetBookByIdUseCase, GetBookByIdUseCase>();
            services.AddScoped<ISearchBooksUseCase, SearchBooksUseCase>();

            return services;
        }
    }
}
