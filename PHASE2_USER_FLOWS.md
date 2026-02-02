# PHASE 2: USER FLOWS - COMPLETE FLOW DOCUMENTATION

**Status**: IN PROGRESS  
**Date Started**: 2025-01-30  
**Purpose**: Document every user flow from entry point to completion

---

## 🎯 FLOW DOCUMENTATION RULES

- ✅ **Trace actual code paths** - Follow the real implementation
- ✅ **Document decision points** - Where flows branch
- ✅ **Mark side effects** - DB writes, emails, cache operations
- ✅ **Identify violations** - Where flows break architecture

---

## 1️⃣ PUBLIC (UNAUTHENTICATED) FLOW

### Entry Point
- **URL**: `/` or `/Public/Index`
- **Default Route**: `{controller=Public}/{action=Index}`
- **Middleware**: RequestLogging → Session → Authentication → RoleSwitching → Authorization

### Flow Steps

#### 1.1 Landing Page (`/Public/Dashboard`)
**Controller**: `PublicController.Dashboard()`

**Steps**:
1. Check if user is authenticated → `IRoleBasedRedirectionService.ShouldBypassPublicArea(User)`
2. If authenticated → Redirect to role-based dashboard
3. If not authenticated → Continue
4. Get showcase content → `IPublicDemoService.GetShowcaseContentAsync()`
5. Return view with showcase data

**Side Effects**:
- Cache reads (showcase content)
- No database writes

**Exit Points**:
- Authenticated user → Redirect to `/User/Dashboard`, `/Admin/Dashboard`, or `/SuperAdmin/Dashboard`
- Unauthenticated user → View showcase page

---

#### 1.2 Browse Books (`/Public/Browse`)
**Controller**: `PublicController.Browse(...)`

**Steps**:
1. Check authentication → Redirect if authenticated
2. Apply filters (search, category, price, sort)
3. Get books → `IPublicDemoService.GetBooksByCategoryAsync()` or `SearchBooksAsync()` or `GetFeaturedBooksAsync()`
4. Get categories → `IPublicDemoService.GetCategoriesWithCountsAsync()`
5. Return paginated book list

**Side Effects**:
- Cache reads (5-10 min cache)
- No database writes

**Query Parameters**:
- `page` (default: 1)
- `search` (optional)
- `categoryId` (optional)
- `sortBy` (default: "title")
- `minPrice`, `maxPrice` (optional)

---

#### 1.3 Book Details (`/Public/BookDetails/{id}`)
**Controller**: `PublicController.BookDetails(int id)`

**Steps**:
1. Check authentication → Redirect if authenticated
2. Get book details → `IPublicDemoService.GetBookDetailsAsync(id)`
3. Return book details view

**Side Effects**:
- Cache reads
- No database writes

---

#### 1.4 Search Books (AJAX) (`/Public/SearchBooks`)
**Controller**: `PublicController.SearchBooks(string query, int page)`

**Steps**:
1. Check cache → `IMemoryCache.TryGetValue("search_{query}_{page}")`
2. If cached → Return cached result
3. If not cached → `IPublicDemoService.SearchBooksAsync(query, page, 12)`
4. Cache result (5 minutes)
5. Return JSON

**Side Effects**:
- Cache reads/writes
- No database writes

---

#### 1.5 Registration Flow (`/Auth/Registration` → `/Auth/SaveData`)
**Controller**: `AuthController`

**Steps**:
1. **GET `/Auth/Registration`**
   - Show registration form with role capabilities
   - Display onboarding info
   - Show system stats

2. **POST `/Auth/SaveData`**
   - Validate model → `ModelState.IsValid`
   - Register user → `IAuthService.RegisterUserAsync(RegisterViewModel)`
     - Creates user with `IsPendingApproval = true`
     - Generates email confirmation token
     - Sends confirmation email
   - Return success with redirect to login

**Side Effects**:
- Database write (User entity created)
- Email send (confirmation email)
- Activity log (registration)

**Exit Points**:
- Success → Redirect to `/Auth/Login` with message
- Failure → Return JSON error

---

#### 1.6 Email Confirmation (`/Auth/ConfirmEmail`)
**Controller**: `AuthController.ConfirmEmail(string token, string email)`

**Steps**:
1. Validate token → `IAuthService.ConfirmEmailAsync(token, email)`
2. Set `EmailConfirmed = true`
3. Clear confirmation token
4. Show confirmation page

**Side Effects**:
- Database write (User.EmailConfirmed = true)
- No email send

**Exit Points**:
- Success → Show confirmation page
- Failure → Show error page

---

## 2️⃣ USER (AUTHENTICATED) FLOW

### Entry Point
- **URL**: `/User/Dashboard` (after login)
- **Authentication**: Required (`[Authorize(Policy = "UserOrHigher")]`)
- **Middleware**: All middleware + Authorization check

### Flow Steps

#### 2.1 Login Flow (`/Auth/Login` → `/Auth/LoginData`)
**Controller**: `AuthController`

**Steps**:
1. **GET `/Auth/Login`**
   - Show login form
   - Display role capabilities
   - Show system stats

2. **POST `/Auth/LoginData`**
   - Validate user → `IAuthService.ValidateUserAsync(LoginViewModel)`
     - Check user exists
     - Check not deleted
     - Check not pending approval
     - Check email confirmed
     - Check not locked out
     - Validate password
   - Generate tokens → `IAuthService.GenerateTokensAsync(user)`
     - Create JWT access token
     - Create refresh token
     - Store refresh token in DB (hashed)
   - Get user roles → `IAuthService.GetUserRolesAsync(userId)`
   - Determine redirect URL based on role:
     - SuperAdmin → `/SuperAdmin/Dashboard`
     - Admin → `/Admin/Dashboard`
     - User → `/User/Dashboard`
   - Set access token cookie (HttpOnly, Secure, SameSite=Strict)
   - Log activity → `IActivityLogger.LogAsync("Login", ...)`
   - Return JSON with tokens and redirect URL

**Side Effects**:
- Database read (user validation)
- Database write (refresh token, last login date)
- Cookie write (access token)
- Activity log

**Exit Points**:
- Success → Redirect to role-based dashboard
- Failure → Return JSON error

---

#### 2.2 User Dashboard (`/User/Dashboard`)
**Controller**: `UserController.Dashboard()`

**Steps**:
1. Get user ID from claims
2. Get dashboard data → `GetUserDashboardDataAsync(userId)`
   - Total books count
   - Favorites count
   - Orders count
   - Cart items count
   - Total spent
   - Featured books
   - Recent orders
   - Categories
   - Recommended books
   - New arrivals
3. Log activity → `IActivityLogger.LogAsync("Dashboard", ...)`
4. Return dashboard view

**Side Effects**:
- Database reads (multiple queries)
- Cache reads (recommendations)
- Activity log

---

#### 2.3 Browse Books (`/User/UserBookList`)
**Controller**: `UserController.UserBookList(...)`

**Steps**:
1. Get user ID from claims
2. Apply filters (search, category, price, sort)
3. Get books → `IBookQueryService.GetBooksForUserAsync(...)`
   - Includes favorite status for user
4. Get categories → `ICategoryInterface.GetCategoriesForDropdownAsync()`
5. Log activity → `IActivityLogger.LogAsync("BrowseBooks", ...)`
6. Return book list view

**Side Effects**:
- Database reads
- Activity log

---

#### 2.4 Book Details (`/User/BookDetails/{id}`)
**Controller**: `UserController.BookDetails(int id)`

**Steps**:
1. Get user ID from claims
2. Get book details → `IBookQueryService.GetBookDetailsForUserAsync(id, userId)`
   - Includes favorite status
   - Includes review eligibility
3. Log activity → `IActivityLogger.LogAsync("ViewBook", ...)`
4. Return book details view

**Side Effects**:
- Database reads
- Activity log

---

#### 2.5 Add to Favorites (`/User/ToggleFavorite`)
**Controller**: `UserController.ToggleFavorite([FromBody] ToggleFavoriteRequest)`

**Steps**:
1. Get user ID from claims
2. Toggle favorite → `IBookFavoriteService.ToggleUserFavoriteAsync(bookId, userId)`
   - Check if favorite exists
   - If exists → Remove from UserFavorites
   - If not exists → Add to UserFavorites
3. Log activity → `IActivityLogger.LogAsync("ToggleFavorite", ...)`
4. Return JSON result

**Side Effects**:
- Database write (UserFavorites table)
- Activity log

---

#### 2.6 Add to Cart (`/User/AddToCart`)
**Controller**: `UserController.AddToCart([FromBody] AddToCartRequest)`

**Steps**:
1. Get user ID from claims
2. Add to cart → `ICartService.AddToCartAsync(userId, bookId, quantity)`
   - Validate book exists and not deleted
   - Validate stock availability
   - Check if item already in cart
   - If exists → Update quantity
   - If not exists → Create cart item
   - Clear cart cache
3. Log activity → `IActivityLogger.LogAsync("AddToCart", ...)`
4. Return JSON with cart count

**Side Effects**:
- Database write (ShoppingCart table)
- Cache clear (`cart_{userId}`)
- Activity log

---

#### 2.7 View Cart (`/User/UserCart`)
**Controller**: `UserController.UserCart()`

**Steps**:
1. Get user ID from claims
2. Get cart → `ICartService.GetUserCartAsync(userId)`
   - Check cache first
   - If not cached → Get from repository with books and categories
   - Cache for 5 minutes
3. Get cart summary → `ICartService.GetCartSummaryAsync(userId)`
4. Return cart view

**Side Effects**:
- Database reads
- Cache reads/writes

---

#### 2.8 Checkout Flow (`/Cart/Checkout` → `/Cart/ProcessCheckout`)
**Controller**: `CartController`

**Steps**:
1. **GET `/Cart/Checkout`**
   - Get user ID from claims
   - Get checkout details → `ICartService.CheckoutDetailsAsync(userId)`
     - Get cart items
     - Get cart summary
   - Return checkout form

2. **POST `/Cart/ProcessCheckout`**
   - Validate model → `ModelState.IsValid`
   - Get user ID from claims
   - Process checkout → `ICartService.ProcessCheckoutAsync(userId, request)`
     - 🔴 **CRITICAL ISSUE**: Currently only clears cart, doesn't create order!
     - Should call `IOrderCommandService.CreateOrderAsync()` but doesn't
   - Clear cart cache
   - Log activity
   - Redirect to order confirmation

**Side Effects**:
- Database write (cart cleared - soft delete)
- Cache clear
- Activity log
- 🔴 **MISSING**: Order creation

**Exit Points**:
- Success → Redirect to `/Cart/OrderConfirmation`
- Failure → Return checkout view with error

**🔴 CRITICAL VIOLATION**: `ProcessCheckoutAsync()` doesn't create order, only clears cart!

---

#### 2.9 Order History (`/User/OrderHistory`)
**Controller**: `UserController.OrderHistory(...)`

**Steps**:
1. Get user ID from claims
2. Apply filters (status, date range)
3. Get order history → `IOrderQueryService.GetUserOrderHistoryAsync(userId, page, pageSize, status, dateFrom, dateTo)`
   - Gets orders from repository
   - Applies filters in-memory
   - Maps to ViewModel
4. Log activity → `IActivityLogger.LogAsync("ViewOrderHistory", ...)`
5. Return order history view

**Side Effects**:
- Database reads
- Activity log

---

#### 2.10 Order Details (`/User/OrderDetails/{id}`)
**Controller**: `UserController.OrderDetails(int id)`

**Steps**:
1. Get user ID from claims
2. Get order → `IOrderQueryService.GetUserOrderDetailsAsync(id, userId)`
   - Validates ownership (userId must match)
3. Log activity → `IActivityLogger.LogAsync("ViewOrderDetails", ...)`
4. Return order details view

**Side Effects**:
- Database reads
- Activity log

---

#### 2.11 Cancel Order (`/User/CancelOrder`)
**Controller**: `UserController.CancelOrder([FromBody] CancelOrderRequest)`

**Steps**:
1. Get user ID from claims
2. Cancel order → `IOrderCommandService.CancelOrderAsync(orderId, userId)`
   - Begin transaction
   - Get order with details
   - Validate can be cancelled (Pending or Processing)
   - Set status to Cancelled
   - Set payment status to Refunded
   - Restore stock quantities
   - Commit transaction
3. Log activity → `IActivityLogger.LogAsync("CancelOrder", ...)`
4. Return JSON result

**Side Effects**:
- Database writes (order status, payment status, stock restoration)
- Transaction management
- Activity log

---

#### 2.12 Submit Review (`/Review/Submit`)
**Controller**: `ReviewController.Submit(ReviewSubmissionViewModel)`

**Steps**:
1. Validate model → `ModelState.IsValid`
2. Get user ID from claims
3. Submit review → `IReviewService.SubmitReviewAsync(userId, bookId, rating, reviewText)`
   - Check if user already has review
   - Create BookReview entity
   - Set status to Pending
   - Recalculate book rating
4. Redirect to book details

**Side Effects**:
- Database write (BookReview entity)
- Database write (Book.AverageRating update)
- Activity log (via service)

---

## 3️⃣ ADMIN FLOW

### Entry Point
- **URL**: `/Admin/Dashboard` (after login as Admin)
- **Authentication**: Required (`[Authorize(Policy = "AdminOrHigher")]`)

### Flow Steps

#### 3.1 Admin Dashboard (`/Admin/Dashboard`)
**Controller**: `AdminController.Dashboard()`

**Steps**:
1. Get user ID from claims
2. Get dashboard data → `GetAdminDashboardDataAsync(userId)`
   - Total books count
   - Total orders count
   - Total users count
   - Total categories count
   - Recent activities (today's logs)
   - Monthly stats (uploads, category distribution, author distribution, favorites)
3. Log activity → `IActivityLogger.LogAsync("Dashboard", ...)`
4. Return admin dashboard view

**Side Effects**:
- Database reads (multiple queries)
- Activity log

---

#### 3.2 Book Management (`/Admin/Books`)
**Controller**: `AdminController.Books(...)`

**Steps**:
1. Get user ID from claims
2. Apply filters (search, category, sort, inStock)
3. Get books → `IBookQueryService.GetPaginatedBooksAsync(...)`
4. Get categories → `ICategoryInterface.GetCategoriesForDropdownAsync()`
5. Log activity → `IActivityLogger.LogAsync("ViewBooks", ...)`
6. Return books view (supports AJAX partial view)

**Side Effects**:
- Database reads
- Activity log

---

#### 3.3 Create Book (`/Admin/CreateBook` → POST)
**Controller**: `AdminController.CreateBook(...)`

**Steps**:
1. **GET `/Admin/CreateBook`**
   - Get view model → `IBookQueryService.GetCreateBookViewModelAsync()`
   - Return create form

2. **POST `/Admin/CreateBook`**
   - Validate model → `ValidateBookModel(model)`
   - Create book → `IBookCommandService.AddBookAsync(book, imageFile)`
     - Begin transaction
     - Validate input
     - Set timestamps
     - Add book to context
     - Save to get ID
     - Save image (resize to 400x600, JPEG 85%)
     - Update book with image URL
     - Commit transaction
   - Log activity → `IActivityLogger.LogAsync("CreateBook", ...)`
   - Redirect to books list

**Side Effects**:
- Database write (Book entity)
- File system write (book image)
- Transaction management
- Activity log

---

#### 3.4 Update Book (`/Admin/EditBook/{id}` → POST)
**Controller**: `AdminController.EditBook(...)`

**Steps**:
1. **GET `/Admin/EditBook/{id}`**
   - Get view model → `IBookQueryService.GetEditBookViewModelAsync(id)`
   - Return edit form

2. **POST `/Admin/EditBook/{id}`**
   - Validate model
   - Update book → `IBookCommandService.UpdateBookAsync(book, imageFile)`
     - Begin transaction
     - Get existing book
     - Handle image update (if provided)
     - Update book properties
     - Delete old image (after successful transaction)
     - Commit transaction
   - Log activity → `IActivityLogger.LogAsync("UpdateBook", ...)`
   - Redirect to books list

**Side Effects**:
- Database write (Book entity)
- File system write (new image)
- File system delete (old image)
- Transaction management
- Activity log

---

#### 3.5 Delete Book (`/Admin/DeleteBook/{id}`)
**Controller**: `AdminController.DeleteBook(int id)`

**Steps**:
1. Get user ID from claims
2. Soft delete → `IBookCommandService.SoftDeleteBookAsync(id, userId)`
   - Set `IsDeleted = true`
   - Update timestamp
3. Log activity → `IActivityLogger.LogAsync("DeleteBook", ...)`
4. Return JSON result

**Side Effects**:
- Database write (soft delete)
- Activity log

---

#### 3.6 Order Management (`/Admin/OrderManagement`)
**Controller**: `AdminController.OrderManagement(...)`

**Steps**:
1. Get user ID from claims
2. Apply filters (search, status, date range)
3. Get orders → `IOrderQueryService.GetOrdersForAdminAsync(page, pageSize, search, status, dateFrom, dateTo)`
4. Log activity → `IActivityLogger.LogAsync("ViewOrders", ...)`
5. Return order management view

**Side Effects**:
- Database reads
- Activity log

---

#### 3.7 Update Order Status (`/Admin/UpdateOrderStatus`)
**Controller**: `AdminController.UpdateOrderStatus(int orderId, string status)`

**Steps**:
1. Get user ID from claims
2. Update status → `IOrderCommandService.UpdateOrderStatusAsync(orderId, status, userId)`
   - Begin transaction
   - Get order with details
   - Update status
   - Update payment status (if Delivered → Paid, if Cancelled → Refunded)
   - Restore stock (if cancelled)
   - Commit transaction
3. Log activity → `IActivityLogger.LogAsync("UpdateOrderStatus", ...)`
4. Return JSON result

**Side Effects**:
- Database writes (order status, payment status, stock restoration)
- Transaction management
- Activity log

---

#### 3.8 Category Management (`/Admin/CategoryManagement`)
**Controller**: `AdminController.CategoryManagement()`

**Steps**:
1. Get user ID from claims
2. Get categories → `ICategoryInterface.GetAllCategoriesAsync()`
3. Log activity → `IActivityLogger.LogAsync("ViewCategories", ...)`
4. Return category management view

**Side Effects**:
- Database reads
- Activity log

---

#### 3.9 Create Category (`/Admin/CreateCategory`)
**Controller**: `AdminController.CreateCategory([FromBody] CreateCategoryRequest)`

**Steps**:
1. Get user ID from claims
2. Create category → `ICategoryInterface.CreateCategoryAsync(name, description, userId)`
3. Log activity → `IActivityLogger.LogAsync("CreateCategory", ...)`
4. Return JSON result

**Side Effects**:
- Database write (Category entity)
- Activity log

---

## 4️⃣ SUPERADMIN FLOW

### Entry Point
- **URL**: `/SuperAdmin/Dashboard` (after login as SuperAdmin)
- **Authentication**: Required (`[Authorize(Policy = "SuperAdminOnly")]`)

### Flow Steps

#### 4.1 SuperAdmin Dashboard (`/SuperAdmin/Dashboard`)
**Controller**: `SuperAdminController.Dashboard()`

**Steps**:
1. Get user ID from claims
2. Get dashboard data → `IUsersService.GetSuperAdminDashboardDataAsync()`
   - Total users, new users today
   - Total books, books added this month
   - Total orders, orders today
   - Total revenue, revenue today
   - Storage usage percent
   - Active sessions
   - Recent activities
3. Get system settings → `ISystemSettingsService.GetSystemSettingsAsync()`
4. Get pending users → `IUsersService.GetPendingUsersAsync()`
5. Build enhanced view model
6. Log activity → `IActivityLogger.LogAsync("Dashboard", ...)`
7. Return super admin dashboard view

**Side Effects**:
- Database reads (multiple queries)
- Activity log

---

#### 4.2 User Management (`/SuperAdmin/ManageUsers`)
**Controller**: `SuperAdminController.ManageUsers(...)`

**Steps**:
1. Get user ID from claims
2. Apply filters (search, role, status)
3. Get users → `IUsersService.GetManageUsersDataAsync(page, pageSize, search, role, status)`
4. Log activity → `IActivityLogger.LogAsync("ManageUsers", ...)`
5. Return user management view

**Side Effects**:
- Database reads
- Activity log

---

#### 4.3 Approve User (`/SuperAdmin/ApproveUser`)
**Controller**: `SuperAdminController.ApproveUser(int userId, string role)`

**Steps**:
1. Get admin ID from claims
2. Approve user → `IUsersService.ApproveUserAsync(userId, role)`
   - Get user
   - Validate role exists
   - Generate confirmation token
   - Hash token (SHA256)
   - Set `IsPendingApproval = false`
   - Set email confirmation token
   - Assign role
   - Send approval email with confirmation link
3. Log activity → `IActivityLogger.LogAsync("ApproveUser", ...)`
4. Return JSON result

**Side Effects**:
- Database writes (user approval, role assignment, token)
- Email send (approval email)
- Activity log

---

#### 4.4 Reject User (`/SuperAdmin/RejectUser`)
**Controller**: `SuperAdminController.RejectUser(int userId)`

**Steps**:
1. Get admin ID from claims
2. Reject user → `IUsersService.RejectUserAsync(userId)`
   - Soft delete user
   - Set `IsPendingApproval = false`
3. Log activity → `IActivityLogger.LogAsync("RejectUser", ...)`
4. Return JSON result

**Side Effects**:
- Database write (soft delete)
- Activity log

---

#### 4.5 Change User Role (`/SuperAdmin/ChangeUserRole`)
**Controller**: `SuperAdminController.ChangeUserRole(int userId, string newRole)`

**Steps**:
1. Get admin ID from claims
2. Validate role
3. Security checks:
   - Can't change own role
   - Only SuperAdmin can assign SuperAdmin
   - Only SuperAdmin can modify SuperAdmin users
4. Change role → `IUsersService.UpdateUserRoleAsync(userId, newRole)`
5. Log activity → `IActivityLogger.LogAsync("ChangeUserRole", ...)`
6. Return JSON result

**Side Effects**:
- Database writes (role removal, role assignment)
- Activity log

---

#### 4.6 System Settings (`/SuperAdmin/SystemSettings`)
**Controller**: `SuperAdminController.SystemSettings()`

**Steps**:
1. Get user ID from claims
2. Get settings → `ISystemSettingsService.GetSystemSettingsAsync()`
3. Log activity → `IActivityLogger.LogAsync("SystemSettings", ...)`
4. Return system settings view

**Side Effects**:
- Database reads
- Cache reads
- Activity log

---

#### 4.7 Update Settings (`/SuperAdmin/UpdateGeneralSettings`, etc.)
**Controller**: `SuperAdminController.UpdateGeneralSettings(...)`

**Steps**:
1. Get user ID from claims
2. Update settings → `ISystemSettingsService.UpdateGeneralSettingsAsync(request)`
   - 🔴 **ISSUE**: Only updates cache, not database
3. Log activity → `IActivityLogger.LogAsync("UpdateSettings", ...)`
4. Return JSON result

**Side Effects**:
- Cache writes
- 🔴 **MISSING**: Database writes

---

## 5️⃣ AUTHENTICATION FLOW (DETAILED)

### 5.1 Registration → Approval → Login Flow

**Step 1: Registration**
- User fills form at `/Auth/Registration`
- POST to `/Auth/SaveData`
- `IAuthService.RegisterUserAsync()` creates user with:
  - `IsPendingApproval = true`
  - `EmailConfirmed = false`
  - `EmailConfirmationToken` generated
- Email sent with confirmation link
- User redirected to login with "pending approval" message

**Step 2: Email Confirmation**
- User clicks link → `/Auth/ConfirmEmail?token=...&email=...`
- `IAuthService.ConfirmEmailAsync()` validates token
- Sets `EmailConfirmed = true`
- User still cannot login (pending approval)

**Step 3: SuperAdmin Approval**
- SuperAdmin views `/SuperAdmin/PendingUsers`
- Clicks "Approve" → `/SuperAdmin/ApproveUser`
- `IUsersService.ApproveUserAsync()`:
  - Sets `IsPendingApproval = false`
  - Assigns role
  - Generates new confirmation token
  - Sends approval email with confirmation link

**Step 4: Final Confirmation**
- User clicks approval email link
- Confirms email (if not already confirmed)
- User can now login

**Step 5: Login**
- User logs in at `/Auth/Login`
- POST to `/Auth/LoginData`
- `IAuthService.ValidateUserAsync()` checks:
  - User exists ✓
  - Not deleted ✓
  - Not pending approval ✓
  - Email confirmed ✓
  - Not locked out ✓
  - Password correct ✓
- `IAuthService.GenerateTokensAsync()` creates JWT + refresh token
- Cookie set with access token
- Redirect to role-based dashboard

---

### 5.2 Token Refresh Flow

**Step 1: Access Token Expires**
- User makes request with expired token
- Returns 401 Unauthorized

**Step 2: Refresh Token**
- Frontend calls `/Auth/RefreshToken` with refresh token
- `IAuthService.RefreshTokenAsync()`:
  - Validates refresh token (checks hash, expiry)
  - Generates new access token
  - Optionally rotates refresh token
- New access token returned
- Cookie updated

---

### 5.3 Logout Flow

**Step 1: Logout Request**
- User clicks logout → `/Auth/Logout`
- `IAuthService.RevokeRefreshTokensAsync(userId)`:
  - Marks all refresh tokens as inactive
- Cookie deleted
- Redirect to login

**Side Effects**:
- Database write (refresh tokens revoked)
- Cookie deletion

---

## 6️⃣ CART → ORDER FLOW (DETAILED)

### 6.1 Add to Cart
**Flow**: `/User/AddToCart` or `/Cart/AddOrUpdateCart`

**Steps**:
1. User clicks "Add to Cart" on book
2. POST to `/User/AddToCart` or `/Cart/AddOrUpdateCart`
3. `ICartService.AddOrUpdateCartAsync(userId, bookId, quantity)`:
   - Validate book exists and not deleted
   - Validate stock availability
   - Check if item already in cart
   - If exists → Update quantity (validate total doesn't exceed stock)
   - If not exists → Create new cart item
   - Clear cart cache
4. Return JSON with success and cart count

**Side Effects**:
- Database write (ShoppingCart)
- Cache clear

---

### 6.2 View Cart
**Flow**: `/User/UserCart`

**Steps**:
1. User navigates to cart
2. `ICartService.GetUserCartAsync(userId)`:
   - Check cache first
   - If not cached → Get from repository with books and categories
   - Cache for 5 minutes
3. `ICartService.GetCartSummaryAsync(userId)`:
   - Get item count
   - Get total
4. Display cart view

**Side Effects**:
- Database reads
- Cache reads/writes

---

### 6.3 Checkout (CURRENT - INCOMPLETE)
**Flow**: `/Cart/Checkout` → `/Cart/ProcessCheckout`

**Steps**:
1. **GET `/Cart/Checkout`**
   - Get cart items and summary
   - Display checkout form

2. **POST `/Cart/ProcessCheckout`**
   - User fills shipping info
   - `ICartService.ProcessCheckoutAsync(userId, request)`:
     - 🔴 **CRITICAL**: Only clears cart!
     - Does NOT create order
     - Does NOT deduct stock
     - Does NOT create order details
   - Redirect to order confirmation

**🔴 CRITICAL VIOLATION**: Order is never created! Cart is cleared but no order entity is created.

**Expected Flow** (what should happen):
1. Validate cart not empty
2. Validate stock for all items
3. Begin transaction
4. Create Order entity
5. Create OrderDetail entities for each cart item
6. Deduct stock quantities
7. Calculate total
8. Save order
9. Clear cart
10. Commit transaction
11. Send order confirmation email
12. Redirect to order confirmation

---

### 6.4 Order Confirmation
**Flow**: `/Cart/OrderConfirmation`

**Steps**:
1. Display order confirmation page
2. Show order ID (if available)
3. Display success message

**Note**: Currently order ID may be null because order isn't created!

---

## 7️⃣ REVIEW FLOW

### 7.1 Submit Review
**Flow**: `/Review/Submit`

**Steps**:
1. User views book details
2. User writes review (rating 1-5, text 10-1000 chars)
3. POST to `/Review/Submit`
4. `IReviewService.SubmitReviewAsync(userId, bookId, rating, reviewText)`:
   - Check if user already has review
   - Create BookReview entity
   - Set status to Pending
   - Recalculate book rating (average)
   - Update Book.AverageRating
5. Redirect to book details

**Side Effects**:
- Database write (BookReview)
- Database write (Book.AverageRating)
- Activity log

---

### 7.2 Update Review
**Flow**: `/Review/Update/{id}`

**Steps**:
1. User edits existing review
2. POST to `/Review/Update/{id}`
3. `IReviewService.UpdateReviewAsync(reviewId, userId, rating, reviewText)`:
   - Validate ownership
   - Update review
   - Reset status to Pending (requires re-moderation)
   - Recalculate rating
4. Redirect to book details

**Side Effects**:
- Database writes (review update, rating recalculation)
- Activity log

---

### 7.3 Delete Review
**Flow**: `/Review/Delete/{id}`

**Steps**:
1. User deletes own review
2. POST to `/Review/Delete/{id}`
3. `IReviewService.DeleteReviewAsync(reviewId, userId)`:
   - Validate ownership
   - Soft delete review
   - Recalculate rating
4. Redirect to book details

**Side Effects**:
- Database writes (soft delete, rating recalculation)
- Activity log

---

### 7.4 Moderate Review (Admin)
**Flow**: Admin reviews pending reviews

**Steps**:
1. Admin views pending reviews
2. Approve → `IReviewService.ApproveReviewAsync(reviewId, moderatorId)`
3. Reject → `IReviewService.RejectReviewAsync(reviewId, moderatorId, reason)`

**Side Effects**:
- Database writes (review status, moderation info)
- Activity log

---

## 8️⃣ ROLE SWITCHING FLOW (SuperAdmin Only)

### 8.1 Switch to Role View
**Flow**: `/SuperAdmin/SwitchToRole?role=Admin`

**Steps**:
1. SuperAdmin clicks "View as Admin"
2. `SuperAdminController.SwitchToRole(string role)`:
   - Store original role in session
   - Store current view role in session
   - Redirect to appropriate dashboard
3. `RoleSwitchingMiddleware`:
   - Checks if SuperAdmin
   - If viewing as different role, adds "ViewRole" claim
   - BaseController uses ViewRole for layout determination

**Side Effects**:
- Session writes
- Claim modification

---

### 8.2 Return to SuperAdmin
**Flow**: `/SuperAdmin/ReturnToSuperAdmin`

**Steps**:
1. SuperAdmin clicks "Return to SuperAdmin"
2. Clear session data (OriginalRole, CurrentViewRole)
3. Redirect to SuperAdmin dashboard

**Side Effects**:
- Session clears

---

## 🔴 CRITICAL FLOW VIOLATIONS

### 1. Checkout Flow - Order Not Created
**Location**: `RefactoredCartService.ProcessCheckoutAsync()`
**Issue**: Only clears cart, doesn't create order
**Impact**: Users cannot complete purchases
**Fix Required**: Call `IOrderCommandService.CreateOrderAsync()` with cart items

### 2. OrderController - Direct DbContext Access
**Location**: `OrderController` (all methods)
**Issue**: Uses `BookManagementContext` directly instead of services
**Impact**: Bypasses business logic, violates Clean Architecture
**Fix Required**: Use `IOrderQueryService` and `IOrderCommandService`

### 3. SystemSettings - Cache Only Updates
**Location**: `SystemSettingsService.UpdateGeneralSettingsAsync()`
**Issue**: Only updates cache, not database
**Impact**: Settings lost on restart
**Fix Required**: Update SystemSettings table in database

---

## 📊 FLOW SUMMARY TABLE

| Flow | Entry Point | Exit Point | Role | Status |
|------|-------------|------------|------|--------|
| Public Browse | `/Public/Browse` | Book list view | Public | ✅ Working |
| Public Book Details | `/Public/BookDetails/{id}` | Book details view | Public | ✅ Working |
| Registration | `/Auth/Registration` | Login page | Public | ✅ Working |
| Email Confirmation | `/Auth/ConfirmEmail` | Confirmation page | Public | ✅ Working |
| Login | `/Auth/Login` | Role dashboard | All | ✅ Working |
| User Dashboard | `/User/Dashboard` | Dashboard view | User | ✅ Working |
| User Browse | `/User/UserBookList` | Book list view | User | ✅ Working |
| Add to Cart | `/User/AddToCart` | JSON response | User | ✅ Working |
| View Cart | `/User/UserCart` | Cart view | User | ✅ Working |
| Checkout | `/Cart/Checkout` | Order confirmation | User | 🔴 **BROKEN** |
| Order History | `/User/OrderHistory` | Order list view | User | ✅ Working |
| Submit Review | `/Review/Submit` | Book details | User | ✅ Working |
| Admin Dashboard | `/Admin/Dashboard` | Dashboard view | Admin | ✅ Working |
| Create Book | `/Admin/CreateBook` | Books list | Admin | ✅ Working |
| Order Management | `/Admin/OrderManagement` | Order list | Admin | ✅ Working |
| SuperAdmin Dashboard | `/SuperAdmin/Dashboard` | Dashboard view | SuperAdmin | ✅ Working |
| Approve User | `/SuperAdmin/ApproveUser` | JSON response | SuperAdmin | ✅ Working |
| System Settings | `/SuperAdmin/SystemSettings` | Settings view | SuperAdmin | 🟡 Partial |

---

**Last Updated**: 2025-01-30  
**Status**: COMPLETE  
**Ready for Phase 3**: Clean Architecture Realignment
