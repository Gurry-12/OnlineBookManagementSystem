using Microsoft.Extensions.DependencyInjection;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Books;

namespace OnlineBookManagementSystem.Shared.Extensions;

/// <summary>
/// Extension methods for registering unified services in DI container
/// Part of the Edge-Cut refactoring to eliminate service duplication
/// </summary>
public static class UnifiedServicesExtensions
{
    /// <summary>
    /// Register unified services that replace role-specific services
    /// </summary>
    public static IServiceCollection AddUnifiedServices(this IServiceCollection services)
    {
        // Register the unified book service that replaces:
        // - IBookQueryService
        // - IBookCommandService  
        // - IPublicBookService
        // - IAdminBookService
        services.AddScoped<IUnifiedBookService, UnifiedBookService>();
        
        // You can add more unified services here as you refactor other areas
        // services.AddScoped<IUnifiedOrderService, UnifiedOrderService>();
        // services.AddScoped<IUnifiedUserService, UnifiedUserService>();
        
        return services;
    }
    
    /// <summary>
    /// Remove old redundant services during migration
    /// Call this method to clean up old service registrations
    /// </summary>
    public static IServiceCollection RemoveRedundantServices(this IServiceCollection services)
    {
        // Remove old service registrations to prevent conflicts
        // This is optional and can be done gradually during migration
        
        // Example:
        // services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(IBookQueryService)));
        // services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(IBookCommandService)));
        
        return services;
    }
}