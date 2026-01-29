using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Reviews;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users;
using OnlineBookManagementSystem.Core.Application.Interfaces.Helpers;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Payment;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Showcase;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Analytics;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Showcase;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Books;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Cart;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Categories;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Charts;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Orders;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Reviews;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Users;
using OnlineBookManagementSystem.Infrastructure.Services.Helpers;
using OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Authentication;
using OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Caching;
using OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Email;
using OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Logging;
using OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Payment;
using OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Performance;
using OnlineBookManagementSystem.Infrastructure.Services.System;

namespace OnlineBookManagementSystem.Shared.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Clean Architecture Services
            services.AddCleanArchitecture();

            // Legacy Application Services (to be gradually migrated)
            // Scoped: Per-request lifetime for stateful services that work with database context
            services.AddScoped<ICategoryInterface, CategoryServices>();

            // New focused Book Services (Single Responsibility Principle)
            // Scoped: Per-request lifetime for services that interact with database and maintain state
            services.AddScoped<IBookQueryService, BookQueryService>();
            services.AddScoped<IBookCommandService, BookCommandService>();
            services.AddScoped<IBookValidationService, BookValidationService>();
            services.AddScoped<IBookFavoriteService, BookFavoriteService>();
            // Note: IBookAnalyticsService is registered in CleanArchitectureExtensions

            // Scoped: Mapping service for consistent object mapping
            services.AddScoped<IMappingService, MappingService>();

            // Scoped: Authentication and authorization services need per-request state
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleBasedRedirectionService, RoleBasedRedirectionService>();

            // Showcase Services for Enhanced Public Portfolio
            // Scoped: Public demo service needs database access and caching
            services.AddScoped<IPublicDemoService, PublicDemoService>();

            // Note: ICartService is registered in CleanArchitectureExtensions

            // Scoped: Activity logger needs to track per-request context
            services.AddScoped<IActivityLogger, ActivityLogger>();

            // REMOVED: Old Order Services - replaced by refactored versions in CleanArchitectureExtensions
            // The refactored services follow SRP and use proper repository pattern
            services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();

            // Note: IUsersService implemented by CompositeUsersService (delegates to focused services)
            services.AddScoped<IUsersService, CompositeUsersService>();

            // New focused User Services (Single Responsibility Principle)
            // Scoped: User services need per-request lifetime for database and authentication operations
            services.AddScoped<IUserQueryService, UserQueryService>();
            services.AddScoped<IUserCommandService, UserCommandService>();
            services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
            services.AddScoped<IUserApprovalService, UserApprovalService>();

            // Transient: DNS checker is stateless and lightweight, can be created per use
            services.AddTransient<IDnsChecker, DNSCheckerHelper>();

            // Scoped: System settings service needs database access, so must be scoped
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();

            // Singleton: Cache service should be shared across the application
            services.AddSingleton<ICacheService, CacheService>();

            // Performance Optimization Services
            // Singleton: Multi-level cache service should be shared across the application
            services.AddSingleton<IMultiLevelCacheService, MultiLevelCacheService>();
            
            // Singleton: Graceful degradation service should be shared for circuit breaker state
            services.AddSingleton<IGracefulDegradationService, GracefulDegradationService>();
            
            // Scoped: Asset optimization service may need per-request context
            services.AddScoped<IAssetOptimizationService, AssetOptimizationService>();

            // Transient: Error view model factory is stateless and lightweight
            services.AddTransient<IErrorViewModelFactory, ErrorViewModelFactory>();

            // Scoped: Concurrency handler for managing database concurrency conflicts
            services.AddScoped<IConcurrencyHandler, ConcurrencyHandler>();

            // Chart Data Providers (Strategy Pattern - OCP Compliance)
            // Transient: Chart providers are stateless and can be created per use
            services.AddTransient<IChartDataProvider, MonthlyChartDataProvider>();
            services.AddTransient<IChartDataProvider, CategoryChartDataProvider>();
            services.AddTransient<IChartDataProvider, AuthorChartDataProvider>();
            services.AddTransient<IChartDataProvider, FavoritesChartDataProvider>();
            services.AddTransient<IChartDataProvider, RevenueChartDataProvider>();
            services.AddTransient<IChartDataProvider, OrderStatusChartDataProvider>();

            // Scoped: Review service works with database context and user-specific operations
            services.AddScoped<IReviewService, ReviewService>();

            // Email Configuration
            services.Configure<OnlineBookManagementSystem.Infrastructure.Data.Context.Configuration.EmailSettings>(configuration.GetSection("EmailSettings"));

            // Register Custom IEmailSender (MailKit)
            // Using AddTransient because MailKit SmtpClient implements IDisposable and is lightweight to create.
            services.AddTransient<OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email.IEmailSender, MailKitEmailSender>();

            services.AddHostedService<LogCleanupService>();

            return services;
        }
    }
}