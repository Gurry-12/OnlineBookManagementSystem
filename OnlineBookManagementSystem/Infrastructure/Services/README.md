# Services Organization

This folder contains all service implementations organized by their responsibility and layer in the clean architecture.

## Folder Structure

### 📁 Domain/
Business logic services that implement core domain operations. These services contain the business rules and domain logic.

#### Books/
- `BookCommandService.cs` - Handles book creation, updates, and deletion
- `BookQueryService.cs` - Handles book queries and searches
- `BookServices.cs` - Legacy unified book service (to be deprecated)
- `BookValidationService.cs` - Book-specific validation logic

#### Categories/
- `CategoryServices.cs` - Category management operations

#### Cart/
- `CartService.cs` - Shopping cart operations

#### Orders/
- `OrderCommandService.cs` - Order creation and updates
- `OrderQueryService.cs` - Order queries and reporting
- `OrderService.cs` - Legacy unified order service (to be deprecated)

#### Users/
- `UserCommandService.cs` - User creation, updates, and management
- `UserQueryService.cs` - User queries and searches
- `UsersService.cs` - Legacy unified user service (to be deprecated)

#### Reviews/
- `ReviewService.cs` - Book review management

---

### 📁 Infrastructure/
Infrastructure concerns like authentication, email, caching, etc. These services handle technical infrastructure needs.

#### Authentication/
- `AuthService.cs` - JWT token generation and validation
- `UserAuthenticationService.cs` - User authentication operations

#### Email/
- `MailKitEmailSender.cs` - Email sending implementation using MailKit

#### Payment/
- `PaymentProcessingService.cs` - Payment processing logic

#### Caching/
- `CacheService.cs` - Distributed caching implementation

#### Logging/
- `ActivityLogger.cs` - User activity logging
- `ActivityLogCleanupService.cs` - Background service for log cleanup
- `LogCleanupService.cs` - General log cleanup service

---

### 📁 Helpers/
Utility helpers and cross-cutting concerns.

- `DNScheckerHelper.cs` - DNS validation helper
- `IDnsChecker.cs` - DNS checker interface
- `ErrorViewModelFactory.cs` - Error view model creation
- `MappingService.cs` - Object mapping utilities

---

### 📁 System/
System-level services that manage application-wide settings.

- `SystemSettingsService.cs` - Application settings management

---

## Service Lifetimes

### Scoped (Per-Request)
Most services use scoped lifetime because they:
- Work with database context
- Maintain per-request state
- Handle user-specific operations

Examples: BookQueryService, OrderCommandService, UserQueryService

### Singleton (Application-Wide)
Services that are:
- Stateless and thread-safe
- Rarely change
- Shared across the application

Examples: SystemSettingsService, CacheService

### Transient (Per-Use)
Services that are:
- Lightweight and stateless
- Created and disposed quickly

Examples: DNSCheckerHelper, ErrorViewModelFactory

---

## Migration Notes

### Legacy Services (To Be Deprecated)
- `BookServices.cs` → Split into BookCommandService, BookQueryService, BookValidationService
- `OrderService.cs` → Split into OrderCommandService, OrderQueryService
- `UsersService.cs` → Split into UserCommandService, UserQueryService

These legacy services are kept for backward compatibility but should be gradually replaced with the focused services following the Single Responsibility Principle.

---

## Namespace Convention

All services follow this namespace pattern:
```
OnlineBookManagementSystem.Infrastructure.Services.{Category}.{SubCategory}
```

Examples:
- `OnlineBookManagementSystem.Infrastructure.Services.Domain.Books`
- `OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Authentication`
- `OnlineBookManagementSystem.Infrastructure.Services.Helpers`
- `OnlineBookManagementSystem.Infrastructure.Services.System`
