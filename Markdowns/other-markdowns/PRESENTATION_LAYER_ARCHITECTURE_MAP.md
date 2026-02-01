# Presentation Layer Architecture Map
## Complete Directory Structure & Connections

> **Purpose**: Visual guide to understand how Controllers, Views, ViewModels, and wwwroot resources connect in your OnlineBookManagementSystem

---

## 📊 High-Level Architecture

```
Presentation/
├── Controllers/          → Handle HTTP requests, return Views/JSON
├── ViewModels/          → Data transfer objects for Views
├── Views/               → Razor templates (CSHTML)
├── Middleware/          → Request/Response pipeline
└── wwwroot/             → Static files (CSS, JS, Images)
```

---

## 🎯 Role-Based Structure

### **Admin Role**
```
Controllers/Admin/
├── AdminBookManagementController.cs    → Views/Admin/Books.cshtml
├── AdminCategoryManagementController.cs → Views/Admin/CategoryManagement.cshtml
├── AdminDashboardController.cs         → Views/Admin/Dashboard.cshtml
├── AdminOrderManagementController.cs   → Views/Admin/OrderManagement.cshtml
├── AdminUserManagementController.cs    → Views/Admin/UserList.cshtml
└── ReviewModerationController.cs       → Views/ReviewModeration/Pending.cshtml

ViewModels/Admin/
├── AdminDashboardViewModel.cs          → Used by Dashboard
├── AdminOrderListViewModel.cs          → Used by OrderManagement
├── AdminUsersViewModel.cs              → Used by UserList
└── [10 more ViewModels]

Views/Admin/
├── Dashboard.cshtml                    → Layout: _LayoutAdmin.cshtml
├── Books.cshtml                        → Uses: _BooksGrid.cshtml
├── _BooksGrid.cshtml                   → Partial (AJAX loadable)
├── CreateBook.cshtml                   → Uses: _BookForm.cshtml
├── EditBook.cshtml                     → Uses: _BookForm.cshtml
└── [8 more views]

wwwroot/css/
├── role-based-theme-engine.css         → Admin theme colors
└── role-color-palette-fix.css          → Admin-specific fixes

wwwroot/js/admin/
├── bookManager.js                      → Handles book CRUD
└── test-ajax.js                        → AJAX testing
```

### **User Role**
```
Controllers/User/
├── UserDashboardController.cs          → Views/User/Dashboard.cshtml
├── UserBookBrowsingController.cs       → Views/User/UserBookList.cshtml
├── UserFavoritesController.cs          → Views/User/Favorite.cshtml
├── UserOrdersController.cs             → Views/User/OrderHistory.cshtml
└── UserProfileController.cs            → Views/User/Profile.cshtml

ViewModels/User/
├── UserDashboardViewModel.cs           → Used by Dashboard
├── OrderHistoryViewModel.cs            → Used by OrderHistory
├── ProfileViewModel.cs                 → Used by Profile
└── [4 more ViewModels]

Views/User/
├── Dashboard.cshtml                    → Layout: _LayoutUser.cshtml
├── UserBookList.cshtml                 → Uses: _UserBooksGrid.cshtml
├── _UserBooksGrid.cshtml               → Partial (AJAX loadable)
├── Favorite.cshtml                     → Favorites management
├── OrderHistory.cshtml                 → Order tracking
└── [7 more views]

wwwroot/js/user/
├── userBookManager.js                  → User book browsing
└── cartManager.js                      → Shopping cart logic
```

### **SuperAdmin Role**
```
Controllers/
└── SuperAdminController.cs             → Views/SuperAdmin/*.cshtml

ViewModels/SuperAdmin/
├── SuperAdminDashboardViewModel.cs     → Used by Dashboard
├── SystemSettingsViewModel.cs          → Used by SystemSettings
├── ManageUsersViewModel.cs             → Used by ManageUsers
└── [3 more ViewModels]

Views/SuperAdmin/
├── Dashboard.cshtml                    → Layout: _LayoutSuperAdmin.cshtml
├── SystemSettings.cshtml               → System configuration
├── ManageUsers.cshtml                  → User management
├── ActivityLogs.cshtml                 → System logs
└── [2 more views]
```

### **Public/Guest Role**
```
Controllers/
├── PublicController.cs                 → Views/Public/*.cshtml
└── HomeController.cs                   → Views/Home/*.cshtml

ViewModels/Showcase/
├── ShowcaseViewModel.cs                → Used by Public/Index
├── InteractiveDemoViewModel.cs         → Used by InteractiveDemo
└── [2 more ViewModels]

Views/Public/
├── Index.cshtml                        → Layout: _LayoutPublic.cshtml
├── Dashboard.cshtml                    → Public dashboard
├── Browse.cshtml                       → Book browsing
├── BookDetails.cshtml                  → Book details
├── InteractiveDemo.cshtml              → Demo features
└── [3 more views]

wwwroot/css/
└── public-view-enhancements.css        → Public-specific styles
```

### **Authentication**
```
Controllers/
└── AuthController.cs                   → Views/Auth/*.cshtml

ViewModels/AuthViewModels/
├── LoginViewModel.cs                   → Used by Login
├── RegisterViewModel.cs                → Used by Registration
├── ForgotPasswordViewModel.cs          → Used by ForgotPassword
└── [3 more ViewModels]

Views/Auth/
├── Login.cshtml                        → Layout: _LayoutAuth.cshtml
├── Registration.cshtml                 → User signup
├── ForgotPassword.cshtml               → Password recovery
└── [3 more views]

wwwroot/css/
└── auth.css                            → Authentication page styles

wwwroot/js/Auth/
└── auth.js                             → Login/Register logic
```

---

## 🔗 Controller → View → ViewModel Connections

### **Example 1: Admin Book Management**
```
Flow:
1. User visits: /Admin/Books
2. AdminBookManagementController.Books() executes
3. Loads: BookListViewModel (from ViewModels/Books/)
4. Returns: Views/Admin/Books.cshtml
5. Books.cshtml uses: _LayoutAdmin.cshtml
6. Books.cshtml includes: _BooksGrid.cshtml (partial)
7. _BooksGrid.cshtml loops through: BookDto objects
8. Styles from: wwwroot/css/booksindex.css
9. Scripts from: wwwroot/js/admin/bookManager.js

AJAX Flow:
1. User clicks pagination
2. JavaScript calls: ajaxWrapper.load()
3. Request to: /Admin/Books?page=2
4. Controller checks: X-Requested-With header
5. Returns: PartialView("_BooksGrid", model)
6. JavaScript injects HTML into: #books-content
```

### **Example 2: User Dashboard**
```
Flow:
1. User visits: /User/Dashboard
2. UserDashboardController.Index() executes
3. Loads: UserDashboardViewModel
4. Returns: Views/User/Dashboard.cshtml
5. Dashboard.cshtml uses: _LayoutUser.cshtml
6. Includes: _CartWidget.cshtml (shared partial)
7. Styles from: role-based-theme-engine.css
8. Scripts from: wwwroot/js/user/userBookManager.js
```

### **Example 3: Public Book Browse**
```
Flow:
1. Guest visits: /Public/Browse
2. PublicController.Browse() executes
3. Loads: BookListViewModel
4. Returns: Views/Public/Browse.cshtml
5. Browse.cshtml uses: _LayoutPublic.cshtml
6. Styles from: public-view-enhancements.css
7. No authentication required
```

---

## 📦 Shared Components

### **Layouts** (Views/Shared/)
```
_LayoutAdmin.cshtml          → Used by: Admin views
├── Includes: role-based-theme-engine.css
├── Includes: components.css (NEW)
├── Includes: ajaxWrapper.js (NEW)
└── Sidebar navigation for Admin

_LayoutUser.cshtml           → Used by: User views
├── Includes: role-based-theme-engine.css
├── Includes: _CartWidget.cshtml
└── User-specific navigation

_LayoutSuperAdmin.cshtml     → Used by: SuperAdmin views
├── Includes: role-based-theme-engine.css
└── SuperAdmin navigation

_LayoutPublic.cshtml         → Used by: Public views
├── Includes: public-view-enhancements.css
└── Public navigation (no auth)

_LayoutAuth.cshtml           → Used by: Auth views
├── Includes: auth.css
└── Minimal layout for login/register
```

### **Reusable Partials** (Views/Shared/)
```
_Notification.cshtml         → Toast notifications (all pages)
_CartWidget.cshtml           → Shopping cart widget (User layout)
_ValidationScriptsPartial.cshtml → Form validation (forms)

Error.cshtml                 → 500 errors
NotFound.cshtml              → 404 errors
Unauthorized.cshtml          → 401/403 errors
SessionExpired.cshtml        → Session timeout
```

### **NEW: Atomic Components** (Views/Shared/Components/)
```
_BookCard.cshtml             → Single book card (reusable)
_BookGrid.cshtml             → Grid of book cards
_LoadingState.cshtml         → Loading spinner
_EmptyState.cshtml           → No data display
_Pagination.cshtml           → Pagination controls
_ErrorAlert.cshtml           → Error messages
```

---

## 🎨 CSS Architecture

### **Core Styles**
```
wwwroot/css/core/
└── variables.css            → CSS variables (colors, spacing, etc.)

wwwroot/css/components/
├── book-card.css            → Book card component styles
├── empty-state.css          → Empty state styles
├── loading-state.css        → Loading indicator styles
├── pagination.css           → Pagination styles
└── error-alert.css          → Error display styles

wwwroot/css/
├── components.css           → Master import file (imports all above)
├── role-based-theme-engine.css → Role-specific theming
├── role-color-palette-fix.css  → Color fixes
├── ui-consistency-fixes.css    → UI consistency
└── accessibility-enhancements.css → A11y improvements
```

### **Page-Specific Styles**
```
auth.css                     → Login/Register pages
booksindex.css               → Book listing pages
cartstylesheet.css           → Shopping cart
public-view-enhancements.css → Public pages
toast-notifications.css      → Toast messages
```

### **CSS Loading Order** (in Layouts)
```html
<!-- 1. Core Variables -->
<link href="~/css/core/variables.css" />

<!-- 2. Component Styles -->
<link href="~/css/components.css" />

<!-- 3. Role-Based Theme -->
<link href="~/css/role-based-theme-engine.css" />

<!-- 4. Page-Specific (optional) -->
@RenderSection("Styles", required: false)
```

---

## 📜 JavaScript Architecture

### **Core Scripts**
```
wwwroot/js/core/
├── ajaxWrapper.js           → Global AJAX handler (NEW)
├── apiClient.js             → API communication
├── notifications.js         → Toast notifications
└── urlStateManager.js       → URL state management
```

### **Role-Specific Scripts**
```
wwwroot/js/admin/
├── bookManager.js           → Admin book CRUD
└── test-ajax.js             → AJAX testing

wwwroot/js/user/
├── userBookManager.js       → User book browsing
└── cartManager.js           → Shopping cart

wwwroot/js/Auth/
└── auth.js                  → Login/Register

wwwroot/js/Books/
└── ChartsAdmin.js           → Admin dashboard charts
```

### **Shared Scripts**
```
wwwroot/js/
├── site.js                  → Global site scripts
├── role-switcher.js         → Role switching
├── cart-utils.js            → Cart utilities
└── unified-interactions.js  → Common interactions
```

### **JavaScript Loading Order** (in Layouts)
```html
<!-- 1. jQuery (required) -->
<script src="jquery-3.6.0.min.js"></script>

<!-- 2. Core AJAX Wrapper -->
<script src="~/js/core/ajaxWrapper.js"></script>

<!-- 3. Page-Specific Scripts -->
@RenderSection("Scripts", required: false)

<!-- 4. Shared Scripts -->
<script src="~/js/site.js"></script>
<script src="~/js/role-switcher.js"></script>
```

---

## 🔄 Data Flow Patterns

### **Pattern 1: Full Page Load**
```
Browser Request
    ↓
Controller Action
    ↓
Service Layer (Infrastructure/)
    ↓
Repository (Infrastructure/Data/)
    ↓
Database
    ↓
Entity → DTO → ViewModel
    ↓
View (CSHTML)
    ↓
HTML Response
```

### **Pattern 2: AJAX Partial Load** (NEW)
```
JavaScript Event (click, input, etc.)
    ↓
ajaxWrapper.load({ url, container })
    ↓
AJAX Request (X-Requested-With: XMLHttpRequest)
    ↓
Controller checks Request.Headers
    ↓
Returns PartialView() instead of View()
    ↓
HTML Fragment Response
    ↓
JavaScript injects into container
    ↓
Re-initialize validation if needed
```

### **Pattern 3: Form Submission**
```
User fills form
    ↓
ajaxWrapper.submit({ form })
    ↓
Validates form (jQuery Validation)
    ↓
POST Request with FormData
    ↓
Controller validates ModelState
    ↓
Service processes data
    ↓
Returns success/error
    ↓
Shows notification
    ↓
Refreshes partial view
```

---

## 🗺️ Complete File Mapping

### **Admin Book Management**
```
Controller:  AdminBookManagementController.cs
Actions:     Books(), Create(), Edit(), Delete()
ViewModels:  BookListViewModel, BookFormViewModel
Views:       Admin/Books.cshtml, Admin/_BooksGrid.cshtml
Partials:    Components/_BookCard.cshtml, Components/_Pagination.cshtml
CSS:         booksindex.css, components/book-card.css
JavaScript:  admin/bookManager.js, core/ajaxWrapper.js
Layout:      _LayoutAdmin.cshtml
```

### **User Shopping Cart**
```
Controller:  CartController.cs
Actions:     Index(), AddToCart(), RemoveFromCart(), Checkout()
ViewModels:  CartViewModel, CheckOutViewModel
Views:       User/UserCart.cshtml, Cart/CheckOut.cshtml
Partials:    _CartWidget.cshtml
CSS:         cartstylesheet.css
JavaScript:  user/cartManager.js, cart-utils.js
Layout:      _LayoutUser.cshtml
```

### **Public Book Browse**
```
Controller:  PublicController.cs
Actions:     Browse(), BookDetails()
ViewModels:  BookListViewModel, BookDetailsViewModel
Views:       Public/Browse.cshtml, Public/BookDetails.cshtml
Partials:    Components/_BookGrid.cshtml, Components/_BookCard.cshtml
CSS:         public-view-enhancements.css, components/book-card.css
JavaScript:  core/ajaxWrapper.js
Layout:      _LayoutPublic.cshtml
```

### **Authentication**
```
Controller:  AuthController.cs
Actions:     Login(), Register(), ForgotPassword()
ViewModels:  LoginViewModel, RegisterViewModel
Views:       Auth/Login.cshtml, Auth/Registration.cshtml
CSS:         auth.css
JavaScript:  Auth/auth.js
Layout:      _LayoutAuth.cshtml
```

---

## 🎯 Quick Reference: Find Your Files

### **"I need to modify the Admin Dashboard"**
```
Controller:  Controllers/Admin/AdminDashboardController.cs
ViewModel:   ViewModels/Admin/AdminDashboardViewModel.cs
View:        Views/Admin/Dashboard.cshtml
Layout:      Views/Shared/_LayoutAdmin.cshtml
CSS:         wwwroot/css/role-based-theme-engine.css
JavaScript:  wwwroot/js/Books/ChartsAdmin.js
```

### **"I need to add a new book card style"**
```
Component:   Views/Shared/Components/_BookCard.cshtml
CSS:         wwwroot/css/components/book-card.css
Variables:   wwwroot/css/core/variables.css
```

### **"I need to fix AJAX loading"**
```
JavaScript:  wwwroot/js/core/ajaxWrapper.js
Component:   Views/Shared/Components/_LoadingState.cshtml
CSS:         wwwroot/css/components/loading-state.css
```

### **"I need to change the color scheme"**
```
Variables:   wwwroot/css/core/variables.css
Theme:       wwwroot/css/role-based-theme-engine.css
Fixes:       wwwroot/css/role-color-palette-fix.css
```

### **"I need to add a new admin page"**
```
1. Create Controller in: Controllers/Admin/
2. Create ViewModel in: ViewModels/Admin/
3. Create View in: Views/Admin/
4. Use Layout: _LayoutAdmin.cshtml
5. Add CSS (if needed): wwwroot/css/
6. Add JS (if needed): wwwroot/js/admin/
```

---

## 📊 Statistics

```
Controllers:     25 files
ViewModels:      60+ files
Views:           70+ files
CSS Files:       15+ files
JS Files:        15+ files
Layouts:         5 files
Components:      6 files (NEW)
Middleware:      3 files
```

---

## 🎉 Summary

Your Presentation layer follows a **clean, role-based architecture** with:

✅ **Separation of Concerns**: Controllers → ViewModels → Views  
✅ **Role-Based Organization**: Admin, User, SuperAdmin, Public  
✅ **Reusable Components**: Atomic CSHTML partials  
✅ **Modular CSS**: BEM methodology with CSS variables  
✅ **AJAX-Ready**: Global wrapper with automatic loading states  
✅ **Consistent Theming**: Role-based color system  

**Next Steps**: Use this map to navigate your codebase efficiently and implement new features following the established patterns!
