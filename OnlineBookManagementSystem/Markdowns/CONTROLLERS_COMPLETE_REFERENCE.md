# Complete Controller Reference Guide

**Last Updated:** January 29, 2026  
**Total Controllers:** 26 (Active)  
**Architecture:** Clean Architecture + SOLID Principles

---

## 📋 Table of Contents

1. [Base Controllers](#base-controllers)
2. [Public Access Controllers](#public-access-controllers)
3. [Authentication Controllers](#authentication-controllers)
4. [Admin Controllers](#admin-controllers)
5. [User Controllers](#user-controllers)
6. [SuperAdmin Controllers](#superadmin-controllers)
7. [Feature Controllers](#feature-controllers)
8. [API Controllers](#api-controllers)
9. [Utility Controllers](#utility-controllers)
10. [Quick Reference Table](#quick-reference-table)

---

## 🏗️ Base Controllers

### 1. BaseController
**File:** `Presentation/Controllers/BaseController.cs`  
**Inherits:** `Controller`  
**Purpose:** Base class for all MVC controllers  
**Authorization:** None (inherited by children)

**Provides:**
- Logger injection
- User claims helper methods
- Common error handling
- Shared controller functionality

**Used By:** All MVC controllers

**Key Methods:**
```csharp
protected int GetUserIdFromClaims()
protected string GetUserEmailFromClaims()
protected ILogger<T> Logger
```

---

## 🌐 Public Access Controllers

### 2. HomeController
**File:** `Presentation/Controllers/HomeController.cs`  
**Inherits:** `BaseController`  
**Authorization:** None (Public + Authenticated)

**Purpose:** Application entry point and static pages

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/` | GET | Landing page (redirects based on auth) |
| `/Home/About` | GET | About page |
| `/Home/Support` | GET | Support/Contact page |
| `/Home/Terms` | GET | Terms & Conditions |

**Dependencies:**
- `IActivityLogger` (optional)

**Behavior:**
- Authenticated users → Redirect to role-specific dashboard
- Unauthenticated users → Redirect to Public landing page

---

### 3. PublicController ✨ NEW
**File:** `Presentation/Controllers/PublicController.cs`  
**Inherits:** `BaseController`  
**Authorization:** None (AllowAnonymous)

**Purpose:** Public-facing book browsing for visitors

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Public/Index` | GET | Landing page with featured books |
| `/Public/Browse` | GET | Browse all books with filters |
| `/Public/BookDetails/{id}` | GET | View book details |
| `/Public/SearchBooks` | GET | AJAX search endpoint |
| `/Public/GetBooksByCategory/{id}` | GET | AJAX category filter |

**Dependencies:**
- `IBookQueryService`
- `IBookAnalyticsService`

**Features:**
- Modern UI with aurora backgrounds
- Spotlight card effects
- Magnetic buttons
- Responsive filters
- Pagination

---

### 4. ErrorController
**File:** `Presentation/Controllers/ErrorController.cs`  
**Inherits:** `Controller`  
**Authorization:** None

**Purpose:** Centralized error handling

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Error/{statusCode}` | GET | Display error page |

**Dependencies:**
- `IErrorViewModelFactory`

**Handles:**
- 404 Not Found
- 401 Unauthorized
- 403 Forbidden
- 500 Internal Server Error
- Custom error pages

---

## 🔐 Authentication Controllers

### 5. AuthController
**File:** `Presentation/Controllers/AuthController.cs`  
**Inherits:** `BaseController`  
**Authorization:** AllowAnonymous (most actions)

**Purpose:** User authentication and account management

**Routes:**
| Route | Method | Auth | Description |
|-------|--------|------|-------------|
| `/Auth/Login` | GET | No | Login page |
| `/Auth/Login` | POST | No | Process login |
| `/Auth/Registration` | GET | No | Registration page |
| `/Auth/Registration` | POST | No | Process registration |
| `/Auth/Logout` | POST | Yes | Logout user |
| `/Auth/ForgotPassword` | POST | No | Password reset request |
| `/Auth/ResetPassword` | POST | No | Reset password |
| `/Auth/ConfirmEmail` | GET | No | Email confirmation |
| `/Auth/ProfileView` | GET | Yes | View profile |

**Dependencies:**
- `IAuthService`
- `IUsersService`
- `IActivityLogger`

**Features:**
- JWT token authentication
- Refresh token support
- Email confirmation
- Password reset
- Activity logging

---

## 👨‍💼 Admin Controllers

### 6. AdminController
**File:** `Presentation/Controllers/AdminController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize]` + Role checks

**Purpose:** Main admin controller (legacy/compatibility)

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Admin/Dashboard` | GET | Admin dashboard |
| `/Admin/Books` | GET | Book management |
| `/Admin/CreateBook` | GET | Create book form |
| `/Admin/CategoryManagement` | GET | Category management |
| `/Admin/OrderManagement` | GET | Order management |
| `/Admin/UserList` | GET | User management |
| `/Admin/ActivityLogs` | GET | Activity logs |

**Dependencies:**
- `IBookQueryService`
- `IOrderQueryService`
- `IUsersService`
- `IActivityLogger`

**Note:** Some functionality delegated to specialized admin controllers

---

### 7. AdminDashboardController
**File:** `Presentation/Controllers/Admin/AdminDashboardController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "AdminOrHigher")]`

**Purpose:** Admin dashboard with analytics and overview

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Admin/Dashboard` | GET | Main dashboard view |
| `/Admin/Dashboard/GetStats` | GET | Dashboard statistics (AJAX) |
| `/Admin/Dashboard/GetChartData` | GET | Chart data (AJAX) |

**Dependencies:**
- `IBookQueryService`
- `IOrderQueryService`
- `IUsersService`
- `IChartDataProvider`
- `IActivityLogger`

**Features:**
- Bento grid layout
- Real-time statistics
- Interactive charts (Chart.js)
- Recent activity feed
- Quick actions

---

### 8. AdminBookManagementController
**File:** `Presentation/Controllers/Admin/AdminBookManagementController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "AdminOrHigher")]`

**Purpose:** Complete book CRUD operations

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Admin/Books` | GET | List all books |
| `/Admin/Books/Create` | GET | Create book form |
| `/Admin/Books/Create` | POST | Save new book |
| `/Admin/Books/Edit/{id}` | GET | Edit book form |
| `/Admin/Books/Edit/{id}` | POST | Update book |
| `/Admin/Books/Delete/{id}` | POST | Delete book |
| `/Admin/Books/Details/{id}` | GET | View book details |
| `/Admin/Books/GetBooks` | GET | AJAX book list |

**Dependencies:**
- `IBookQueryService`
- `IBookCommandService`
- `ICategoryInterface`
- `IActivityLogger`

**Features:**
- Image upload
- Stock management
- Category assignment
- Soft delete
- Search and filters
- Pagination

---

### 9. AdminCategoryManagementController
**File:** `Presentation/Controllers/Admin/AdminCategoryManagementController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "AdminOrHigher")]`

**Purpose:** Category CRUD operations

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Admin/CategoryManagement` | GET | List categories |
| `/Admin/Category/Create` | POST | Create category |
| `/Admin/Category/Edit/{id}` | POST | Update category |
| `/Admin/Category/Delete/{id}` | POST | Delete category |

**Dependencies:**
- `ICategoryInterface`
- `IActivityLogger`

**Features:**
- Category hierarchy
- Book count per category
- AJAX operations

---

### 10. AdminOrderManagementController
**File:** `Presentation/Controllers/Admin/AdminOrderManagementController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "AdminOrHigher")]`

**Purpose:** View and manage all orders

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Admin/OrderManagement` | GET | List all orders |
| `/Admin/Order/Details/{id}` | GET | Order details |
| `/Admin/Order/UpdateStatus/{id}` | POST | Update order status |
| `/Admin/Order/GetOrders` | GET | AJAX order list |

**Dependencies:**
- `IOrderQueryService`
- `IOrderCommandService`
- `IActivityLogger`

**Features:**
- Order status management
- Payment tracking
- Order search and filters
- Export functionality

---

### 11. AdminUserManagementController
**File:** `Presentation/Controllers/Admin/AdminUserManagementController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "AdminOrHigher")]`

**Purpose:** Manage user accounts

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Admin/UserList` | GET | List all users |
| `/Admin/User/Details/{id}` | GET | User details |
| `/Admin/User/Edit/{id}` | POST | Update user |
| `/Admin/User/Delete/{id}` | POST | Delete user |
| `/Admin/User/AssignRole` | POST | Assign role to user |

**Dependencies:**
- `IUsersService`
- `IActivityLogger`

**Features:**
- User search
- Role assignment
- Account status management
- Activity history

---

### 12. ReviewModerationController
**File:** `Presentation/Controllers/Admin/ReviewModerationController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "AdminOrHigher")]`  
**Route Prefix:** `/Admin/Reviews`

**Purpose:** Moderate user reviews

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Admin/Reviews/Pending` | GET | Pending reviews |
| `/Admin/Reviews/Analytics` | GET | Review analytics |
| `/Admin/Reviews/Approve/{id}` | POST | Approve review |
| `/Admin/Reviews/Reject/{id}` | POST | Reject review |

**Dependencies:**
- `IReviewService`
- `IActivityLogger`

**Features:**
- Review queue
- Bulk actions
- Analytics dashboard
- Spam detection

---

## 👤 User Controllers

### 13. UserController
**File:** `Presentation/Controllers/UserController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize]`

**Purpose:** Main user controller (legacy/compatibility)

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/User/Dashboard` | GET | User dashboard |
| `/User/UserBookList` | GET | Browse books |
| `/User/UserCart` | GET | Shopping cart |
| `/User/Favorite` | GET | Favorite books |
| `/User/OrderHistory` | GET | Order history |
| `/User/Profile` | GET | User profile |

**Dependencies:**
- `IBookQueryService`
- `ICartService`
- `IOrderQueryService`

---

### 14. UserDashboardController
**File:** `Presentation/Controllers/User/UserDashboardController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "UserOrHigher")]`

**Purpose:** User's personal dashboard

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/User/Dashboard` | GET | Main dashboard |
| `/User/Dashboard/GetRecommendations` | GET | Book recommendations |
| `/User/Dashboard/GetActivity` | GET | Recent activity |

**Dependencies:**
- `IBookQueryService`
- `IOrderQueryService`
- `IBookFavoriteService`
- `IActivityLogger`

**Features:**
- Personalized recommendations
- Order summary
- Reading statistics
- Quick actions

---

### 15. UserBookBrowsingController
**File:** `Presentation/Controllers/User/UserBookBrowsingController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "UserOrHigher")]`

**Purpose:** Browse books as authenticated user

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/User/UserBookList` | GET | Browse books |
| `/User/BookDetails/{id}` | GET | Book details |
| `/User/SearchBooks` | GET | Search books |

**Dependencies:**
- `IBookQueryService`
- `IBookFavoriteService`

**Features:**
- Personalized browsing
- Favorite indicators
- Advanced filters
- Cart integration

---

### 16. UserFavoritesController
**File:** `Presentation/Controllers/User/UserFavoritesController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "UserOrHigher")]`

**Purpose:** Manage favorite books

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/User/Favorite` | GET | List favorites |
| `/User/ToggleFavorite/{id}` | POST | Add/remove favorite |
| `/User/RemoveFavorite/{id}` | POST | Remove favorite |

**Dependencies:**
- `IBookFavoriteService`
- `IBookQueryService`

**Features:**
- Favorite list
- Quick add/remove
- Favorite statistics

---

### 17. UserOrdersController
**File:** `Presentation/Controllers/User/UserOrdersController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "UserOrHigher")]`

**Purpose:** View order history

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/User/OrderHistory` | GET | List orders |
| `/User/OrderDetails/{id}` | GET | Order details |
| `/User/CancelOrder/{id}` | POST | Cancel order |
| `/User/ReorderItems/{id}` | POST | Reorder items |

**Dependencies:**
- `IOrderQueryService`
- `IOrderCommandService`

**Features:**
- Order history
- Order tracking
- Reorder functionality
- Invoice download

---

### 18. UserProfileController
**File:** `Presentation/Controllers/User/UserProfileController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "UserOrHigher")]`

**Purpose:** Manage user profile

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/User/Profile` | GET | View profile |
| `/User/Profile/Edit` | POST | Update profile |
| `/User/Profile/ChangePassword` | POST | Change password |
| `/User/Profile/UpdateAvatar` | POST | Update avatar |

**Dependencies:**
- `IBookQueryService`
- `IUsersService`
- `IActivityLogger`

**Features:**
- Profile editing
- Password change
- Avatar upload
- Account settings

---

## 🦸 SuperAdmin Controllers

### 19. SuperAdminController
**File:** `Presentation/Controllers/SuperAdminController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize]` + Role checks

**Purpose:** System-wide administration

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/SuperAdmin/Dashboard` | GET | SuperAdmin dashboard |
| `/SuperAdmin/ManageUsers` | GET | User management |
| `/SuperAdmin/PendingUsers` | GET | Pending approvals |
| `/SuperAdmin/ActivityLogs` | GET | System logs |
| `/SuperAdmin/SystemSettings` | GET | System settings |
| `/SuperAdmin/SystemHealth` | GET | System health |

**Dependencies:**
- `IUsersService`
- `ISystemSettingsService`
- `IActivityLogger`

**Features:**
- User approval workflow
- Role management
- System configuration
- Health monitoring

---

### 20. SuperAdminDashboardController
**File:** `Presentation/Controllers/SuperAdmin/SuperAdminDashboardController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Roles = "SuperAdmin")]`  
**Route Prefix:** `/SuperAdmin/Dashboard`

**Purpose:** SuperAdmin dashboard with system overview

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/SuperAdmin/Dashboard` | GET | Main dashboard |
| `/SuperAdmin/Dashboard/GetSystemStats` | GET | System statistics |
| `/SuperAdmin/Dashboard/GetHealthMetrics` | GET | Health metrics |

**Dependencies:**
- `IOrderQueryService`
- `IUsersService`
- `IBookQueryService`
- `IActivityLogger`

**Features:**
- System-wide analytics
- User statistics
- Revenue tracking
- Performance metrics

---

## 🎯 Feature Controllers

### 21. BooksController
**File:** `Presentation/Controllers/BooksController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize]` (per-action overrides)

**Purpose:** General book operations across all roles

**Routes:**
| Route | Method | Auth | Description |
|-------|--------|------|-------------|
| `/Books/Details/{id}` | GET | No | Book details |
| `/Books/PublicList` | GET | No | Public book list |
| `/Books/GetBooks` | GET | Yes | AJAX book list |
| `/Books/ToggleFavorite/{id}` | POST | Yes | Toggle favorite |
| `/Books/GetMonthlyBookUploads` | GET | Admin | Chart data |
| `/Books/GetBooksByCategory` | GET | Admin | Chart data |

**Dependencies:**
- `IBookQueryService`
- `IBookCommandService`
- `IBookAnalyticsService`
- `IBookFavoriteService`
- `IReviewService`

**Features:**
- Role-based authorization per action
- Public and authenticated views
- Analytics endpoints
- Review integration

---

### 22. CartController
**File:** `Presentation/Controllers/CartController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize]`

**Purpose:** Shopping cart operations

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Cart` | GET | View cart |
| `/Cart/Add/{id}` | POST | Add to cart |
| `/Cart/Update/{id}` | POST | Update quantity |
| `/Cart/Remove/{id}` | POST | Remove item |
| `/Cart/Clear` | POST | Clear cart |
| `/Cart/Checkout` | GET | Checkout page |
| `/Cart/ProcessCheckout` | POST | Process order |

**Dependencies:**
- `ICartService`
- `IOrderCommandService`

**Features:**
- Session-based cart
- Quantity management
- Price calculation
- Checkout flow

---

### 23. OrderController
**File:** `Presentation/Controllers/OrderController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize]`

**Purpose:** Order processing and management

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Order/Create` | POST | Create order |
| `/Order/Details/{id}` | GET | Order details |
| `/Order/AdminIndex` | GET | Admin order list |
| `/Order/UserOrders` | GET | User order list |

**Dependencies:**
- `BookManagementContext` (direct access)

**Features:**
- Order creation
- Order tracking
- Status updates
- Invoice generation

---

### 24. ReviewController
**File:** `Presentation/Controllers/ReviewController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "UserOrHigher")]`

**Purpose:** Book review operations

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Review/Submit` | POST | Submit review |
| `/Review/Edit/{id}` | POST | Edit review |
| `/Review/Delete/{id}` | POST | Delete review |
| `/Review/GetReviews/{bookId}` | GET | Get book reviews |

**Dependencies:**
- `IReviewService`
- `IActivityLogger`

**Features:**
- Review submission
- Edit own reviews
- Rating system
- Review moderation

---

### 25. CategoryController
**File:** `Presentation/Controllers/CategoryController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize]`

**Purpose:** Category browsing and management

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/Category` | GET | List categories |
| `/Category/Books/{id}` | GET | Books by category |
| `/Category/GetCategories` | GET | AJAX category list |

**Dependencies:**
- `BookManagementContext` (direct access)

**Features:**
- Category listing
- Category filtering
- Book count per category

---

## 🔌 API Controllers

### 26. AdvancedBookSearchController
**File:** `Presentation/Controllers/Api/AdvancedBookSearchController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "UserOrHigher")]`  
**Route Prefix:** `/api/AdvancedBookSearch`

**Purpose:** Advanced search API with complex filters

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/api/AdvancedBookSearch` | POST | Advanced search |

**Dependencies:**
- `IAdvancedBookSearchUseCase`
- `IActivityLogger`

**Request Body:**
```json
{
  "searchTerm": "string",
  "categoryId": 0,
  "minPrice": 0,
  "maxPrice": 0,
  "author": "string",
  "isbn": "string",
  "inStock": true,
  "sortBy": "string",
  "page": 1,
  "pageSize": 12
}
```

**Response:**
```json
{
  "books": [],
  "totalCount": 0,
  "currentPage": 1,
  "totalPages": 0
}
```

---

### 27. CleanBooksController
**File:** `Presentation/Controllers/CleanBooksController.cs`  
**Inherits:** `ControllerBase`  
**Authorization:** `[Authorize]`  
**Route Prefix:** `/api/v1/CleanBooks`

**Purpose:** Clean Architecture API example using Use Cases

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/api/v1/CleanBooks` | POST | Create book |
| `/api/v1/CleanBooks/{id}` | GET | Get book by ID |
| `/api/v1/CleanBooks/search` | POST | Search books |

**Dependencies:**
- `ICreateBookUseCase`
- `IGetBookByIdUseCase`
- `ISearchBooksUseCase`

**Features:**
- RESTful API design
- Use case pattern
- DTO mapping
- Error handling

---

## 🛠️ Utility Controllers

### 28. HealthController
**File:** `Presentation/Controllers/HealthController.cs`  
**Inherits:** `BaseController`  
**Authorization:** `[Authorize(Policy = "AdminOrHigher")]`  
**Route Prefix:** `/api/Health`

**Purpose:** System health monitoring

**Routes:**
| Route | Method | Description |
|-------|--------|-------------|
| `/api/Health` | GET | Health check |
| `/api/Health/Database` | GET | Database health |
| `/api/Health/Services` | GET | Service health |

**Dependencies:**
- `BookManagementContext`

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-29T00:00:00Z",
  "database": "Connected",
  "services": {
    "bookService": "OK",
    "orderService": "OK"
  }
}
```

---

## 📊 Quick Reference Table

| # | Controller | Type | Auth Level | Primary Purpose |
|---|-----------|------|------------|-----------------|
| 1 | BaseController | Base | N/A | Base functionality |
| 2 | HomeController | Public | None | Entry point |
| 3 | PublicController | Public | None | Public browsing |
| 4 | ErrorController | Public | None | Error handling |
| 5 | AuthController | Auth | None | Authentication |
| 6 | AdminController | Admin | Admin+ | Admin main |
| 7 | AdminDashboardController | Admin | Admin+ | Admin dashboard |
| 8 | AdminBookManagementController | Admin | Admin+ | Book CRUD |
| 9 | AdminCategoryManagementController | Admin | Admin+ | Category CRUD |
| 10 | AdminOrderManagementController | Admin | Admin+ | Order management |
| 11 | AdminUserManagementController | Admin | Admin+ | User management |
| 12 | ReviewModerationController | Admin | Admin+ | Review moderation |
| 13 | UserController | User | User+ | User main |
| 14 | UserDashboardController | User | User+ | User dashboard |
| 15 | UserBookBrowsingController | User | User+ | Browse books |
| 16 | UserFavoritesController | User | User+ | Favorites |
| 17 | UserOrdersController | User | User+ | Order history |
| 18 | UserProfileController | User | User+ | Profile |
| 19 | SuperAdminController | SuperAdmin | SuperAdmin | System admin |
| 20 | SuperAdminDashboardController | SuperAdmin | SuperAdmin | SA dashboard |
| 21 | BooksController | Feature | Mixed | Book operations |
| 22 | CartController | Feature | User+ | Shopping cart |
| 23 | OrderController | Feature | User+ | Orders |
| 24 | ReviewController | Feature | User+ | Reviews |
| 25 | CategoryController | Feature | User+ | Categories |
| 26 | AdvancedBookSearchController | API | User+ | Advanced search |
| 27 | CleanBooksController | API | User+ | Clean API |
| 28 | HealthController | Utility | Admin+ | Health check |

---

## 🎯 Controller Selection Guide

### "I need to..."

| Task | Use This Controller |
|------|---------------------|
| Show landing page | `PublicController.Index()` |
| Browse books (no login) | `PublicController.Browse()` |
| Login/Register | `AuthController` |
| Admin dashboard | `AdminDashboardController.Dashboard()` |
| Manage books | `AdminBookManagementController` |
| Manage categories | `AdminCategoryManagementController` |
| Manage orders | `AdminOrderManagementController` |
| Manage users | `AdminUserManagementController` |
| Moderate reviews | `ReviewModerationController` |
| User dashboard | `UserDashboardController.Dashboard()` |
| Browse books (logged in) | `UserBookBrowsingController` |
| Manage favorites | `UserFavoritesController` |
| View order history | `UserOrdersController` |
| Edit profile | `UserProfileController` |
| SuperAdmin tasks | `SuperAdminController` |
| Add to cart | `CartController.Add()` |
| Checkout | `CartController.Checkout()` |
| Write review | `ReviewController.Submit()` |
| Advanced search | `AdvancedBookSearchController` |
| Health check | `HealthController` |

---

## 📈 Controller Statistics

| Metric | Count |
|--------|-------|
| Total Controllers | 26 |
| Base Controllers | 1 |
| Public Controllers | 3 |
| Auth Controllers | 1 |
| Admin Controllers | 6 |
| User Controllers | 5 |
| SuperAdmin Controllers | 2 |
| Feature Controllers | 5 |
| API Controllers | 2 |
| Utility Controllers | 1 |

**Average Lines per Controller:** ~150-200  
**Largest Controller:** `BooksController` (~300 lines)  
**Smallest Controller:** `ErrorController` (~50 lines)

---

## ✅ Architecture Benefits

1. **Single Responsibility** - Each controller has one clear purpose
2. **Security** - Clear authorization boundaries
3. **Maintainability** - Easy to locate and modify features
4. **Testability** - Small, focused controllers are easy to test
5. **Scalability** - Easy to add new features
6. **Team Collaboration** - Multiple developers can work without conflicts

---

**Last Updated:** January 29, 2026  
**Version:** 2.0  
**Status:** ✅ Production Ready
