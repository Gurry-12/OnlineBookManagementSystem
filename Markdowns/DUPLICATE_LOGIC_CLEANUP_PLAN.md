# Duplicate Logic Cleanup Plan

## Overview
This document identifies all duplicate business logic across interfaces and services that violate SOLID principles, particularly SRP (Single Responsibility Principle) and ISP (Interface Segregation Principle).

---

## 1. Analytics Methods - Now in IBookAnalyticsService

### ❌ Remove from IBookService:
- `GetMonthlyBookUploadsAsync()`
- `GetBooksByCategoryAsync()` (analytics version)
- `GetBooksByAuthorAsync()`
- `GetFavoriteStatsAsync()`
- `GetMonthlyStatsAsync()`

### ❌ Remove from IBookQueryService:
- `GetMonthlyBookUploadsAsync()`
- `GetBooksByCategoryAsync()` (analytics version)
- `GetBooksByAuthorAsync()`
- `GetFavoriteStatsAsync()`
- `GetMonthlyStatsAsync()`

### ❌ Remove from BookServices.cs:
- Lines 829-908: All analytics methods

### ❌ Remove from BookQueryService.cs:
- Lines 374-454: All analytics methods

### ✅ Keep only in:
- `IBookAnalyticsService`
- `BookAnalyticsService`

---

## 2. Favorites Methods - Now in IBookFavoriteService

### ❌ Remove from IBookService:
- `GetFavoriteBooksAsync(int userId)`
- `ToggleFavoriteAsync(int bookId, int userId)`

### ❌ Remove from IBookQueryService:
- `GetFavoriteBooksAsync(int userId)`
- `GetUserFavoriteBooksAsync(int userId)`
- `GetUserFavoritesCountAsync(int userId)`

### ❌ Remove from IBookCommandService:
- `ToggleFavoriteAsync(int bookId, int userId)`
- `ToggleUserFavoriteAsync(int bookId, int userId)`

### ❌ Remove from BookServices.cs:
- Lines 308-340: Favorite methods

### ❌ Remove from BookQueryService.cs:
- Lines 264-280: Favorite query methods

### ❌ Remove from BookCommandService.cs:
- Lines 254-290: Favorite command methods

### ✅ Keep only in:
- `IBookFavoriteService`
- `BookFavoriteService`

---

## 3. User Approval Methods - Now in IUserApprovalService

### ❌ Remove from IUsersService:
- `GetPendingUsersAsync()`
- `ApproveUserAsync(int userId, string role)`
- `RejectUserAsync(int userId)`

### ❌ Remove from IUserQueryService:
- `GetPendingUsersAsync()`

### ❌ Remove from IUserCommandService:
- `ApproveUserAsync(int userId, string role)`
- `RejectUserAsync(int userId)`

### ❌ Remove from UsersService.cs:
- Lines 333-430: Approval workflow methods

### ❌ Remove from UserQueryService.cs:
- Lines 220-245: GetPendingUsersAsync

### ❌ Remove from UserCommandService.cs:
- Lines 135-210: Approval methods

### ✅ Keep only in:
- `IUserApprovalService`
- `UserApprovalService`

---

## 4. Execution Order

1. **Phase 1**: Remove from legacy fat interfaces (IBookService, IUsersService)
2. **Phase 2**: Remove from implementations (BookServices, UsersService)
3. **Phase 3**: Remove from focused interfaces (IBookQueryService, IBookCommandService, etc.)
4. **Phase 4**: Remove from focused implementations (BookQueryService, BookCommandService, etc.)
5. **Phase 5**: Update all controllers to use new services
6. **Phase 6**: Update service registrations
7. **Phase 7**: Run tests and verify

---

## 5. Controller Updates Required

### AdminController:
- Replace `_bookService.GetMonthlyBookUploadsAsync()` → `_bookAnalyticsService.GetMonthlyBookUploadsAsync()`
- Replace `_bookService.GetBooksByCategoryAsync()` → `_bookAnalyticsService.GetBooksByCategoryAsync()`
- Replace `_bookService.GetBooksByAuthorAsync()` → `_bookAnalyticsService.GetBooksByAuthorAsync()`
- Replace `_bookService.GetFavoriteStatsAsync()` → `_bookAnalyticsService.GetFavoriteStatsAsync()`

### BooksController:
- Replace `_bookService.GetFavoriteBooksAsync()` → `_bookFavoriteService.GetUserFavoritesAsync()`
- Replace `_bookService.ToggleFavoriteAsync()` → `_bookFavoriteService.ToggleFavoriteAsync()`

### SuperAdminController:
- Replace `_usersService.GetPendingUsersAsync()` → `_userApprovalService.GetPendingUsersAsync()`
- Replace `_usersService.ApproveUserAsync()` → `_userApprovalService.ApproveUserAsync()`
- Replace `_usersService.RejectUserAsync()` → `_userApprovalService.RejectUserAsync()`

### UserController:
- Replace `_bookService.GetFavoriteBooksAsync()` → `_bookFavoriteService.GetUserFavoritesAsync()`
- Replace `_bookService.ToggleFavoriteAsync()` → `_bookFavoriteService.ToggleFavoriteAsync()`

---

## 6. Benefits After Cleanup

### Before:
- IBookService: 50+ methods
- IUsersService: 12 methods
- BookServices: 900+ lines
- UsersService: 450+ lines
- Duplicate logic in 3-4 places

### After:
- IBookService: 30 methods (or deprecated)
- IBookAnalyticsService: 4 methods
- IBookFavoriteService: 5 methods
- IUserApprovalService: 4 methods
- Single source of truth for each concern
- Clear separation of responsibilities
- Easier to test and maintain

---

## 7. Deprecation Strategy

Instead of immediately removing IBookService and IUsersService (which would break existing code), we'll:

1. Mark them as `[Obsolete]` with messages pointing to new services
2. Keep implementations but delegate to new services
3. Update all controllers to use new services
4. In next major version, remove deprecated interfaces

Example:
```csharp
[Obsolete("Use IBookAnalyticsService instead")]
Task<List<MonthlyBookUploadViewModel>> GetMonthlyBookUploadsAsync();
```

---

## 8. Testing Strategy

After each phase:
1. Run unit tests
2. Run integration tests
3. Manual smoke testing of affected features
4. Verify no duplicate logic remains

---

## Status: Ready for Execution
