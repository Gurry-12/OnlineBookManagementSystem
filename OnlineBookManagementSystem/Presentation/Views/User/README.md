# User Views Documentation

This document provides an overview of all User views in the Online Book Management System.

## Views Overview

### 1. Dashboard.cshtml
**Purpose**: Main user dashboard with overview and recommendations
**Controller Action**: `UserController.Dashboard()`
**Features**:
- Inspirational quotes carousel
- New arrivals section (loaded via AJAX)
- Personalized recommendations (loaded via AJAX)
- Statistics overview
- Quick navigation to other sections

**JavaScript Dependencies**:
- `~/js/core/apiClient.js`
- `~/js/core/notifications.js`

### 2. UserBookList.cshtml
**Purpose**: Browse and search books with filters
**Controller Action**: `UserController.UserBookList()`
**Features**:
- Search functionality
- Category filtering
- Price range filtering
- Sorting options
- Pagination
- AJAX-powered book loading

**Partial Views**:
- `_UserBooksGrid.cshtml` - Book grid display

**JavaScript Dependencies**:
- `~/js/core/apiClient.js`
- `~/js/core/notifications.js`
- `~/js/core/urlStateManager.js`
- `~/js/user/userBookManager.js`

### 3. BookDetails.cshtml
**Purpose**: Detailed view of a single book
**Controller Action**: `UserController.BookDetails()` or `UserController.Details()`
**Features**:
- Book information display
- Add to cart functionality
- Favorite toggle
- Stock status
- Quantity selection
- Breadcrumb navigation

### 4. Profile.cshtml
**Purpose**: User profile management
**Controller Action**: `UserController.Profile()`
**Features**:
- Personal information editing
- Address management
- Account statistics
- Password change
- Quick action links

### 5. Favorite.cshtml
**Purpose**: Display user's favorite books
**Controller Action**: `UserController.Favorite()`
**Features**:
- Favorite books grid
- Remove from favorites
- Add to cart from favorites
- Empty state handling

### 6. OrderHistory.cshtml
**Purpose**: Display user's order history with filters
**Controller Action**: `UserController.OrderHistory()`
**Features**:
- Order filtering by status and date
- Order statistics
- Pagination
- Order cancellation (for pending orders)
- Order details navigation

### 7. OrderDetails.cshtml
**Purpose**: Detailed view of a specific order
**Controller Action**: `UserController.OrderDetails()`
**Features**:
- Order status timeline
- Order items display
- Order summary
- Shipping information
- Order actions (cancel, reorder, print)

### 8. UserCart.cshtml
**Purpose**: Shopping cart management
**Controller Action**: `UserController.UserCart()`
**Features**:
- Cart items display
- Quantity updates
- Item removal
- Order summary
- Checkout navigation
- Free shipping indicator

### 9. CategoryClassify.cshtml
**Purpose**: Browse books by categories
**Controller Action**: Not directly linked to UserController
**Features**:
- Category-based book display
- Limited books per category
- View all category books link
- Add to cart functionality

### 10. _UserBooksGrid.cshtml (Partial)
**Purpose**: Reusable book grid component
**Features**:
- Book cards with images
- Stock status indicators
- Favorite toggle buttons
- Add to cart buttons
- Pagination controls

## Shared Components

### _LayoutUser.cshtml
**Purpose**: Main layout for user pages
**Features**:
- Sidebar navigation
- Top bar with user info
- Cart count display
- SuperAdmin return button (when applicable)
- Responsive design

### _Notification.cshtml (Partial)
**Purpose**: Unified notification system
**Features**:
- Bootstrap toast notifications
- Success, error, info, and warning toasts
- TempData message handling
- Consistent styling

## JavaScript Architecture

### Core Scripts
- `apiClient.js` - AJAX request handling
- `notifications.js` - Toast notification system
- `urlStateManager.js` - URL state management

### User-Specific Scripts
- `userBookManager.js` - Book browsing and filtering
- `cartManager.js` - Cart operations

## API Endpoints Used

### User Controller Endpoints
- `GET /User/Dashboard` - Dashboard data
- `GET /User/UserBookList` - Book listing with filters
- `GET /User/BookDetails/{id}` - Book details
- `GET /User/Profile` - User profile
- `POST /User/UpdateProfile` - Update profile
- `GET /User/Favorite` - Favorite books
- `POST /User/ToggleFavorite` - Toggle favorite status
- `GET /User/OrderHistory` - Order history
- `GET /User/OrderDetails/{id}` - Order details
- `GET /User/UserCart` - Cart contents
- `POST /User/AddToCart` - Add item to cart
- `GET /User/GetCartCount` - Cart item count
- `GET /User/GetRecommendations` - Personalized recommendations
- `GET /User/GetNewArrivals` - New arrival books
- `POST /User/CancelOrder` - Cancel order
- `POST /User/ChangePassword` - Change password

### Cart Controller Endpoints
- `POST /Cart/UpdateQuantity` - Update cart item quantity
- `DELETE /Cart/RemoveItem` - Remove cart item

## Styling

### CSS Files
- `~/css/role-based-theme-engine.css` - Main theme
- `~/css/booksindex.css` - Book listing styles
- `~/css/cartstylesheet.css` - Cart-specific styles

### Bootstrap Integration
- Bootstrap 5.3.3
- Bootstrap Icons 1.11.3
- Custom theme overrides for user role

## Security Features

- Authorization policies (`UserOrHigher`)
- CSRF protection with `@Html.AntiForgeryToken()`
- JWT token handling in JavaScript
- Secure API endpoints

## Responsive Design

All views are designed to be responsive with:
- Mobile-first approach
- Bootstrap grid system
- Responsive navigation
- Touch-friendly interactions

## Error Handling

- Consistent error messaging
- Graceful degradation
- Loading states
- Empty state handling
- Network error recovery

## Accessibility

- ARIA labels and roles
- Keyboard navigation support
- Screen reader compatibility
- High contrast support
- Focus management

## Performance Optimizations

- AJAX loading for dynamic content
- Image lazy loading
- Pagination for large datasets
- Debounced search input
- Efficient DOM updates