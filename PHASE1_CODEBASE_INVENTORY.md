# PHASE 1: FULL CODEBASE AUDIT - LIVING INVENTORY

**Status**: IN PROGRESS  
**Date Started**: 2025-01-30  
**Purpose**: Complete understanding of every file, method, and responsibility before any refactoring

---

## 🎯 AUDIT RULES

- ✅ **READ ONLY** - No refactoring during audit
- ✅ **Document everything** - Every method, dependency, side effect
- ✅ **Mark redundancies** - But don't delete yet
- ✅ **Understand flows** - Who calls what, when, why

---

## 📁 PROJECT STRUCTURE

```
OnlineBookManagementSystem/
├── Core/
│   ├── Application/        # Application layer (55 files)
│   ├── Domain/             # Domain entities (22 files)
├── Infrastructure/
│   ├── Data/               # EF Core, Repositories (23 files)
│   └── Services/           # Domain & Infrastructure services (36 files)
├── Presentation/
│   ├── Controllers/        # MVC Controllers (28 files)
│   ├── ViewModels/         # View models (59 files)
│   ├── Views/              # Razor views (79 files)
│   └── Middleware/        # Custom middleware (3 files)
└── Shared/                 # Extensions, utilities (16 files)
```

---

## 🎮 CONTROLLERS INVENTORY

### BaseController.cs
**Purpose**: Base class for all controllers  
**Dependencies**: `ILogger<BaseController>`, `HttpContext`, `User.Claims`

**Methods**:
- `OnActionExecuting()` → Sets layout based on user role
- `DetermineLayout()` → Returns layout name based on primary role (SuperAdmin/Admin/User/Guest)
- `GetPrimaryRole()` → Extracts primary role from claims (priority: SuperAdmin > Admin > User > Guest)
- `SessionExpired()` → Clears session, shows expired message
- `GetCurrentRole()` → Gets role from claims
- `GetUserIdFromClaims()` → Extracts user ID from NameIdentifier claim
- `IsUserAuthorized(int expectedUserId)` → Validates user ID matches expected

**Side Effects**: 
- Modifies `ViewData["Layout"]`
- Accesses `HttpContext.Session`
- Reads `User.Claims`

**Callers**: All controllers inherit from this

**Status**: 🟢 Core - Layout determination logic

---

### PublicController.cs
**Purpose**: Public-facing controller for unauthenticated users  
**Dependencies**: 
- `IBookQueryService`
- `IBookAnalyticsService`
- `IPublicDemoService`
- `IRoleBasedRedirectionService`
- `IMemoryCache`

**Methods**:
- `Index()` → Returns view (no logic)
- `Dashboard()` → Landing page; redirects authenticated users to their dashboard
- `Showcase()` → Project showcase page; redirects authenticated users
- `TechnicalDetails()` → Technical architecture documentation
- `InteractiveDemo()` → Live system demo with read-only access
- `DeveloperStory()` → Developer narrative page
- `Browse()` → Browse books with filters (search, category, price, sort)
- `BookDetails(int id)` → Public book details view
- `SearchBooks(string query, int page)` → AJAX search endpoint (cached 5 min)
- `GetBooksByCategory(int categoryId, int page)` → AJAX category filter (cached 10 min)
- `GetSystemStatistics()` → AJAX system stats endpoint
- `GetFeaturedBooks(int count)` → AJAX featured books endpoint
- `SubmitCollaborationInquiry([FromBody] CollaborationInquiryViewModel)` → POST collaboration form (logs only, no DB)

**Side Effects**:
- Memory cache writes (search, category results)
- Activity logging (via service calls)
- HTTP redirects for authenticated users

**Callers**: 
- Public routes
- AJAX calls from frontend

**Status**: 🟢 Core - Public access layer

**Notes**:
- Heavy use of `IPublicDemoService` for read-only access
- Caching strategy: search (5 min), categories (10 min)
- All methods check `ShouldBypassPublicArea()` and redirect authenticated users

---

### AuthController.cs
**Purpose**: Authentication and user registration  
**Dependencies**:
- `IAuthService`
- `IActivityLogger`

**Methods**:
- `Login()` → GET login page with enhanced view model
- `LoginData([FromBody] LoginViewModel)` → POST login; validates user, generates tokens, sets cookie, returns redirect URL
- `RefreshToken([FromBody] RefreshTokenViewModel)` → POST refresh JWT token
- `Logout()` → POST logout; revokes refresh tokens, deletes cookie
- `Registration()` → GET registration page
- `SaveData([FromBody] RegisterViewModel)` → POST registration; creates pending user
- `ConfirmEmail(string token, string email)` → GET email confirmation
- `ForgotPassword()` → GET forgot password page
- `ForgotPassword([FromBody] ForgotPasswordViewModel)` → POST password reset request
- `ResetPassword(string token, string email)` → GET reset password page
- `ResetPassword([FromBody] ResetPasswordViewModel)` → POST password reset
- `ProfileView()` → GET user profile (requires auth)
- `UpdateProfile([FromBody] ProfileViewModel)` → POST profile update
- `TestAuth()` → GET debug endpoint to test authentication claims

**Helper Methods**:
- `SetAccessTokenCookie(string token)` → Sets HttpOnly, Secure, SameSite=Strict cookie
- `DeleteAccessTokenCookie()` → Removes access token cookie
- `GetRoleCapabilities()` → Returns role info for UI
- `GetOnboardingInfo()` → Returns onboarding steps
- `GetRoleDescriptions()` → Returns role descriptions
- `GetSystemStatsAsync()` → Returns system statistics (hardcoded values)
- `GetRecentFeatures()` → Returns feature list (hardcoded)

**Side Effects**:
- Cookie writes (accessToken)
- Activity logging (login, logout, profile updates)
- Database writes (user registration, password reset)
- Email sends (via service)

**Callers**:
- Public routes (login, register)
- Authenticated routes (profile, logout)

**Status**: 🟢 Core - Authentication layer

**Notes**:
- Uses JWT tokens stored in cookies
- Registration creates pending users (requires approval)
- `GetSystemStatsAsync()` returns hardcoded values (🔴 Suspicious - should come from service)

---

### BooksController.cs
**Purpose**: Book management (mixed public/admin/user access)  
**Dependencies**:
- `IBookQueryService`
- `IBookCommandService`
- `IBookAnalyticsService`
- `IBookFavoriteService`
- `IActivityLogger`
- `IReviewService`

**Methods**:
- `GetAdminData()` → GET admin book list (JSON, AdminOrHigher)
- `UserIndex()` → GET user book browsing page
- `GetBooks(string? search, int? categoryId)` → GET user books (JSON, UserOrHigher)
- `PublicList(string? search, int? categoryId)` → GET public book list (AllowAnonymous)
- `Create()` → GET create book form (AdminOrHigher)
- `Create(BookFormViewModel)` → POST create book (AdminOrHigher)
- `Edit(int id)` → GET edit book form (AdminOrHigher)
- `Edit(int id, BookFormViewModel)` → POST update book (AdminOrHigher)
- `Delete(int id)` → POST soft delete book (AdminOrHigher)
- `ToggleFavorite(int bookId)` → POST toggle favorite (UserOrHigher)
- `Details(int id)` → GET book details with reviews (AllowAnonymous)
- `BookList(...)` → GET admin book list with filters (AdminOrHigher)
- `GetMonthlyBookUploads()` → GET chart data (AdminOrHigher)
- `GetBooksByCategory()` → GET chart data (AdminOrHigher)
- `GetBooksByAuthor()` → GET chart data (AdminOrHigher)
- `GetFavoriteBookStats()` → GET chart data (AdminOrHigher)

**Side Effects**:
- Database writes (create, update, delete books)
- File system writes (book images)
- Activity logging
- Cache invalidations (via services)

**Callers**:
- Admin routes
- User routes
- Public routes (details, list)

**Status**: 🟡 Suspicious - Mixed responsibilities (public/admin/user), should be split

**Notes**:
- Handles public, user, and admin book operations in one controller
- Chart endpoints return JSON for dashboard widgets
- `Details()` shows different content based on authentication

---

### AdminController.cs
**Purpose**: Admin dashboard and management  
**Dependencies**:
- `IBookQueryService`
- `IBookCommandService`
- `IBookAnalyticsService`
- `ICartService`
- `IOrderQueryService`
- `IOrderCommandService`
- `IActivityLogger`
- `IUsersService`
- `ICategoryInterface`
- `ILogger<AdminController>`

**Methods**:
- `Dashboard()` → GET admin dashboard (AdminOrHigher)
- `ActivityLogs(...)` → GET activity logs with filters (AdminOrHigher)
- `Books(...)` → GET book management page (AdminOrHigher)
- `CreateBook()` → GET create book form (AdminOrHigher)
- `CreateBook(BookFormViewModel, IFormFile?)` → POST create book (AdminOrHigher)
- `EditBook(int id)` → GET edit book form (AdminOrHigher)
- `EditBook(int id, BookFormViewModel, IFormFile?)` → POST update book (AdminOrHigher)
- `DeleteBook(int id)` → POST delete book (AdminOrHigher)
- `UserList(...)` → GET user management page (AdminOrHigher)
- `OrderManagement(...)` → GET order management page (AdminOrHigher)
- `UpdateOrderStatus(int orderId, string status)` → POST update order status (AdminOrHigher)
- `CategoryManagement()` → GET category management page (AdminOrHigher)
- `CreateCategory([FromBody] CreateCategoryRequest)` → POST create category (AdminOrHigher)
- `UpdateCategory([FromBody] UpdateCategoryRequest)` → POST update category (AdminOrHigher)
- `DeleteCategory(int id)` → POST delete category (AdminOrHigher)
- `Details(int id)` → GET book details (AdminOrHigher)
- `GetChartData(string chartType)` → GET chart data for dashboard (AdminOrHigher)

**Helper Methods**:
- `GetAdminDashboardDataAsync(int userId)` → Aggregates dashboard stats
- `HandleUnauthorized()` → Returns JSON or redirect based on request type
- `ValidateBookModel(BookFormViewModel?)` → Validates book model
- `LoadCategoriesForModel(BookFormViewModel)` → Loads categories for dropdown
- `HandleSuccess(string message, string redirectAction)` → Returns JSON or redirect
- `HandleError(string message, BookFormViewModel?)` → Returns error response
- `IsAjaxRequest()` → Checks for AJAX request header

**Side Effects**:
- Database writes (books, categories, orders)
- File system writes (book images)
- Activity logging
- Cache operations

**Callers**:
- Admin routes only

**Status**: 🟢 Core - Admin functionality

**Notes**:
- Supports both AJAX and traditional form submissions
- Comprehensive error handling with JSON/redirect fallback
- Dashboard aggregates multiple data sources

---

### SuperAdminController.cs
**Purpose**: SuperAdmin system management  
**Dependencies**:
- `IUsersService`
- `IOrderQueryService`
- `IActivityLogger`
- `ISystemSettingsService`
- `UserManager<User>`
- `RoleManager<IdentityRole<int>>`
- `ILogger<SuperAdminController>`

**Methods**:
- `Dashboard()` → GET super admin dashboard (SuperAdminOnly)
- `ManageUsers(...)` → GET user management page (SuperAdminOnly)
- `PendingUsers()` → GET pending user approvals (SuperAdminOnly)
- `ApproveUser(int userId, string role)` → POST approve pending user (SuperAdminOnly)
- `RejectUser(int userId)` → POST reject pending user (SuperAdminOnly)
- `SystemSettings()` → GET system settings page (SuperAdminOnly)
- `ActivityLogs(...)` → GET activity logs (SuperAdminOnly)
- `ExportActivityLogs(...)` → GET export logs as CSV (SuperAdminOnly)
- `CreateUser([FromForm] CreateUserRequest)` → POST create user (SuperAdminOnly)
- `UpdateGeneralSettings([FromForm] GeneralSettingsRequest)` → POST update general settings (SuperAdminOnly)
- `UpdateSecuritySettings([FromForm] SecuritySettingsRequest)` → POST update security settings (SuperAdminOnly)
- `UpdateEmailSettings([FromForm] EmailSettingsRequest)` → POST update email settings (SuperAdminOnly)
- `TestEmail()` → POST test email configuration (SuperAdminOnly)
- `ClearCache()` → POST clear system cache (SuperAdminOnly)
- `BackupDatabase()` → POST backup database (SuperAdminOnly)
- `ClearOldLogs()` → POST clear old activity logs (SuperAdminOnly)
- `ExecuteQuickAction([FromBody] QuickActionRequest)` → POST unified quick action endpoint (SuperAdminOnly)
- `GetSystemOverview()` → GET system overview JSON (SuperAdminOnly)
- `SwitchToRole(string role)` → GET switch view role (SuperAdminOnly)
- `ReturnToSuperAdmin()` → GET return to super admin view (SuperAdminOnly)
- `PromoteUser(int userId, string newRole)` → POST promote user role (SuperAdminOnly)
- `DemoteUser(int userId, string newRole)` → POST demote user role (SuperAdminOnly)
- `ChangeUserRole(int userId, string newRole)` → POST change user role (SuperAdminOnly)
- `ToggleUserStatus(int userId)` → POST toggle user active/inactive (SuperAdminOnly)

**Helper Methods**:
- `EscapeCsv(string field)` → Escapes CSV fields, prevents formula injection
- `GetUserIdFromClaims()` → Extracts user ID from claims
- `ExecuteClearCacheAsync()` → Clears cache
- `ExecuteClearOldLogsAsync(int days)` → Clears old logs
- `ConvertToServiceResult((bool, string))` → Converts tuple to ServiceResult

**Side Effects**:
- Database writes (users, roles, settings)
- File system writes (CSV exports, backups)
- Cache clears
- Email sends (test)
- Activity logging

**Callers**:
- SuperAdmin routes only

**Status**: 🟢 Core - SuperAdmin functionality

**Notes**:
- Role management with security checks (can't demote self, can't create SuperAdmin unless SuperAdmin)
- CSV export with injection prevention
- Unified quick action endpoint for common operations
- Role switching for testing (stored in session)

---

### UserController.cs
**Purpose**: User-facing book browsing and profile management  
**Dependencies**:
- `IBookQueryService`
- `IBookFavoriteService`
- `IBookCommandService` (🔴 Suspicious - user shouldn't use command service directly)
- `IUserCommandService`
- `ICartService`
- `IOrderQueryService`
- `IOrderCommandService`
- `IActivityLogger`
- `ICategoryInterface`
- `IAuthService`

**Methods**:
- `Dashboard()` → GET user dashboard (UserOrHigher)
- `UserBookList(...)` → GET user book browsing page (UserOrHigher)
- `BookDetails(int id)` → GET book details for user (UserOrHigher)
- `Details(int id)` → Alias for BookDetails (UserOrHigher)
- `Favorite()` → GET favorite books list (UserOrHigher)
- `ToggleFavorite([FromBody] ToggleFavoriteRequest)` → POST toggle favorite (UserOrHigher)
- `OrderHistory(...)` → GET user order history (UserOrHigher)
- `OrderDetails(int id)` → GET order details (UserOrHigher)
- `Profile()` → GET user profile (UserOrHigher)
- `UpdateProfile(UserProfileViewModel)` → POST update profile (UserOrHigher) - 🔴 Uses BookCommandService
- `AddToCart([FromBody] AddToCartRequest)` → POST add to cart (UserOrHigher)
- `GetCartCount()` → GET cart item count (UserOrHigher)
- `SearchBooks(string query, int page)` → GET search books (UserOrHigher)
- `BooksByCategory(int categoryId, int page)` → GET books by category (UserOrHigher)
- `GetRecommendations()` → GET personalized recommendations (UserOrHigher)
- `GetNewArrivals()` → GET new arrivals (UserOrHigher)
- `UserCart()` → GET user cart view (UserOrHigher)
- `CancelOrder([FromBody] CancelOrderRequest)` → POST cancel order (UserOrHigher)
- `ChangePassword([FromBody] ChangePasswordRequest)` → POST change password (UserOrHigher)

**Helper Methods**:
- `GetUserDashboardDataAsync(int userId)` → Aggregates dashboard data

**Side Effects**:
- Database reads/writes (favorites, cart, orders, profile)
- Activity logging
- Cache reads (recommendations)

**Callers**:
- User routes only

**Status**: 🟡 Suspicious - Uses BookCommandService for profile update (layer violation)

**Notes**:
- Line 227: `UpdateProfile()` calls `_bookCommandService.UpdateUserProfileAsync()` - 🔴 This is wrong, should use `IUserCommandService`
- Comprehensive user-facing features (browsing, favorites, cart, orders)

---

### CartController.cs
**Purpose**: Shopping cart management  
**Dependencies**:
- `ICartService`

**Methods**:
- `AddOrUpdateCart([FromBody] CartItemRequestViewModel)` → POST add/update cart item (UserOrHigher)
- `UpdateQuantity([FromBody] CartItemRequestViewModel)` → PUT update cart quantity (UserOrHigher)
- `RemoveItem(int bookId)` → DELETE remove cart item (UserOrHigher)
- `AdminCarts()` → GET all carts view (AdminOrHigher)
- `Checkout()` → GET checkout page (UserOrHigher)
- `ProcessCheckout(CheckOutRequestViewModel)` → POST process checkout (UserOrHigher)
- `OrderConfirmation(int? orderId)` → GET order confirmation page

**Side Effects**:
- Database writes (cart items, orders)
- Activity logging (via service)

**Callers**:
- User routes (cart operations)
- Admin routes (view all carts)

**Status**: 🟢 Core - Cart functionality

**Notes**:
- Simple controller, delegates to `ICartService`
- Checkout creates order and clears cart

---

### OrderController.cs
**Purpose**: Order viewing and management  
**Dependencies**:
- `BookManagementContext` (🔴 Suspicious - direct DbContext access)
- `IActivityLogger`

**Methods**:
- `AdminIndex(...)` → GET admin order list (AdminOrHigher) - 🔴 Direct EF queries
- `AdminDetails(int id)` → GET admin order details (AdminOrHigher) - 🔴 Direct EF queries
- `UpdateStatus(int id, string status)` → POST update order status (AdminOrHigher) - 🔴 Direct EF writes
- `Index()` → GET user order list (UserOrHigher) - 🔴 Direct EF queries
- `Details(int id)` → GET user order details (UserOrHigher) - 🔴 Direct EF queries

**Side Effects**:
- Database reads/writes (direct EF Core)
- Activity logging

**Callers**:
- Admin routes
- User routes

**Status**: 🔴 Dead/Redundant - Should use `IOrderQueryService` and `IOrderCommandService` instead of direct DbContext

**Notes**:
- **MAJOR VIOLATION**: Direct `BookManagementContext` access in controller
- Should delegate to `IOrderQueryService` and `IOrderCommandService`
- Duplicates functionality that exists in services

---

### ReviewController.cs
**Purpose**: Book review management  
**Dependencies**:
- `IReviewService`
- `ILogger<ReviewController>`

**Methods**:
- `Submit(ReviewSubmissionViewModel)` → POST submit review (UserOrHigher)
- `Update(int id, ReviewSubmissionViewModel)` → POST update review (UserOrHigher)
- `Delete(int id, int bookId)` → POST delete review (UserOrHigher)
- `GetBookReviews(int bookId, ...)` → GET paginated reviews (public)
- `GetBookRating(int bookId)` → GET book rating summary (public)
- `GetUserReview(int bookId)` → GET current user's review for book (UserOrHigher)

**Side Effects**:
- Database writes (reviews)
- Activity logging (via service)

**Callers**:
- Public routes (view reviews)
- User routes (submit/update/delete)

**Status**: 🟢 Core - Review functionality

**Notes**:
- Clean controller, delegates to `IReviewService`
- Supports pagination, sorting, filtering

---

### CategoryController.cs
**Purpose**: Category management  
**Dependencies**:
- `BookManagementContext` (🔴 Suspicious - direct DbContext access)
- `ICategoryInterface`

**Methods**:
- `DisplayCategory()` → GET category list (AdminOrHigher)
- `CategoryClassify()` → GET category classification (AllowAnonymous)
- `CreateCategory([FromBody] Category)` → POST create category (AdminOrHigher)
- `DeleteCategory(int Id)` → DELETE delete category (AdminOrHigher)
- `GetCategoryById(int Id)` → GET category by ID (AdminOrHigher)
- `UpdateCategory([FromBody] Category)` → POST update category (AdminOrHigher)

**Side Effects**:
- Database reads/writes (categories)

**Callers**:
- Admin routes
- Public routes (classification)

**Status**: 🟡 Suspicious - Mixed use of `ICategoryInterface` and direct `BookManagementContext` (not seen in code but context is injected)

**Notes**:
- Uses `ICategoryInterface` for operations (good)
- But also injects `BookManagementContext` (why?)

---

## 🔍 REDUNDANCY & SMELL MARKING

### 🔴 Dead / Redundant

1. **OrderController.cs** - Direct `BookManagementContext` access
   - Should use `IOrderQueryService` and `IOrderCommandService`
   - Duplicates service layer functionality

2. **AuthController.GetSystemStatsAsync()** - Returns hardcoded values
   - Should query actual services for real statistics

### 🟡 Suspicious

1. **BooksController.cs** - Mixed responsibilities
   - Handles public, user, and admin operations
   - Should be split into separate controllers

2. **UserController.UpdateProfile()** - Layer violation
   - Uses `IBookCommandService` instead of `IUserCommandService`
   - Line 227: `_bookCommandService.UpdateUserProfileAsync()`

3. **CategoryController** - Unnecessary dependency
   - Injects `BookManagementContext` but uses `ICategoryInterface`
   - Why is context injected if not used?

### 🟢 Core (Business Critical)

- BaseController
- PublicController
- AuthController
- AdminController
- SuperAdminController
- CartController
- ReviewController

---

## 📊 CONTROLLER SUMMARY

| Controller | Methods | Role Access | Status | Notes |
|------------|---------|-------------|--------|-------|
| BaseController | 7 | All | 🟢 Core | Layout determination |
| PublicController | 13 | Public | 🟢 Core | Public access layer |
| AuthController | 15 | Public/Auth | 🟢 Core | Authentication |
| BooksController | 15 | Mixed | 🟡 Suspicious | Should split |
| AdminController | 17 | Admin | 🟢 Core | Admin management |
| SuperAdminController | 20 | SuperAdmin | 🟢 Core | System management |
| UserController | 18 | User | 🟡 Suspicious | Layer violation |
| CartController | 7 | User/Admin | 🟢 Core | Cart operations |
| OrderController | 5 | User/Admin | 🔴 Dead | Direct EF access |
| ReviewController | 6 | Public/User | 🟢 Core | Review management |
| CategoryController | 6 | Public/Admin | 🟡 Suspicious | Unnecessary dependency |

**Total Controllers**: 11  
**Total Methods**: ~129

---

---

## 📦 APPLICATION SERVICES LAYER INVENTORY

### Domain Services - Books

#### IBookQueryService
**Purpose**: Book read operations and queries  
**Methods**: 20
- `GetAllBooksAsync()` → Returns all books
- `GetBookByIdAsync(int id)` → Returns book by ID
- `GetTotalBooksCountAsync()` → Returns total book count
- `GetPaginatedBooksAsync(...)` → Returns paginated, filtered books
- `GetBooksForUserAsync(...)` → Returns books for user with personalization
- `SearchBooksAsync(...)` → Search books by query
- `GetBooksByCategoryAsync(...)` → Get books by category
- `GetBookDetailsForUserAsync(...)` → Get book details for user
- `GetPersonalizedRecommendationsAsync(...)` → Get recommendations
- `GetFeaturedBooksAsync(...)` → Get featured books
- `GetNewArrivalsAsync(...)` → Get new arrivals
- `GetCreateBookViewModelAsync()` → Get view model for create form
- `GetEditBookViewModelAsync(int id)` → Get view model for edit form
- `GetCategoriesAsync()` → Get categories as SelectListItem
- `GetUserProfileAsync(int userId)` → Get user profile (🔴 Suspicious - why in BookQueryService?)
- `GetFavoriteBooksAsync(int userId)` → Get favorite books (for backward compatibility)
- `GetBookSuggestionsAsync(...)` → Get book suggestions

**Status**: 🟡 Suspicious - Contains user profile method (should be in UserQueryService)

---

#### IBookCommandService
**Purpose**: Book write operations and commands  
**Methods**: 5
- `AddBookAsync(Book, IFormFile?)` → Create new book with optional image
- `UpdateBookAsync(Book, IFormFile?)` → Update existing book
- `SoftDeleteBookAsync(int id, int userId)` → Soft delete book
- `SaveImageAsync(IFormFile, string)` → Save book image
- `UpdateUserProfileAsync(int userId, UserProfileViewModel)` → 🔴 **MAJOR VIOLATION** - User profile update in BookCommandService

**Status**: 🔴 Dead/Redundant - Contains `UpdateUserProfileAsync()` which violates SRP

**Notes**:
- Line 21: `UpdateUserProfileAsync()` should be in `IUserCommandService`
- This is called from `UserController.UpdateProfile()` (line 227)

---

#### IBookFavoriteService
**Purpose**: Book favorites management  
**Methods**: 6
- `ToggleFavoriteAsync(int userId, int bookId)` → Toggle favorite status
- `ToggleUserFavoriteAsync(int bookId, int userId)` → Toggle with result object
- `GetUserFavoriteBooksAsync(int userId)` → Get user's favorite books
- `GetUserFavoritesCountAsync(int userId)` → Get count of favorites
- `IsFavoriteAsync(int userId, int bookId)` → Check if book is favorite
- `RemoveFavoriteAsync(int userId, int bookId)` → Remove favorite

**Status**: 🟢 Core - Favorites functionality

---

#### IUnifiedBookService
**Purpose**: Unified book service for all roles (CQRS alternative?)  
**Methods**: 10
- `GetBookDetailsAsync(...)` → Role-aware book details
- `GetBooksAsync(...)` → Role-aware book list
- `CanUserEditBookAsync(...)` → Permission check
- `CanUserDeleteBookAsync(...)` → Permission check
- `CanUserReviewBookAsync(...)` → Permission check
- `CreateBookAsync(...)` → Create book with role check
- `UpdateBookAsync(...)` → Update book with role check
- `DeleteBookAsync(...)` → Delete book with role check
- `ToggleFavoriteAsync(...)` → Toggle favorite
- `GetBookAnalyticsAsync(...)` → Get analytics

**Status**: 🟡 Suspicious - Appears to be an alternative to CQRS pattern, but not fully implemented

**Notes**:
- Not used in controllers (controllers use IBookQueryService/IBookCommandService)
- May be intended for future refactoring

---

### Domain Services - Orders

#### IOrderQueryService
**Purpose**: Order read operations  
**Methods**: 12
- `GetTotalOrders()` / `GetTotalOrdersCountAsync()` → Total order count
- `GetUserOrdersCountAsync(int userId)` → User order count
- `GetUserTotalSpentAsync(int userId)` → User total spent
- `GetUserRecentOrdersAsync(...)` → Recent orders for user
- `GetUserOrderHistoryAsync(...)` → Paginated order history
- `GetUserOrderDetailsAsync(...)` → Order details for user
- `GetOrdersForAdminAsync(...)` → Admin order list
- `GetMonthlyRevenueAsync()` → Revenue analytics
- `GetOrderStatusDistributionAsync()` → Status distribution
- `GetTotalRevenueAsync()` → Total revenue
- `GetRecentOrdersAsync(...)` → Recent orders (all users)

**Status**: 🟢 Core - Order queries

---

#### IOrderCommandService
**Purpose**: Order write operations  
**Methods**: 5
- `UpdateOrderStatusAsync(int, OrderStatus, int)` → Update order status
- `CancelOrderAsync(int, int)` → Cancel order
- `CreateOrderAsync(CreateOrderRequest)` → Create new order
- `ProcessOrderAsync(int, int)` → Process order
- `CompleteOrderAsync(int, int)` → Complete order

**Status**: 🟢 Core - Order commands

**Notes**:
- `OrderController` should use this instead of direct DbContext access

---

#### ICartService
**Purpose**: Shopping cart management  
**Methods**: 10
- `GetUserCartAsync(int userId)` → Get user cart
- `GetCartSummaryAsync(int userId)` → Get cart summary
- `AddOrUpdateCartAsync(...)` → Add/update cart item
- `UpdateCartQuantityAsync(...)` → Update quantity
- `RemoveCartItemAsync(...)` → Remove item
- `GetAllCartsAsync(int? adminUserId)` → Get all carts (admin)
- `CheckoutDetailsAsync(int userId)` → Get checkout details
- `ProcessCheckoutAsync(...)` → Process checkout (creates order)
- `DeductInventoryAsync(int orderId)` → Deduct inventory after order
- `GetCartItemCountAsync(int userId)` → Get cart count
- `AddToCartAsync(...)` → Add to cart with result

**Status**: 🟢 Core - Cart functionality

---

### Domain Services - Users

#### IUsersService
**Purpose**: User management (composite service?)  
**Methods**: 12
- `GetTotalUsers()` / `GetTotalUsersCountAsync()` → User count
- `GetSuperAdminDashboardDataAsync()` → Dashboard data
- `GetManageUsersDataAsync(...)` → Manage users data
- `GetUsersForAdminAsync(...)` → Users for admin
- `CreateUserAsync(...)` → Create user
- `UpdateUserRoleAsync(...)` → Update role (2 overloads)
- `ToggleUserStatusAsync(...)` → Toggle active/inactive
- `GetPendingUsersAsync()` → Get pending approvals
- `ApproveUserAsync(...)` → Approve user
- `RejectUserAsync(...)` → Reject user
- `SoftDeleteUserAsync(int)` → Soft delete user
- `GetUserDetailsAsync(int)` → Get user details
- `GetUserStatisticsAsync(int)` → Get user statistics

**Status**: 🟡 Suspicious - Appears to be a composite/facade service combining IUserQueryService and IUserCommandService

**Notes**:
- May violate SRP by combining query and command operations
- Should potentially be split or used as a facade

---

#### IUserQueryService
**Purpose**: User read operations  
**Methods**: 6
- `GetTotalUsers()` / `GetTotalUsersCountAsync()` → User count
- `GetSuperAdminDashboardDataAsync()` → Dashboard data
- `GetManageUsersDataAsync(...)` → Manage users data
- `GetUsersForAdminAsync(...)` → Users for admin
- `GetActiveUsersCountAsync()` → Active user count
- `GetPendingUsersAsync()` → Pending users

**Status**: 🟢 Core - User queries

---

#### IUserCommandService
**Purpose**: User write operations  
**Methods**: 6
- `CreateUserAsync(...)` → Create user
- `UpdateUserRoleAsync(int, string)` → Update role
- `ToggleUserStatusAsync(...)` → Toggle status
- `SoftDeleteUserAsync(int)` → Soft delete
- `ApproveUserAsync(...)` → Approve user (delegated from IUserApprovalService)
- `RejectUserAsync(...)` → Reject user (delegated from IUserApprovalService)

**Status**: 🟢 Core - User commands

**Notes**:
- Contains approval methods (should these be in IUserApprovalService?)

---

### Domain Services - Categories

#### ICategoryInterface
**Purpose**: Category management  
**Methods**: 13
- `GetAllCategories()` → Get all (sync)
- `GetCategoryById(int)` → Get by ID (sync)
- `AddCategory(Category)` → Add category (sync)
- `UpdateCategory(Category)` → Update (async)
- `DeleteCategory(int)` → Delete (async)
- `GetAllCategoriesClassified()` → Get classified categories
- `GetCategoriesForDropdownAsync()` → Get for dropdown
- `GetAllCategoriesAsync()` → Get all (async)
- `GetTotalCategoriesCountAsync()` → Get count
- `GetCategoriesWithCountAsync()` → Get with book counts
- `GetCategoryByIdAsync(int)` → Get by ID (async)
- `CreateCategoryAsync(...)` → Create with user ID
- `UpdateCategoryAsync(...)` → Update with user ID
- `DeleteCategoryAsync(...)` → Delete with user ID

**Status**: 🟡 Suspicious - Mixed sync/async methods, inconsistent naming

**Notes**:
- Has both sync and async versions of same methods
- Inconsistent parameter patterns (some take Category entity, some take primitives)

---

### Domain Services - Reviews

#### IReviewService
**Purpose**: Review management  
**Methods**: 12
- `SubmitReviewAsync(...)` → Submit review
- `UpdateReviewAsync(...)` → Update review
- `DeleteReviewAsync(...)` → Delete review
- `GetUserReviewForBookAsync(...)` → Get user's review
- `GetBookReviewsAsync(...)` → Get paginated reviews
- `GetReviewByIdAsync(...)` → Get review by ID
- `GetBookRatingAsync(...)` → Get rating summary
- `RecalculateBookRatingAsync(...)` → Recalculate rating
- `InvalidateRatingCacheAsync(...)` → Invalidate cache
- `GetPendingReviewsAsync(...)` → Get pending (moderation)
- `ApproveReviewAsync(...)` → Approve review
- `RejectReviewAsync(...)` → Reject review
- `FlagReviewAsync(...)` → Flag review
- `GetReviewAnalyticsAsync()` → Get analytics
- `GetTopRatedBooksAsync(...)` → Top rated
- `GetLowestRatedBooksAsync(...)` → Lowest rated

**Status**: 🟢 Core - Review functionality

---

### Domain Services - Showcase

#### IPublicDemoService
**Purpose**: Read-only public demo data  
**Methods**: 8
- `GetFeaturedBooksAsync(...)` → Featured books
- `GetCategoriesWithCountsAsync()` → Categories with counts
- `SearchBooksAsync(...)` → Search books
- `GetBookDetailsAsync(int)` → Book details
- `GetSystemStatisticsAsync()` → System stats
- `GetBooksByCategoryAsync(...)` → Books by category
- `GetShowcaseContentAsync()` → Showcase content
- `GetTechnicalHighlightsAsync(...)` → Technical highlights
- `GetFeatureShowcasesAsync(...)` → Feature showcases
- `GetPerformanceMetricsAsync()` → Performance metrics

**Status**: 🟢 Core - Public demo functionality

---

### Infrastructure Services - Authentication

#### IAuthService
**Purpose**: Authentication and user management  
**Methods**: 18
- `SeedRolesAsync()` → Seed roles
- `ValidateUserAsync(...)` → Validate login
- `GenerateTokensAsync(...)` → Generate JWT tokens
- `RegisterUserAsync(...)` → Register user
- `ConfirmEmailAsync(...)` → Confirm email
- `UpdatePasswordAsync(...)` → Update password
- `GeneratePasswordResetTokenAsync(...)` → Generate reset token
- `GetUserProfileAsync(...)` → Get profile
- `GetUserById(int)` → Get user (sync)
- `UpdateUserDetailAsync(...)` → Update user (2 overloads - sync/async)
- `AssignRoleAsync(...)` → Assign role
- `GetUserRolesAsync(...)` → Get roles
- `RevokeRefreshTokensAsync(...)` → Revoke tokens
- `RefreshTokenAsync(...)` → Refresh token
- `ManageUsers()` → Manage users (legacy?)
- `SendWelcomeEmailAsync(...)` → Send welcome email
- `SendUserApprovedEmailAsync(...)` → Send approval email
- `ChangePasswordAsync(...)` → Change password
- `UpdateEmailAsync(...)` → Update email
- `ValidatePasswordAsync(...)` → Validate password

**Status**: 🟡 Suspicious - Large interface, mixes concerns (auth + user management)

**Notes**:
- Contains both authentication and user management methods
- Has sync and async versions of some methods

---

#### IRoleBasedRedirectionService
**Purpose**: Role-based redirection logic  
**Methods**: 6
- `GetRedirectUrlForUserAsync(int)` → Get redirect for user
- `GetDefaultRedirectForRoleAsync(string)` → Get default for role
- `ShouldBypassPublicArea(ClaimsPrincipal)` → Check if should bypass
- `GetHighestPriorityRole(ClaimsPrincipal)` → Get highest role
- `GetRedirectUrlForClaimsAsync(ClaimsPrincipal)` → Get redirect from claims
- `IsValidRedirectUrl(string, ClaimsPrincipal)` → Validate redirect URL

**Status**: 🟢 Core - Redirection logic

---

### Infrastructure Services - System

#### ISystemSettingsService
**Purpose**: System settings management  
**Methods**: 7
- `GetSystemSettingsAsync()` → Get settings
- `GetEmailSettingsAsync()` → Get email settings
- `UpdateGeneralSettingsAsync(...)` → Update general
- `UpdateSecuritySettingsAsync(...)` → Update security
- `UpdateEmailSettingsAsync(...)` → Update email
- `TestEmailConfigurationAsync()` → Test email
- `ClearCacheAsync()` → Clear cache
- `BackupDatabaseAsync()` → Backup database

**Status**: 🟢 Core - System settings

---

---

## 🔧 SERVICE IMPLEMENTATIONS INVENTORY

### Domain Services - Books

#### BookQueryService
**Purpose**: Book read operations implementation  
**Dependencies**: `BookManagementContext`, `IMemoryCache`, `ILogger<BookQueryService>`

**Key Methods**:
- `GetAllBooksAsync()` → Direct EF query, includes Category
- `GetBookByIdAsync(int id)` → Direct EF query
- `GetPaginatedBooksAsync(...)` → Complex query with filters, sorting, pagination
- `GetBooksForUserAsync(...)` → Similar to GetPaginatedBooksAsync but sets favorite status
- `GetBookDetailsForUserAsync(...)` → Gets book with favorite check and review check
- `GetPersonalizedRecommendationsAsync(...)` → Based on user's favorite categories
- `GetFeaturedBooksAsync(...)` → Books where IsFeatured == true
- `GetNewArrivalsAsync(...)` → Ordered by CreatedAt descending
- `GetUserProfileAsync(int userId)` → 🔴 **VIOLATION** - User profile in BookQueryService
- `GetMonthlyBookUploadsAsync()` → Analytics query
- `GetBooksByCategoryAsync()` → Analytics query
- `GetBooksByAuthorAsync()` → Analytics query
- `GetFavoriteStatsAsync()` → Analytics query

**Side Effects**:
- Database reads (EF Core queries)
- Cache reads/writes (GetQuickStats uses cache)
- No writes

**Status**: 🟡 Suspicious - Contains `GetUserProfileAsync()` which should be in UserQueryService

**Notes**:
- Line 440-462: `GetUserProfileAsync()` queries Users, Orders, UserFavorites - wrong service
- Uses direct EF Core queries (no repository pattern)
- Some methods have caching, others don't (inconsistent)

---

#### BookCommandService
**Purpose**: Book write operations implementation  
**Dependencies**: `BookManagementContext`, `IWebHostEnvironment`, `ILogger`, `IActivityLogger`

**Key Methods**:
- `AddBookAsync(Book, IFormFile?)` → Creates book, saves image, uses transaction
- `UpdateBookAsync(Book, IFormFile?)` → Updates book, handles image replacement, uses transaction
- `SoftDeleteBookAsync(int, int)` → Soft deletes book
- `SaveImageAsync(IFormFile, string)` → Validates, resizes, saves image (400x600, JPEG 85%)
- `ToggleFavoriteAsync(int, int)` → 🔴 **VIOLATION** - Favorite logic in BookCommandService
- `ToggleUserFavoriteAsync(int, int)` → 🔴 **VIOLATION** - Duplicate favorite logic
- `UpdateUserProfileAsync(int, UserProfileViewModel)` → 🔴 **MAJOR VIOLATION** - User profile update

**Side Effects**:
- Database writes (transactions)
- File system writes (images)
- Activity logging
- Image cleanup on rollback

**Status**: 🔴 Dead/Redundant - Contains favorite and user profile methods (should be in separate services)

**Notes**:
- Line 256-268: `ToggleFavoriteAsync()` - should be in BookFavoriteService
- Line 270-308: `ToggleUserFavoriteAsync()` - duplicate of BookFavoriteService logic
- Line 310-329: `UpdateUserProfileAsync()` - should be in UserCommandService
- Good transaction handling with rollback and cleanup
- Image processing with validation and resizing

---

#### BookFavoriteService
**Purpose**: Book favorites management implementation  
**Dependencies**: `BookManagementContext`, `IActivityLogger`, `ILogger<BookFavoriteService>`

**Key Methods**:
- `GetUserFavoritesAsync(int userId)` → Returns BookDto list
- `ToggleFavoriteAsync(int userId, int bookId)` → Toggles UserFavorite entity
- `IsFavoriteAsync(int userId, int bookId)` → Checks favorite status
- `GetFavoriteCountAsync(int bookId)` → Gets count for a book
- `GetTopFavoritedBooksAsync(int count)` → Top favorited books
- `GetUserFavoriteBooksAsync(int userId)` → Returns Book entities (backward compatibility)
- `ToggleUserFavoriteAsync(int bookId, int userId)` → Returns result tuple
- `GetUserFavoritesCountAsync(int userId)` → Gets count for user
- `AddToFavoritesAsync(...)` → Add only
- `RemoveFromFavoritesAsync(...)` → Remove only
- `IsBookFavoriteAsync(...)` → Check status
- `GetUserFavoriteBooksPagedAsync(...)` → Paginated favorites

**Side Effects**:
- Database reads/writes (UserFavorites table)
- Activity logging

**Status**: 🟢 Core - Favorites functionality (correct service)

**Notes**:
- Many methods for backward compatibility (could be consolidated)
- Uses UserFavorite entity correctly
- Good error handling

---

### Domain Services - Orders

#### RefactoredOrderQueryService
**Purpose**: Order read operations (uses repository pattern)  
**Dependencies**: `IOrderRepository`, `ILogger<RefactoredOrderQueryService>`

**Key Methods**:
- `GetTotalOrdersCountAsync()` → Delegates to repository
- `GetUserOrdersCountAsync(int userId)` → Delegates to repository
- `GetUserTotalSpentAsync(int userId)` → Delegates to repository
- `GetUserRecentOrdersAsync(...)` → Delegates to repository
- `GetUserOrderHistoryAsync(...)` → Gets from repository, applies filters in-memory
- `GetUserOrderDetailsAsync(...)` → Gets from repository, validates ownership
- `GetOrdersForAdminAsync(...)` → Gets from repository, applies filters in-memory (🔴 inefficient - gets 10x data)
- `GetMonthlyRevenueAsync()` → Calls repository for each month (12 queries)
- `GetOrderStatusDistributionAsync()` → Delegates to repository
- `GetTotalRevenueAsync()` → Delegates to repository

**Side Effects**:
- Database reads (via repository)
- No writes

**Status**: 🟢 Core - Good use of repository pattern

**Notes**:
- Line 111: `GetOrdersForAdminAsync()` gets `pageSize * 10` records then filters in-memory - inefficient
- Line 164: `GetMonthlyRevenueAsync()` makes 12 sequential queries - could be optimized
- Good separation of concerns (uses repository)

---

#### RefactoredOrderCommandService
**Purpose**: Order write operations (uses repository pattern)  
**Dependencies**: `IOrderRepository`, `IBookRepository`, `IUnitOfWork`, `ILogger`, `IActivityLogger`

**Key Methods**:
- `UpdateOrderStatusAsync(int, OrderStatus, int)` → Updates status, handles payment status, restores stock on cancel
- `CancelOrderAsync(int, int)` → Cancels order, restores stock, uses transaction
- `CreateOrderAsync(CreateOrderRequest)` → Creates order with details, deducts stock, uses transaction
- `ProcessOrderAsync(int, int)` → Wrapper for UpdateOrderStatusAsync(Processing)
- `CompleteOrderAsync(int, int)` → Wrapper for UpdateOrderStatusAsync(Delivered)
- `RestoreStockQuantitiesAsync(Order)` → Private helper to restore stock

**Side Effects**:
- Database writes (via repository, transactions)
- Activity logging
- Stock quantity updates

**Status**: 🟢 Core - Excellent implementation with proper transaction handling

**Notes**:
- Excellent transaction management
- Proper stock restoration on cancellation
- Payment status updates based on order status
- Good error handling with rollback

---

### Domain Services - Cart

#### RefactoredCartService
**Purpose**: Shopping cart management (uses repository pattern)  
**Dependencies**: `ICartRepository`, `IBookRepository`, `IOrderRepository`, `IMemoryCache`, `ILogger`, `IActivityLogger`

**Key Methods**:
- `GetUserCartAsync(int userId)` → Gets cart with caching (5 min)
- `GetCartSummaryAsync(int userId)` → Gets count and total
- `AddOrUpdateCartAsync(...)` → Validates stock, adds/updates item, clears cache
- `UpdateCartQuantityAsync(...)` → Updates quantity, validates stock
- `RemoveCartItemAsync(...)` → Soft deletes cart item
- `GetAllCartsAsync(...)` → 🔴 Returns empty list (not implemented)
- `CheckoutDetailsAsync(int userId)` → Gets cart and summary
- `ProcessCheckoutAsync(...)` → 🔴 Only clears cart, doesn't create order
- `DeductInventoryAsync(int orderId)` → Deducts stock from books
- `GetCartItemCountAsync(int userId)` → Gets count
- `AddToCartAsync(...)` → Wrapper with result tuple

**Side Effects**:
- Database reads/writes (via repository)
- Cache operations
- Activity logging

**Status**: 🟡 Suspicious - `GetAllCartsAsync()` not implemented, `ProcessCheckoutAsync()` incomplete

**Notes**:
- Line 231-244: `GetAllCartsAsync()` returns empty list - not implemented
- Line 266-287: `ProcessCheckoutAsync()` only clears cart, doesn't create order - should call OrderCommandService
- Good caching strategy
- Proper stock validation

---

### Domain Services - Users

#### CompositeUsersService
**Purpose**: Facade/composite service delegating to focused services  
**Dependencies**: `IUserQueryService`, `IUserCommandService`, `IUserApprovalService`, `UserManager<User>`, `ILogger`

**Key Methods**:
- All methods delegate to `IUserQueryService`, `IUserCommandService`, or `IUserApprovalService`
- `GetUserDetailsAsync(int userId)` → Direct UserManager access
- `GetUserStatisticsAsync(int userId)` → Returns hardcoded zeros (🔴 not implemented)

**Side Effects**:
- Database reads/writes (via delegated services)
- UserManager operations

**Status**: 🟢 Core - Good facade pattern implementation

**Notes**:
- Clean facade pattern
- Maintains backward compatibility
- Line 138-159: `GetUserStatisticsAsync()` returns zeros - not implemented

---

#### UserCommandService
**Purpose**: User write operations  
**Dependencies**: `UserManager<User>`, `RoleManager<IdentityRole<int>>`, `ILogger`, `IActivityLogger`, `IEmailSender`, `IConfiguration`

**Key Methods**:
- `CreateUserAsync(CreateUserRequest)` → Creates user, assigns role
- `UpdateUserRoleAsync(int, string)` → Updates user role
- `ToggleUserStatusAsync(int, bool)` → Activates/deactivates user (LockoutEnd)
- `SoftDeleteUserAsync(int)` → Soft deletes and locks user
- `ApproveUserAsync(int, string)` → Approves user, generates confirmation token, sends email
- `RejectUserAsync(int)` → Rejects user, soft deletes

**Side Effects**:
- Database writes (via UserManager)
- Email sends
- Activity logging

**Status**: 🟢 Core - User command operations

**Notes**:
- Good use of UserManager and RoleManager
- Email sending for approvals
- Token generation for email confirmation

---

#### UserQueryService
**Purpose**: User read operations  
**Dependencies**: `BookManagementContext`, `UserManager<User>`, `ILogger`

**Key Methods**:
- `GetTotalUsersCountAsync()` → Direct EF query
- `GetSuperAdminDashboardDataAsync()` → Aggregates multiple queries
- `GetManageUsersDataAsync(...)` → Paginated user list with filters
- `GetUsersForAdminAsync(...)` → Similar to GetManageUsersDataAsync
- `GetPendingUsersAsync()` → Gets pending approval users

**Side Effects**:
- Database reads (EF Core queries)
- UserManager reads (for roles)

**Status**: 🟢 Core - User query operations

**Notes**:
- Uses direct EF Core (no repository pattern)
- Good filtering and pagination
- Role retrieval via UserManager

---

### Domain Services - Categories

#### CategoryServices
**Purpose**: Category management  
**Dependencies**: `BookManagementContext`

**Key Methods**:
- `GetAllCategories()` → Sync method, includes books
- `AddCategory(Category)` → Sync method, direct SaveChanges
- `DeleteCategory(int)` → Async soft delete
- `GetCategoryById(int)` → Async get
- `UpdateCategory(Category)` → Async update
- `GetAllCategoriesClassified()` → Sync grouping by category
- `GetCategoriesForDropdownAsync()` → Async SelectListItem
- `GetAllCategoriesAsync()` → Async list
- `GetTotalCategoriesCountAsync()` → Async count
- `GetCategoriesWithCountAsync()` → Async with book counts
- `GetCategoryByIdAsync(int)` → Async get
- `CreateCategoryAsync(...)` → Async create with user ID
- `UpdateCategoryAsync(...)` → Async update with user ID
- `DeleteCategoryAsync(...)` → Async delete with user ID

**Side Effects**:
- Database reads/writes (direct EF Core)

**Status**: 🟡 Suspicious - Mixed sync/async, inconsistent patterns

**Notes**:
- Has both sync and async versions of same operations
- Some methods take Category entity, others take primitives
- Direct EF Core access (no repository)
- Line 199: `DeleteCategoryAsync()` calls `DeleteCategory()` - redundant

---

### Domain Services - Showcase

#### PublicDemoService
**Purpose**: Read-only public demo data with caching and graceful degradation  
**Dependencies**: `IBookQueryService`, `IBookAnalyticsService`, `IAnalyticsRepository`, `IMultiLevelCacheService`, `IGracefulDegradationService`, `ILogger`

**Key Methods**:
- `GetFeaturedBooksAsync(int count)` → Cached, uses graceful degradation
- `GetCategoriesWithCountsAsync()` → Cached
- `SearchBooksAsync(...)` → Cached search
- `GetBookDetailsAsync(int)` → Gets book details
- `GetSystemStatisticsAsync()` → System stats
- `GetBooksByCategoryAsync(...)` → Books by category
- `GetShowcaseContentAsync()` → Complete showcase data
- `GetTechnicalHighlightsAsync(...)` → Technical highlights
- `GetFeatureShowcasesAsync(...)` → Feature showcases
- `GetPerformanceMetricsAsync()` → Performance metrics

**Side Effects**:
- Database reads (via services)
- Cache reads/writes
- No writes

**Status**: 🟢 Core - Excellent implementation with caching and fallback

**Notes**:
- Excellent use of caching (multi-level)
- Graceful degradation with fallback operations
- Rate limiting considerations
- Clean separation of concerns

---

---

## 🏗️ DOMAIN ENTITIES INVENTORY

### BaseEntity
**Purpose**: Base class for all domain entities  
**Properties**:
- `Id` (int) - Primary key
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime) - Last update timestamp
- `ConcurrencyToken` (Guid) - Optimistic concurrency control (SQLite-compatible)
- `IsDeleted` (bool) - Soft delete flag

**Methods**:
- `MarkAsDeleted()` → Sets IsDeleted = true, updates timestamp
- `UpdateTimestamp()` → Updates UpdatedAt and generates new ConcurrencyToken
- `SetId(int)` → Protected method to set ID with validation

**Status**: 🟢 Core - Foundation for all entities

**Notes**:
- Uses Guid for concurrency (SQLite doesn't support RowVersion)
- Automatic timestamp management
- Soft delete pattern

---

### Book
**Purpose**: Book entity with rich domain logic  
**Properties**:
- `Title` (string) - Validated (1-200 chars)
- `Author` (string) - Validated (1-100 chars)
- `Price` (Money) - Value object
- `ISBN` (ISBN?) - Value object
- `StockQuantity` (int) - Validated (>= 0)
- `LowStockThreshold` (int) - Default 5
- `Description` (string?)
- `CategoryId` (int?)
- `IsFeatured` (bool)
- `AverageRating` (double)
- `TotalReviews` (int)
- `ImageUrl` (string?)

**Computed Properties**:
- `IsAvailable` → StockQuantity > 0 && !IsDeleted
- `IsLowStock` → StockQuantity <= LowStockThreshold && StockQuantity > 0
- `IsOutOfStock` → StockQuantity == 0
- `IsInStock` → StockQuantity > 0
- `IsFavorite` (bool) - Non-persisted, for UI

**Methods**:
- `UpdateBasicInfo(...)` → Updates title, author, price, description
- `SetISBN(string)` → Sets ISBN with validation
- `SetPublicationDate(DateTime)` → Validates not in future
- `SetImageUrl(string)` → Validates length
- `UpdateStock(int)` → Updates stock quantity
- `AddStock(int)` → Adds to stock (validates positive)
- `ReduceStock(int)` → Reduces stock (validates sufficient stock)
- `RestoreStock(int)` → Restores stock
- `SetLowStockThreshold(int)` → Sets threshold
- `SetCategory(int?)` → Sets category
- `SetFeatured(bool)` → Sets featured flag
- `UpdateRating(double)` → Updates average rating (0-5)
- `CanFulfillOrder(int)` → Checks if can fulfill order quantity

**Status**: 🟢 Core - Rich domain model with business logic

**Notes**:
- Excellent encapsulation with private fields and validation
- Business logic in entity (stock management, validation)
- Uses value objects (Money, ISBN)

---

### User
**Purpose**: User entity (extends IdentityUser<int>)  
**Properties**:
- `Name` (string) - Validated (1-100 chars)
- `IsDeleted` (bool)
- `EmailConfirmationToken` (string?)
- `EmailConfirmationTokenExpiry` (DateTime?)
- `PasswordResetToken` (string?)
- `PasswordResetExpiry` (DateTime?)
- `IsPendingApproval` (bool) - Default true
- `RequestDate` (DateTime?)
- `RequestedRole` (string?)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime)
- `LastLoginDate` (DateTime?)
- Address fields (Address, City, State, Country, ZipCode)

**Computed Properties**:
- `RefreshToken` → Gets active refresh token
- `RefreshTokenExpiryTime` → Gets expiry from active token

**Methods**:
- `UpdateProfile(string)` → Updates name
- `SetEmailConfirmationToken(...)` → Sets confirmation token
- `ConfirmEmail()` → Confirms email, clears token
- `SetPasswordResetToken(...)` → Sets reset token
- `ClearPasswordResetToken()` → Clears reset token
- `RequestRole(string)` → Requests role approval
- `ApproveUser()` → Approves user
- `UpdateLastLogin()` → Updates last login timestamp
- `SoftDelete()` → Soft deletes user
- `Restore()` → Restores user

**Status**: 🟢 Core - User management with approval workflow

**Notes**:
- Extends IdentityUser<int> for ASP.NET Identity integration
- Approval workflow built into entity
- Token management for email confirmation and password reset

---

### Order
**Purpose**: Order entity with order management logic  
**Properties**:
- `UserId` (int?)
- `TotalAmount` (Money) - Value object
- `OrderDate` (DateTime?)
- `Status` (OrderStatus) - Enum
- `PaymentStatus` (PaymentStatus) - Enum
- `PaymentMethod` (string)
- `ShippingAddress` (Address?) - Value object
- `PhoneNumber` (string?)
- `Email` (string?)
- `Notes` (string?)
- `ShippedDate` (DateTime?)
- `DeliveredDate` (DateTime?)

**Computed Properties**:
- `FullName`, `Address`, `Phone`, `City`, `State`, `Country`, `ZipCode` - Extracted from ShippingAddress

**Methods**:
- `AddOrderDetail(Book, int, Money)` → Adds order detail, validates stock, recalculates total
- `RemoveOrderDetail(int)` → Removes detail, recalculates total
- `UpdateOrderDetailQuantity(int, int)` → Updates quantity, recalculates total
- `UpdateStatus(OrderStatus)` → Updates status with state transition validation
- `UpdatePaymentStatus(PaymentStatus, string?)` → Updates payment status
- `UpdateShippingAddress(Address)` → Updates address (only if Pending)
- `CanBeCancelled()` → Checks if order can be cancelled
- `Cancel()` → Cancels order with validation
- `IsCompleted()` → Checks if order is in final state
- `IsPaid()` → Checks if payment is successful
- `GetItemsTotal()` → Calculates items total
- `GetTotalItemCount()` → Gets total item count
- `RecalculateTotal()` → Private method to recalculate total

**Status**: 🟢 Core - Rich order management with state transitions

**Notes**:
- Excellent domain logic (state transitions, validation)
- Uses value objects (Money, Address)
- Business rules enforced in entity

---

### OrderDetail
**Purpose**: Order detail line item  
**Properties**:
- `OrderId` (int)
- `BookId` (int)
- `Quantity` (int) - Validated (> 0), auto-recalculates subtotal
- `UnitPrice` (Money) - Value object, auto-recalculates subtotal
- `Subtotal` (Money) - Calculated property
- `Price` (Money) - Compatibility property (alias for UnitPrice)

**Computed Properties**:
- `TotalPrice` (decimal) → Quantity * Price.Amount

**Methods**:
- `UpdateQuantity(int)` → Updates quantity
- `UpdateUnitPrice(Money)` → Updates unit price
- `GetTotalPrice()` → Returns subtotal
- `RecalculateSubtotal()` → Private method to recalculate

**Status**: 🟢 Core - Order detail with automatic calculations

**Notes**:
- Automatic subtotal recalculation on quantity/price changes
- Uses Money value object

---

### Category
**Purpose**: Book category entity  
**Properties**:
- `Name` (string) - Validated (1-100 chars)
- `Description` (string?) - Validated (max 500 chars)

**Navigation Properties**:
- `Books` (IReadOnlyCollection<Book>) - Read-only collection

**Methods**:
- `UpdateName(string)` → Updates name
- `SetDescription(string?)` → Sets description with validation
- `GetBookCount()` → Gets count of non-deleted books
- `HasBooks()` → Checks if has books
- `AddBook(Book)` → Internal method for EF Core
- `RemoveBook(Book)` → Internal method for EF Core

**Status**: 🟢 Core - Category with encapsulation

**Notes**:
- Read-only collection for Books (encapsulation)
- Internal methods for EF Core relationship management

---

### ShoppingCart
**Purpose**: Shopping cart item entity  
**Properties**:
- `UserId` (int)
- `BookId` (int)
- `Quantity` (int) - Validated (> 0)
- `AddedAt` (DateTime)

**Methods**:
- `UpdateQuantity(int)` → Updates quantity
- `IncreaseQuantity(int)` → Increases quantity
- `DecreaseQuantity(int)` → Decreases quantity (validates >= 1)
- `GetSubtotal()` → Calculates subtotal from Book.Price

**Status**: 🟢 Core - Shopping cart item

**Notes**:
- Simple entity with quantity management
- Calculates subtotal from navigation property

---

### BookReview
**Purpose**: Book review entity with moderation  
**Properties**:
- `BookId` (int)
- `UserId` (int)
- `Rating` (int) - Validated (1-5)
- `ReviewText` (string) - Validated (10-1000 chars)
- `Status` (ReviewStatus) - Enum, default Pending
- `RejectionReason` (string?)
- `ModeratedBy` (int?)
- `ModeratedAt` (DateTime?)

**Methods**:
- `UpdateReview(int, string)` → Updates review, resets to Pending
- `Approve(int)` → Approves review, sets moderator
- `Reject(int, string)` → Rejects review with reason
- `CanBeModerated()` → Checks if status is Pending
- `IsApproved()` → Checks if approved

**Status**: 🟢 Core - Review with moderation workflow

**Notes**:
- Moderation workflow built into entity
- Status management with approval/rejection

---

### UserFavorite
**Purpose**: User favorite book relationship  
**Properties**:
- `UserId` (int)
- `BookId` (int)
- `CreatedAt` (DateTime)
- `AddedAt` (DateTime) - Alias for CreatedAt

**Status**: 🟢 Core - Simple relationship entity

**Notes**:
- Simple junction table entity
- Both CreatedAt and AddedAt (redundant but for compatibility)

---

### ActivityLog
**Purpose**: Activity logging entity  
**Properties**:
- `UserId` (int?)
- `Action` (string) - Validated (1-100 chars)
- `Message` (string) - Validated (1-1000 chars)
- `IpAddress` (string?) - Max 45 chars
- `UserAgent` (string?) - Max 500 chars
- `Level` (string) - Default "Info"
- `Timestamp` (DateTime)

**Computed Properties**:
- `ActionType` → Alias for Action
- `Description` → Alias for Message

**Methods**:
- `UpdateLevel(string)` → Updates log level

**Status**: 🟢 Core - Activity logging

**Notes**:
- String truncation in constructor for IpAddress/UserAgent
- Compatibility properties for different naming conventions

---

## 📚 REPOSITORIES INVENTORY

### Repository<T> (Base)
**Purpose**: Generic repository base class  
**Dependencies**: `BookManagementContext`

**Methods**:
- `GetByIdAsync(int, CancellationToken)` → Gets entity by ID (excludes deleted)
- `GetAllAsync(CancellationToken)` → Gets all non-deleted entities
- `GetByConditionAsync(Expression<Func<T, bool>>, CancellationToken)` → Gets by predicate
- `FindAsync(Expression<Func<T, bool>>, CancellationToken)` → Same as GetByConditionAsync
- `FirstOrDefaultAsync(Expression<Func<T, bool>>, CancellationToken)` → Gets first matching
- `ExistsAsync(Expression<Func<T, bool>>, CancellationToken)` → Checks existence
- `CountAsync(Expression<Func<T, bool>>?, CancellationToken)` → Counts entities
- `AddAsync(T, CancellationToken)` → Adds entity
- `AddRangeAsync(IEnumerable<T>, CancellationToken)` → Adds multiple
- `Update(T)` → Updates entity (sync)
- `UpdateAsync(T, CancellationToken)` → Updates entity, calls UpdateTimestamp()
- `UpdateRange(IEnumerable<T>)` → Updates multiple
- `Remove(T)` → Soft deletes entity (calls MarkAsDeleted)
- `RemoveRange(IEnumerable<T>)` → Soft deletes multiple
- `SaveChangesAsync(CancellationToken)` → Saves changes, handles concurrency conflicts

**Status**: 🟢 Core - Generic repository with soft delete and concurrency handling

**Notes**:
- Excellent concurrency conflict handling (reloads on conflict)
- Automatic soft delete filtering
- UpdateTimestamp() called automatically on UpdateAsync

---

### BookRepository
**Purpose**: Book-specific repository  
**Implements**: `IBookRepository`, `IBookReadRepository`, `IBookWriteRepository`, `IBookQueryRepository`

**Methods**:
- `GetBooksByCategoryAsync(int, CancellationToken)` → Gets books by category
- `GetFeaturedBooksAsync(int, CancellationToken)` → Gets featured books
- `GetLowStockBooksAsync(CancellationToken)` → Gets low stock books
- `SearchBooksAsync(string, CancellationToken)` → Searches books
- `GetPagedBooksAsync(...)` → Gets paginated books with filters and sorting
- `ExistsAsync(int, CancellationToken)` → Checks book exists
- `IsbnExistsAsync(string, CancellationToken)` → Checks ISBN exists
- `CountByCategoryAsync(int, CancellationToken)` → Counts by category
- `GetAveragePriceAsync(CancellationToken)` → Gets average price
- `GetAveragePriceByCategoryAsync(int, CancellationToken)` → Gets average by category

**Status**: 🟢 Core - Book repository with comprehensive queries

**Notes**:
- Implements multiple interfaces (CQRS pattern?)
- Good query methods for different use cases

---

### OrderRepository
**Purpose**: Order-specific repository  
**Implements**: `IOrderRepository`

**Methods**:
- `GetOrderWithDetailsAsync(int)` → Gets order with details and user
- `GetUserOrdersAsync(int, int, int)` → Gets paginated user orders
- `GetOrdersByStatusAsync(OrderStatus)` → Gets orders by status
- `GetTotalRevenueAsync()` → Gets total revenue
- `GetMonthlyRevenueAsync(int, int)` → Gets monthly revenue
- `GetTotalOrdersCountAsync()` → Gets total count
- `GetUserOrdersCountAsync(int)` → Gets user order count
- `GetUserTotalSpentAsync(int)` → Gets user total spent
- `GetUserRecentOrdersAsync(int, int)` → Gets recent user orders
- `GetRecentOrdersAsync(int)` → Gets recent orders (all users)
- `GetOrderStatusDistributionAsync()` → Gets status distribution
- `GetOrdersForDateRangeAsync(DateTime, DateTime)` → Gets orders in date range
- `UpdateAsync(Order)` → Updates order

**Status**: 🟢 Core - Comprehensive order repository

**Notes**:
- Good use of Include() for eager loading
- Analytics methods (revenue, distribution)
- Date range queries

---

### CartRepository
**Purpose**: Shopping cart repository  
**Implements**: `ICartRepository`

**Methods**:
- `GetUserCartAsync(int)` → Gets user cart with books and categories
- `GetCartItemAsync(int, int)` → Gets specific cart item
- `GetCartItemsCountAsync(int)` → Gets total item count (sum of quantities)
- `GetCartTotalAsync(int)` → Gets cart total (sum of prices * quantities)
- `ClearUserCartAsync(int)` → Soft deletes all user cart items
- `HasCartItemAsync(int, int)` → Checks if item exists
- `GetCartItemsWithBooksAsync(int)` → Gets cart items with book details
- `UpdateAsync(ShoppingCart)` → Updates cart item

**Status**: 🟢 Core - Cart repository

**Notes**:
- Good eager loading with Include()
- ClearUserCartAsync uses soft delete (good)

---

### UnitOfWork
**Purpose**: Unit of Work pattern implementation  
**Dependencies**: `BookManagementContext`

**Properties**:
- `Books` (IBookRepository) - Legacy property
- `Categories` (ICategoryRepository) - Legacy property

**Methods**:
- `SaveChangesAsync(CancellationToken)` → Saves changes, handles concurrency
- `BeginTransactionAsync(CancellationToken)` → Begins transaction
- `CommitTransactionAsync(CancellationToken)` → Commits transaction
- `RollbackTransactionAsync(CancellationToken)` → Rollbacks transaction
- `Dispose()` → Disposes transaction and context

**Status**: 🟢 Core - Unit of Work with transaction support

**Notes**:
- Transaction management
- Concurrency conflict handling
- Legacy repository properties for backward compatibility

---

---

## 🔌 INFRASTRUCTURE SERVICES INVENTORY

### Authentication Services

#### AuthService
**Purpose**: Authentication and user management implementation  
**Dependencies**: `UserManager<User>`, `RoleManager<IdentityRole<int>>`, `BookManagementContext`, `IConfiguration`, `IDnsChecker`, `IMemoryCache`, `ILogger`, `IEmailSender`, `IHttpContextAccessor`

**Key Methods**:
- `SeedRolesAsync()` → Seeds roles (SuperAdmin, Admin, User, Guest)
- `ValidateUserAsync(LoginViewModel)` → Validates user login with comprehensive checks
- `GenerateTokensAsync(User)` → Generates JWT access token and refresh token
- `RegisterUserAsync(RegisterViewModel)` → Registers new user
- `ConfirmEmailAsync(string, string)` → Confirms email
- `UpdatePasswordAsync(string, string)` → Updates password from reset token
- `GeneratePasswordResetTokenAsync(string)` → Generates password reset token
- `GetUserProfileAsync(int)` → Gets user profile
- `UpdateUserDetailAsync(...)` → Updates user details (2 overloads)
- `AssignRoleAsync(int, string)` → Assigns role to user
- `GetUserRolesAsync(int)` → Gets user roles
- `RevokeRefreshTokensAsync(int)` → Revokes all refresh tokens for user
- `RefreshTokenAsync(string)` → Refreshes access token
- `ChangePasswordAsync(int, string, string)` → Changes password
- `SendWelcomeEmailAsync(User)` → Sends welcome email
- `SendUserApprovedEmailAsync(User, string)` → Sends approval email

**Side Effects**:
- Database writes (users, refresh tokens)
- Email sends
- JWT token generation
- Cache operations

**Status**: 🟢 Core - Comprehensive authentication service

**Notes**:
- Excellent validation (deleted users, pending approval, email confirmation, lockout)
- JWT token generation with refresh tokens
- Refresh token stored in database with hashing
- Email sending integration

---

#### RoleBasedRedirectionService
**Purpose**: Role-based redirection logic  
**Dependencies**: `IUserQueryService`, `ILogger<RoleBasedRedirectionService>`

**Key Methods**:
- `GetRedirectUrlForUserAsync(int)` → 🔴 Returns default User redirect (not implemented)
- `GetDefaultRedirectForRoleAsync(string)` → Gets default redirect for role
- `ShouldBypassPublicArea(ClaimsPrincipal)` → Checks if authenticated user should bypass public
- `GetHighestPriorityRole(ClaimsPrincipal)` → Gets highest priority role (SuperAdmin > Admin > User)
- `GetRedirectUrlForClaimsAsync(ClaimsPrincipal)` → Gets redirect from claims
- `IsValidRedirectUrl(string, ClaimsPrincipal)` → Validates redirect URL (prevents open redirect)

**Side Effects**:
- No side effects (read-only)

**Status**: 🟡 Suspicious - `GetRedirectUrlForUserAsync()` not fully implemented

**Notes**:
- Good security (open redirect prevention)
- Role priority mapping
- Line 50-67: `GetRedirectUrlForUserAsync()` defaults to User role, doesn't query actual role

---

### System Services

#### SystemSettingsService
**Purpose**: System settings management  
**Dependencies**: `IConfiguration`, `IMemoryCache`, `ILogger`, `BookManagementContext`, `IWebHostEnvironment`, `IOptions<EmailSettings>`, `IDataProtectionProvider`

**Key Methods**:
- `GetSystemSettingsAsync()` → Gets system settings from configuration
- `GetEmailSettingsAsync()` → Gets email settings (cache → DB → config fallback)
- `UpdateGeneralSettingsAsync(...)` → Updates general settings (cache only)
- `UpdateSecuritySettingsAsync(...)` → Updates security settings
- `UpdateEmailSettingsAsync(...)` → Updates email settings (encrypted password)
- `TestEmailConfigurationAsync()` → Tests email configuration
- `ClearCacheAsync()` → Clears cache
- `BackupDatabaseAsync()` → Backs up database

**Side Effects**:
- Database reads/writes (SystemSettings table)
- Cache operations
- Email sends (test)
- File system writes (backups)

**Status**: 🟢 Core - System settings with encryption

**Notes**:
- Email password encryption using DataProtectionProvider
- Three-tier fallback (cache → DB → config)
- Line 128-142: `UpdateGeneralSettingsAsync()` only updates cache, not DB

---

### Logging Services

#### ActivityLogger
**Purpose**: Activity logging implementation  
**Dependencies**: `BookManagementContext`, `UserManager<User>`, `ILogger`, `IHttpContextAccessor`

**Key Methods**:
- `LogAsync(string, string?, int?)` → Logs activity with IP and UserAgent
- `LogActivityAsync(...)` → Alias for LogAsync
- `GetLogsAsync(int?)` → Gets logs (optionally filtered by user)
- `GetAllLogsAsync()` → Gets all logs as ViewModels
- `GetTodayLogsAsync()` → Gets today's logs
- `GetFilteredLogsAsync(...)` → Gets filtered logs (date, search, action)
- `GetActivityLogsAsync(...)` → Gets paginated activity logs
- `ClearOldLogsAsync(int)` → Clears logs older than N days

**Side Effects**:
- Database writes (ActivityLogs table)
- Database reads

**Status**: 🟢 Core - Activity logging with filtering

**Notes**:
- Captures IP address and UserAgent automatically
- Uses Indian timezone for timestamps
- Good filtering capabilities

---

### Caching Services

#### MultiLevelCacheService
**Purpose**: Multi-level caching (Memory + Distributed)  
**Dependencies**: `IMemoryCache`, `IDistributedCache`, `ILogger`

**Key Methods**:
- `GetOrSetAsync<T>(string, Func<Task<T>>, TimeSpan)` → Gets or sets with factory
- `TryGetValue<T>(string, out T?)` → Tries to get value
- `Set<T>(string, T, TimeSpan)` → Sets value in both caches
- `Remove(string)` → Removes from both caches
- `Clear()` → Clears all cache

**Side Effects**:
- Cache reads/writes (memory and distributed)

**Status**: 🟢 Core - Excellent multi-level caching

**Notes**:
- L1: Memory cache (fastest)
- L2: Distributed cache (shared, persistent)
- L3: Factory method (most expensive)
- Automatic fallback if cache fails
- Memory cache duration capped at 5 minutes

---

## 🔗 DEPENDENCY MAPPING & CALL CHAINS

### Controller → Service Dependencies

#### PublicController
- `IBookQueryService` → BookQueryService
- `IBookAnalyticsService` → RefactoredAnalyticsService
- `IPublicDemoService` → PublicDemoService
- `IRoleBasedRedirectionService` → RoleBasedRedirectionService
- `IMemoryCache` → Built-in

**Call Chain**: PublicController → PublicDemoService → BookQueryService → BookRepository → DbContext

---

#### AuthController
- `IAuthService` → AuthService
- `IActivityLogger` → ActivityLogger

**Call Chain**: AuthController → AuthService → UserManager/RoleManager → DbContext

---

#### AdminController
- `IBookQueryService` → BookQueryService
- `IBookCommandService` → BookCommandService
- `IBookAnalyticsService` → RefactoredAnalyticsService
- `ICartService` → RefactoredCartService
- `IOrderQueryService` → RefactoredOrderQueryService
- `IOrderCommandService` → RefactoredOrderCommandService
- `IActivityLogger` → ActivityLogger
- `IUsersService` → CompositeUsersService
- `ICategoryInterface` → CategoryServices

**Call Chain**: AdminController → Services → Repositories → UnitOfWork → DbContext

---

#### UserController
- `IBookQueryService` → BookQueryService
- `IBookFavoriteService` → BookFavoriteService
- `IBookCommandService` → BookCommandService (🔴 for UpdateProfile)
- `IUserCommandService` → UserCommandService
- `ICartService` → RefactoredCartService
- `IOrderQueryService` → RefactoredOrderQueryService
- `IOrderCommandService` → RefactoredOrderCommandService
- `IActivityLogger` → ActivityLogger
- `ICategoryInterface` → CategoryServices
- `IAuthService` → AuthService

**Call Chain**: UserController → Services → Repositories → UnitOfWork → DbContext

---

### Service → Repository Dependencies

#### RefactoredOrderQueryService
- `IOrderRepository` → OrderRepository

#### RefactoredOrderCommandService
- `IOrderRepository` → OrderRepository
- `IBookRepository` → BookRepository
- `IUnitOfWork` → UnitOfWork

#### RefactoredCartService
- `ICartRepository` → CartRepository
- `IBookRepository` → BookRepository
- `IOrderRepository` → OrderRepository
- `IMemoryCache` → Built-in

#### BookQueryService
- `BookManagementContext` → Direct EF Core (🔴 should use repository)

#### BookCommandService
- `BookManagementContext` → Direct EF Core (🔴 should use repository)

---

## 🔐 AUTHENTICATION & AUTHORIZATION FLOW

### Authentication Flow

1. **Login Request** → `AuthController.LoginData()`
   - Calls `IAuthService.ValidateUserAsync()`
   - Checks: user exists, not deleted, not pending, email confirmed, not locked out
   - On success: `IAuthService.GenerateTokensAsync()`
   - Returns JWT access token + refresh token
   - Sets access token in HttpOnly cookie

2. **Token Generation** → `AuthService.GenerateTokensAsync()`
   - Creates JWT with claims (NameIdentifier, Email, Name, Roles)
   - Generates refresh token (64 bytes, base64)
   - Hashes refresh token (SHA256)
   - Stores refresh token in database (RefreshToken entity)
   - Returns both tokens

3. **Token Refresh** → `AuthController.RefreshToken()`
   - Validates refresh token
   - Generates new access token
   - Optionally rotates refresh token

4. **Logout** → `AuthController.Logout()`
   - Revokes all refresh tokens for user
   - Deletes access token cookie

---

### Authorization Flow

1. **Role-Based Policies** (defined in Program.cs / ServiceCollectionExtensions)
   - `UserOrHigher` → User, Admin, SuperAdmin
   - `AdminOrHigher` → Admin, SuperAdmin
   - `SuperAdminOnly` → SuperAdmin only

2. **Policy Enforcement**
   - `[Authorize(Policy = "AdminOrHigher")]` on controller actions
   - BaseController determines layout based on role
   - RoleSwitchingMiddleware allows SuperAdmin to switch view roles

3. **Role Priority**
   - SuperAdmin (priority 3)
   - Admin (priority 2)
   - User (priority 1)
   - Guest (no priority, unauthenticated)

4. **Redirection Logic**
   - `RoleBasedRedirectionService.GetRedirectUrlForClaimsAsync()`
   - Gets highest priority role from claims
   - Redirects to appropriate dashboard

---

### User Registration & Approval Flow

1. **Registration** → `AuthController.SaveData()`
   - Calls `IAuthService.RegisterUserAsync()`
   - Creates user with `IsPendingApproval = true`
   - Generates email confirmation token
   - Sends confirmation email

2. **Email Confirmation** → `AuthController.ConfirmEmail()`
   - Validates token
   - Sets `EmailConfirmed = true`
   - User still pending approval

3. **Admin Approval** → `SuperAdminController.ApproveUser()`
   - Calls `IUsersService.ApproveUserAsync()`
   - Sets `IsPendingApproval = false`
   - Assigns role
   - Generates new confirmation token
   - Sends approval email with confirmation link

4. **User Can Login** → After approval and email confirmation

---

## 💾 DATABASE WRITE OPERATIONS

### Write Operations by Entity

#### Books
- **Create**: `BookCommandService.AddBookAsync()` → `DbContext.Books.Add()` → `SaveChangesAsync()`
- **Update**: `BookCommandService.UpdateBookAsync()` → `DbContext.Books.Update()` → `SaveChangesAsync()`
- **Delete**: `BookCommandService.SoftDeleteBookAsync()` → Sets `IsDeleted = true` → `SaveChangesAsync()`

#### Orders
- **Create**: `RefactoredOrderCommandService.CreateOrderAsync()` → Transaction → `IOrderRepository.AddAsync()` → `IUnitOfWork.SaveChangesAsync()` → `CommitTransactionAsync()`
- **Update Status**: `RefactoredOrderCommandService.UpdateOrderStatusAsync()` → Transaction → `IOrderRepository.UpdateAsync()` → `SaveChangesAsync()` → `CommitTransactionAsync()`
- **Cancel**: `RefactoredOrderCommandService.CancelOrderAsync()` → Transaction → Restores stock → Updates order → `CommitTransactionAsync()`

#### Users
- **Create**: `UserCommandService.CreateUserAsync()` → `UserManager.CreateAsync()` → `UserManager.AddToRoleAsync()`
- **Update Role**: `UserCommandService.UpdateUserRoleAsync()` → `UserManager.RemoveFromRolesAsync()` → `UserManager.AddToRoleAsync()`
- **Approve**: `UserCommandService.ApproveUserAsync()` → Updates user → Assigns role → Sends email

#### Shopping Cart
- **Add/Update**: `RefactoredCartService.AddOrUpdateCartAsync()` → `ICartRepository.AddAsync()` or `UpdateAsync()` → `SaveChangesAsync()`
- **Remove**: `RefactoredCartService.RemoveCartItemAsync()` → Soft delete → `SaveChangesAsync()`
- **Clear**: `RefactoredCartService.ProcessCheckoutAsync()` → `ICartRepository.ClearUserCartAsync()` → Soft deletes all items

#### Reviews
- **Submit**: `ReviewService.SubmitReviewAsync()` → `DbContext.BookReviews.Add()` → `SaveChangesAsync()` → Recalculates rating
- **Update**: `ReviewService.UpdateReviewAsync()` → Updates review → Resets to Pending → `SaveChangesAsync()`
- **Delete**: `ReviewService.DeleteReviewAsync()` → Soft delete → `SaveChangesAsync()`

#### Activity Logs
- **Log**: `ActivityLogger.LogAsync()` → `DbContext.ActivityLogs.Add()` → `SaveChangesAsync()`
- **Clear Old**: `ActivityLogger.ClearOldLogsAsync()` → Deletes logs older than N days

---

### Transaction Patterns

1. **Order Creation** (RefactoredOrderCommandService)
   - BeginTransaction → Create Order → Create OrderDetails → Update Stock → SaveChanges → CommitTransaction
   - On error: RollbackTransaction

2. **Order Status Update** (RefactoredOrderCommandService)
   - BeginTransaction → Update Order → Update Payment Status → Restore Stock (if cancelled) → SaveChanges → CommitTransaction

3. **Book Creation** (BookCommandService)
   - BeginTransaction → Add Book → Save Image → Update Book with Image URL → SaveChanges → CommitTransaction
   - On error: RollbackTransaction + Cleanup image file

---

## 🚧 NEXT STEPS

1. ✅ Complete controller inventory
2. ✅ Inventory Application Services layer (interfaces)
3. ✅ Inventory Application Services layer (implementations)
4. ✅ Inventory Domain Entities
5. ✅ Inventory Infrastructure Services (implementations)
6. ✅ Inventory Repositories
7. ✅ Map dependencies and call chains
8. ✅ Document authentication/authorization flow
9. ✅ Document database write operations

---

## 📊 PHASE 1 COMPLETION SUMMARY

### Inventory Statistics
- **Controllers**: 11 controllers, ~129 methods
- **Service Interfaces**: 15+ interfaces
- **Service Implementations**: 8 major services
- **Domain Entities**: 14 entities
- **Repositories**: 5 repositories + base
- **Infrastructure Services**: 5+ services

### Violations Found
- **Critical**: 6 violations (OrderController, BookCommandService, BookQueryService, etc.)
- **Suspicious**: 8 patterns (mixed responsibilities, incomplete implementations)
- **Core**: 30+ components working correctly

### Architecture Assessment
- **Strengths**: Rich domain models, repository pattern, value objects, transaction handling
- **Weaknesses**: Some direct DbContext access, mixed responsibilities, incomplete implementations
- **Overall**: Good foundation with Clean Architecture principles, needs refactoring in specific areas

---

**Last Updated**: 2025-01-30  
**Phase 1 Status**: ✅ COMPLETE  
**Ready for Phase 2**: Role & Flow Definition
