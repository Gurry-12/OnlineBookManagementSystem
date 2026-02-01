# Systematic Compilation Error Fixes

## Error Categories and Solutions

### 1. Enum String Conversion Errors (OrderStatus, PaymentStatus, ReviewStatus)
**Problem**: Cannot implicitly convert string to enum types
**Solution**: Use enum values directly (e.g., `OrderStatus.Pending` instead of `"Pending"`)

**Files to Fix**:
- OrderCommandService.cs
- OrderQueryService.cs  
- PaymentProcessingService.cs
- ReviewService.cs
- OrderController.cs
- AdminController.cs

### 2. Value Object Conversion Errors

#### ISBN (string to ISBN)
**Problem**: Cannot convert string to ISBN value object
**Solution**: Use `ISBN.Create(string)` method

#### Money (decimal to/from Money)
**Problem**: Cannot convert between decimal and Money
**Solution**: 
- To Money: `Money.FromDecimal(decimal)`
- From Money: `money.Amount`
- Operations: Use `.Amount` property for comparisons

#### Address (Address to string)
**Problem**: Cannot convert Address to string
**Solution**: Use `address?.ToString()` or access individual properties

### 3. Missing ViewModel Properties
**Problem**: ViewModels missing required properties
**Solutions**:

#### ProfileViewModel - Add:
- PhoneNumber
- Address
- City
- State
- Country
- ZipCode

#### CheckOutViewModel & CheckOutRequestViewModel - Add:
- FullName
- ShippingAddress
- PhoneNumber
- City
- State
- Country
- ZipCode
- Order (for CheckOutViewModel)

#### UserWithRoleViewModel - Add:
- UserName
- RequestedRole
- IsPendingApproval
- CreatedAt

#### CategoryViewModel - Change to:
- Remove `required` keyword or initialize in constructor

### 4. Read-Only Property Assignment Errors
**Problem**: Properties with only getters cannot be assigned
**Solutions**:

#### Order Entity:
- FullName, Address: Use constructor or dedicated methods
- OrderDetails: Use collection initializer or Add methods

#### OrderDetail Entity:
- Price, Subtotal, TotalPrice: Calculate in constructor or use methods

#### Category Entity:
- Books: Use collection initializer or navigation property

#### ActivityLog Entity:
- ActionType, Description: Set via constructor

### 5. Missing Type Definitions
**Problem**: Types not found
**Solutions**:

#### ReviewViewModel
- Already exists as ReviewDisplayViewModel in ReviewViewModels.cs
- Update references to use correct type name

#### SelectListItem
- Add using: `using Microsoft.AspNetCore.Mvc.Rendering;`

#### BookRatingViewModel, ReviewSubmissionViewModel
- Already exist in ReviewViewModels.cs
- Add proper using statements

### 6. Missing Entity Properties
**Problem**: Entities missing properties that code expects
**Solutions**:

#### User Entity - Add:
- Address, City, State, Country, ZipCode
- PhoneNumber
- LastLoginAt
- RefreshToken, RefreshTokenExpiryTime (or use separate RefreshToken entity)

#### Order Entity - Add:
- PhoneNumber, Email, Notes
- City, State, Country, ZipCode
- ShippedDate, DeliveredDate
- Property (if needed)

#### OrderDetail Entity - Add:
- TotalPrice (calculated property)
- CreatedAt, IsDeleted (if using soft delete pattern)

#### Book Entity - Add:
- TotalReviews (calculated property or cached value)
- BookReviews navigation property

#### UserFavorite Entity - Add:
- AddedAt property

### 7. Namespace and Interface Issues

#### IEmailSender Registration
**Problem**: MailKitEmailSender doesn't match interface
**Solution**: Verify IEmailSender interface matches implementation

#### PaymentStatus Ambiguity
**Problem**: Two PaymentStatus types exist
**Solution**: Remove duplicate, use only Core.Domain.Enums.PaymentStatus

#### IRepository Methods
**Problem**: GetByConditionAsync not found
**Solution**: Add method to IRepository interface or use existing LINQ methods

#### IActivityLogger Methods
**Problem**: LogActivityAsync signature mismatch
**Solution**: Update interface or implementation to match

### 8. Collection Type Mismatches
**Problem**: Cannot convert between collection types
**Solutions**:

#### List<User> to List<UserWithRoleViewModel>
- Use `.Select()` to map: `users.Select(u => new UserWithRoleViewModel { ... }).ToList()`

#### List<ShoppingCart> to List<ShoppingCartViewModel>
- Use mapping: `carts.Select(c => new ShoppingCartViewModel { ... }).ToList()`

#### IReadOnlyCollection to ICollection
- Use `.ToList()` or change parameter type

### 9. Operator Issues with Value Objects

#### Money Comparisons
- Change: `price >= 100` to `price.Amount >= 100`
- Change: `price * quantity` to `price.Amount * quantity`

#### Address Operations
- Change: `address.Length` to `address.ToString().Length`
- Change: `address.PostalCode` to `address.ZipCode` (if property exists)

### 10. Missing Enum Values
**Problem**: Enum values don't exist
**Solutions**:

#### ReviewStatus.Flagged
- Add to ReviewStatus enum or use existing value

#### PaymentStatus.Paid
- Verify enum has this value or use correct value name

### 11. Protection Level Issues
**Problem**: `_logger` is inaccessible
**Solution**: Change from `private` to `protected` in BaseController

### 12. Required Member Issues
**Problem**: Required members not set in initializer
**Solutions**:

#### CategoryViewModel
- Remove `required` keyword
- Or always initialize: `new CategoryViewModel { CategoryList = ..., NewCategory = ... }`

### 13. Lambda Return Type Issues
**Problem**: Lambda return types not convertible
**Solution**: Ensure all code paths return same type, add explicit casts if needed

## Implementation Priority

1. **High Priority** (Breaks compilation completely):
   - Enum conversions
   - Value object conversions
   - Missing using directives
   - Missing type definitions

2. **Medium Priority** (Many errors):
   - Missing ViewModel properties
   - Missing Entity properties
   - Read-only property assignments

3. **Low Priority** (Fewer errors):
   - Collection type mismatches
   - Protection levels
   - Lambda return types

## Next Steps

1. Fix enum conversions across all files
2. Add missing properties to ViewModels
3. Add missing properties to Entities
4. Fix value object conversions
5. Update read-only properties to use proper patterns
6. Add missing using directives
7. Fix collection mappings
8. Test compilation after each major category
