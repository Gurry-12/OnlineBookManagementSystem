# Remaining Compilation Errors - Fix Plan

## Summary
The main issues are:
1. **Validators** expect old Order/Book structure (string addresses, decimal prices)
2. **Mappings** expect old User/Order properties that don't exist
3. **Order entity** needs to be checked - does it use Address value object or string fields?
4. **Book entity** needs to be checked - does it use Money value object or decimal?

## Critical Decision Needed

The Order and Book entities were migrated to use Value Objects (Address, Money), but:
- The validators still expect the old structure
- The mappings still expect the old structure  
- The database migrations might not match

## Two Options:

### Option A: Complete the Value Object Migration
- Update all validators to work with Money and Address value objects
- Update all mappings to work with value objects
- Ensure database is properly migrated

### Option B: Revert to Simple Types
- Change Order back to use string fields for address
- Change Book back to use decimal for price
- Update validators and mappings accordingly

## Recommendation: Option A (Complete Migration)

This maintains clean architecture principles. Here's what needs to be fixed:

### 1. OrderValidator Fixes
```csharp
// Change method signatures:
private void ValidateTotalAmount(Money totalAmount, ValidationResult result)
private void ValidateStatus(OrderStatus status, ValidationResult result)  
private void ValidateOrderDetails(IReadOnlyCollection<OrderDetail> orderDetails, ValidationResult result)

// In ValidateOrderDetails:
if (detail.UnitPrice.Amount <= 0)

// In ValidateTotalAmount:
if (totalAmount.Amount <= 0)

// In ValidateAddress - use Address value object:
if (order.ShippingAddress == null)
{
    result.AddError("ShippingAddress", "Shipping address is required");
    return;
}
if (string.IsNullOrWhiteSpace(order.ShippingAddress.Street))
if (string.IsNullOrWhiteSpace(order.ShippingAddress.City))
// etc.
```

### 2. BookValidator Fixes
```csharp
// In ValidatePrice:
private void ValidatePrice(Money price, ValidationResult result)
{
    if (price.Amount <= 0)
    {
        result.AddError("Price", "Price must be greater than zero");
    }
    else if (price.Amount > 999999.99m)
    {
        result.AddError("Price", "Price cannot exceed $999,999.99");
    }
}

// In ValidateStock:
private void ValidateStock(int? stockQuantity, int? lowStockThreshold, ValidationResult result)
{
    if (stockQuantity.HasValue && stockQuantity.Value < 0)
    if (lowStockThreshold.HasValue && lowStockThreshold.Value < 0)
    if (lowStockThreshold.HasValue && stockQuantity.HasValue && 
        lowStockThreshold.Value > stockQuantity.Value)
}
```

### 3. ReviewValidator Fixes
```csharp
// In ValidateStatus:
private void ValidateStatus(ReviewStatus status, ValidationResult result)
{
    if (!Enum.IsDefined(typeof(ReviewStatus), status))
    {
        result.AddError("Status", "Invalid review status");
    }
}

// In ValidateStatusTransitionAsync:
var validTransitions = new Dictionary<ReviewStatus, ReviewStatus[]>
{
    [ReviewStatus.Pending] = new[] { ReviewStatus.Approved, ReviewStatus.Rejected },
    [ReviewStatus.Approved] = new[] { ReviewStatus.Rejected },
    [ReviewStatus.Rejected] = new[] { ReviewStatus.Approved, ReviewStatus.Pending }
};
```

### 4. UserMappingExtensions - Remove Non-Existent Properties
The User entity doesn't have these properties anymore:
- Address, City, State, ZipCode, Country (removed in favor of navigation)
- LastLoginAt
- RefreshToken, RefreshTokenExpiryTime (moved to separate table)

Remove all references to these properties from mappings.

### 5. OrderMappingExtensions - Fix Address Fields
Order entity uses Address value object, not separate string fields.
Access via: `order.ShippingAddress.Street`, `order.ShippingAddress.City`, etc.

### 6. ReviewMappingExtensions - Use ReviewStatus Enum
Change all string comparisons to enum comparisons:
```csharp
// Instead of:
review.Status == "Approved"
// Use:
review.Status == ReviewStatus.Approved
```

### 7. SystemSettingsService - Already Fixed
Added System.Collections.Generic and System.Diagnostics namespaces.

### 8. AdminController - Already Fixed
Added Domain.Entities using statement.

### 9. BaseController - Already Fixed
Changed _logger to protected.

## Next Steps
1. Verify Order entity structure (does it use Address value object?)
2. Verify Book entity structure (does it use Money value object?)
3. Apply all validator fixes
4. Apply all mapping fixes
5. Test compilation
