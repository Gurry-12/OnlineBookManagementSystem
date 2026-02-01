# Enhanced Controllers and Services Guide

## Overview
This document outlines the comprehensive controller and service architecture for the multi-role Online Book Management System, supporting SuperAdmin, Admin, and User roles with enterprise-level functionality.

## Controller Architecture

### 1. SuperAdminController
**Purpose**: System-wide management and oversight
**Authorization**: `SuperAdminOnly` policy

#### Key Actions:
- ✅ **Dashboard** - System overview with health metrics and statistics
- ✅ **ManageUsers** - Complete user management with role assignment
- ✅ **SystemSettings** - Site configuration and system settings
- ✅ **ActivityLogs** - Comprehensive audit trail with filtering and CSV Export
- ✅ **CreateUser** - API endpoint for user creation
- ✅ **UpdateSettings** - API endpoints for various settings updates
- ✅ **SystemMaintenance** - Cache clearing, database backup, etc.

#### Features:
- Real-time system health monitoring
- User role management with validation
- System configuration management
- Comprehensive activity logging with **CSV Export**
- Database backup and maintenance
- Email configuration testing

### 2. AdminController
**Purpose**: Business operations and content management
**Authorization**: `AdminOrHigher` policy

#### Key Actions:
- ✅ **Dashboard** - Business metrics and analytics
- ✅ **Books** - Book inventory management with pagination, filtering, and stock management
- ✅ **CreateBook/EditBook** - Book CRUD operations with image upload and stock control
- ✅ **UserList** - Customer account oversight
- ✅ **OrderManagement** - Order processing and status updates
- ✅ **CategoryManagement** - Category CRUD operations
- ✅ **ActivityLogs** - Business operation logs (filtered)
- ✅ **GetChartData** - API endpoint for dashboard analytics

#### Features:
- Advanced book management with image processing
- **Stock Management**: Track quantity, low stock thresholds.
- Order status tracking and updates
- User account oversight
- Business analytics and reporting

### 3. UserController
**Purpose**: Customer experience and account management
**Authorization**: `UserOrHigher` policy

#### Key Actions:
- ✅ **Dashboard** - Personalized user experience
- ✅ **UserBookList** - Book browsing with advanced filtering (Category, Price, Sort)
- ✅ **BookDetails** - Detailed book information with stock status
- ✅ **Favorite** - Wishlist management
- ✅ **OrderHistory** - Personal order tracking
- ✅ **Profile** - Account settings and preferences
- ✅ **SearchBooks** - Advanced search functionality
- ✅ **BooksByCategory** - Category-based browsing
- ✅ **AddToCart** - Shopping cart management (Stock validated)
- ✅ **GetRecommendations** - Personalized book suggestions

#### Features:
- Personalized book recommendations
- **Advanced search and filtering** (Price range, Category, Sort)
- **Stock Availability**: "Out of Stock" badges and validation.
- Wishlist/favorites management
- Order history and tracking

### 4. AuthController (Updated)
**Purpose**: Authentication and Account Recovery
**Authorization**: Anonymous / Authenticated

#### Key Actions:
- ✅ **Login/Logout** - JWT based auth
- ✅ **Registration** - New user signup (pending approval)
- ✅ **ForgotPassword** - Secure password reset request (Rate Limited: 5/hr)
- ✅ **ResetPassword** - Token based password reset
- ✅ **ConfirmEmail** - Account activation token verification
- ✅ **RefreshToken** - JWT refresh flow

## Service Architecture

### 1. SystemSettingsService
**Purpose**: System configuration and maintenance

#### Key Methods:
- ✅ `GetSystemSettingsAsync()` - Retrieve current system configuration
- ✅ `UpdateGeneralSettingsAsync()` - Update site-wide settings
- ✅ `UpdateSecuritySettingsAsync()` - Manage security policies
- ✅ `UpdateEmailSettingsAsync()` - Configure email services
- ✅ `TestEmailConfigurationAsync()` - Validate email setup
- ✅ `ClearCacheAsync()` - System cache management
- ✅ `BackupDatabaseAsync()` - Database backup operations

### 2. Enhanced UsersService
**Purpose**: User management and analytics

#### Key Methods:
- ✅ `GetSuperAdminDashboardDataAsync()` - System overview statistics
- ✅ `GetManageUsersDataAsync()` - Paginated user management
- ✅ `CreateUserAsync()` - User creation with role assignment
- ✅ `ApproveUserAsync()` - Approves user and sends **Email Confirmation Link**
- ✅ `RejectUserAsync()` - Rejects and soft-deletes user

### 3. Enhanced ActivityLogger
**Purpose**: Comprehensive audit logging

#### Key Methods:
- ✅ `LogAsync()` - Enhanced activity logging with context
- ✅ `GetActivityLogsAsync()` - Paginated and filtered log retrieval
- ✅ `GetRecentActivitiesAsync()` - Dashboard activity feeds
- ✅ `ClearOldLogsAsync()` - Log cleanup and maintenance

### 4. MailKitEmailSender (New)
**Purpose**: Production-grade email delivery
**Interface**: `OnlineBookManagementSystem.Interfaces.IEmailSender`

#### Features:
- **MailKit + MimeKit** integration (industry standard)
- Supports **HTML emails** with plain-text fallback
- **Async** execution for performance
- **Configuration** via `SystemSettingsService` (Database) with fallback to `appsettings.json`
- **Secure** (TLS/StartTLS support)

### 5. Enhanced BookService
**Purpose**: Book management and user experience

#### New Methods:
- ✅ `GetPaginatedBooksAsync()` - Supports filtering by Search, Category, Price Range, Stock, Sorting.
- ✅ `GetBookDetailsForUserAsync()` - Detailed book view with user context
- ✅ `UpdateBookAsync()` - Handles Stock Quantity and Low Stock Thresholds.

### 6. Enhanced CartService
**Purpose**: Shopping cart management

#### New Methods:
- ✅ `AddOrUpdateCartAsync()` - Validates stock availability before adding.
- ✅ `ProcessCheckoutAsync()` - Deducts stock and sends **Low Stock Warnings** to Admin.

## API Endpoints Structure

### SuperAdmin API Endpoints:
```
POST /SuperAdmin/CreateUser
POST /SuperAdmin/UpdateGeneralSettings
POST /SuperAdmin/UpdateSecuritySettings
POST /SuperAdmin/UpdateEmailSettings
POST /SuperAdmin/TestEmail
POST /SuperAdmin/ClearCache
POST /SuperAdmin/BackupDatabase
POST /SuperAdmin/ClearOldLogs
GET  /SuperAdmin/ExportActivityLogs (CSV)
```

### Admin API Endpoints:
```
GET  /Books/BookList (Advanced Filter)
POST /Books/Create
POST /Books/Edit/{id}
POST /Books/Delete/{id}
```

### User API Endpoints:
```
POST /User/ToggleFavorite
POST /User/AddToCart
GET  /User/GetCartCount
GET  /User/GetRecommendations
GET  /User/UserBookList (Advanced Filter)
```

### Auth API Endpoints:
```
POST /Auth/ForgotPassword
POST /Auth/ResetPassword
GET  /Auth/ConfirmEmail
```

## Security Implementation

### Authorization Policies:
- **SuperAdminOnly**: Exclusive SuperAdmin access
- **AdminOrHigher**: Admin and SuperAdmin access
- **UserOrHigher**: All authenticated users

### Security Features:
- **Account Activation**: Users must confirm email after approval.
- **Rate Limiting**: Password Reset protected by IP/Email rate limiting.
- **Stock Validation**: Prevents overselling.
- **Audit Logging**: All critical actions logged.

## File Structure
```
Controllers/
├── SuperAdminController.cs ✅ (Enhanced + Export)
├── AdminController.cs ✅ (Enhanced)
├── UserController.cs ✅ (Enhanced + Filters)
├── BooksController.cs ✅ (Enhanced + Stock/Filters)
├── AuthController.cs ✅ (Updated + Reset/Confirm)
├── ApiBaseController.cs ✅ (New)
└── BaseController.cs (Existing)

Services/
├── SystemSettingsService.cs ✅ (New)
├── UsersService.cs ✅ (Enhanced + Email)
├── ActivityLogger.cs ✅ (Enhanced)
├── BookServices.cs ✅ (Enhanced + Stock/Filters)
├── OrderService.cs (Needs enhancement)
├── CartService.cs ✅ (Enhanced + Stock)
├── CategoryServices.cs (Needs enhancement)
├── MailKitEmailSender.cs ✅ (New)
└── CacheService.cs ✅ (New)
```

This architecture provides a solid foundation for an enterprise-level book management system with proper separation of concerns, comprehensive security, and scalable design patterns.
