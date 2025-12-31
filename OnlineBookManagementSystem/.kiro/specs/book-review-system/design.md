# Design Document

## Overview

The Book Review and Rating System will be implemented as an extension to the existing Online Book Management System, integrating seamlessly with the current ASP.NET Core MVC architecture. The system will provide a complete review lifecycle from submission through moderation to display, with robust caching and analytics capabilities.

## Architecture

### Integration Points
- **Database**: Extends existing `BookManagementContext` with new review-related entities
- **Authentication**: Leverages existing JWT-based authentication and role system
- **Caching**: Utilizes existing `IMemoryCache` service with review-specific cache keys
- **Logging**: Integrates with existing `IActivityLogger` for audit trails
- **Controllers**: Adds new `ReviewController` and extends existing `BookController`

### Technology Stack
- **Backend**: ASP.NET Core 9.0 with Entity Framework Core
- **Database**: SQLite (existing) with new review tables
- **Caching**: MemoryCache with future Redis compatibility
- **Frontend**: Razor Views with JavaScript for dynamic interactions
- **Validation**: FluentValidation for review content validation

## Components and Interfaces

### Core Entities

#### BookReview Entity
```csharp
public class BookReview
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string ReviewText { get; set; }
    public ReviewStatus Status { get; set; } // Pending, Approved, Rejected
    public string? RejectionReason { get; set; }
    public int? ModeratedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation Properties
    public virtual Book Book { get; set; }
    public virtual User User { get; set; }
    public virtual User? Moderator { get; set; }
}

public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Flagged = 3
}
```

#### BookRatingCache Entity
```csharp
public class BookRatingCache
{
    public int BookId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // Navigation Properties
    public virtual Book Book { get; set; }
}
```

### Service Interfaces

#### IReviewService Interface
```csharp
public interface IReviewService
{
    // Review Management
    Task<(bool Success, string Message)> SubmitReviewAsync(int userId, int bookId, int rating, string reviewText);
    Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int userId, int rating, string reviewText);
    Task<bool> DeleteReviewAsync(int reviewId, int userId);
    Task<BookReview?> GetUserReviewForBookAsync(int userId, int bookId);
    
    // Review Display
    Task<PaginatedResult<ReviewDisplayViewModel>> GetBookReviewsAsync(int bookId, int page, int pageSize, ReviewSortOrder sortOrder, int? ratingFilter);
    Task<ReviewDisplayViewModel?> GetReviewByIdAsync(int reviewId);
    
    // Rating Calculations
    Task<BookRatingViewModel> GetBookRatingAsync(int bookId);
    Task RecalculateBookRatingAsync(int bookId);
    Task InvalidateRatingCacheAsync(int bookId);
    
    // Moderation
    Task<PaginatedResult<ReviewModerationViewModel>> GetPendingReviewsAsync(int page, int pageSize);
    Task<bool> ApproveReviewAsync(int reviewId, int moderatorId);
    Task<bool> RejectReviewAsync(int reviewId, int moderatorId, string reason);
    Task<bool> FlagReviewAsync(int reviewId, int moderatorId, string reason);
    
    // Analytics
    Task<ReviewAnalyticsViewModel> GetReviewAnalyticsAsync();
    Task<List<BookRatingStatsViewModel>> GetTopRatedBooksAsync(int count);
    Task<List<BookRatingStatsViewModel>> GetLowestRatedBooksAsync(int count);
}
```

### Controllers

#### ReviewController
- **POST** `/Review/Submit` - Submit new review
- **PUT** `/Review/Update/{id}` - Update existing review
- **DELETE** `/Review/Delete/{id}` - Delete review
- **GET** `/Review/Book/{bookId}` - Get paginated reviews for book
- **GET** `/Review/User/{userId}` - Get user's reviews

#### Extended BookController
- **GET** `/Book/Details/{id}` - Enhanced with review data
- **GET** `/Book/Rating/{id}` - Get book rating information

#### Admin/ReviewModerationController
- **GET** `/Admin/Reviews/Pending` - Pending reviews for moderation
- **POST** `/Admin/Reviews/Approve/{id}` - Approve review
- **POST** `/Admin/Reviews/Reject/{id}` - Reject review with reason
- **GET** `/Admin/Reviews/Analytics` - Review analytics dashboard

## Data Models

### Database Schema Extensions

#### BookReviews Table
```sql
CREATE TABLE BookReviews (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BookId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    Rating INTEGER NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    ReviewText TEXT NOT NULL CHECK (LENGTH(ReviewText) >= 10 AND LENGTH(ReviewText) <= 1000),
    Status INTEGER NOT NULL DEFAULT 0,
    RejectionReason TEXT NULL,
    ModeratedBy INTEGER NULL,
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    UpdatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    ModeratedAt DATETIME NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT 0,
    
    FOREIGN KEY (BookId) REFERENCES Books(Id),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (ModeratedBy) REFERENCES AspNetUsers(Id),
    
    UNIQUE(BookId, UserId) -- Prevent duplicate reviews per user per book
);

CREATE INDEX IX_BookReviews_BookId_Status ON BookReviews(BookId, Status);
CREATE INDEX IX_BookReviews_UserId ON BookReviews(UserId);
CREATE INDEX IX_BookReviews_Status_CreatedAt ON BookReviews(Status, CreatedAt);
CREATE INDEX IX_BookReviews_Rating ON BookReviews(Rating);
```

#### BookRatingCache Table
```sql
CREATE TABLE BookRatingCache (
    BookId INTEGER PRIMARY KEY,
    AverageRating REAL NOT NULL,
    TotalReviews INTEGER NOT NULL,
    LastUpdated DATETIME NOT NULL DEFAULT (datetime('now')),
    
    FOREIGN KEY (BookId) REFERENCES Books(Id)
);
```

### ViewModels

#### ReviewSubmissionViewModel
```csharp
public class ReviewSubmissionViewModel
{
    public int BookId { get; set; }
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 1000 characters")]
    public string ReviewText { get; set; } = string.Empty;
}
```

#### ReviewDisplayViewModel
```csharp
public class ReviewDisplayViewModel
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool CanEdit { get; set; } // True if current user owns this review
    public bool IsEdited { get; set; } // True if UpdatedAt > CreatedAt
}
```

#### BookRatingViewModel
```csharp
public class BookRatingViewModel
{
    public int BookId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Star -> Count
    public bool HasUserReview { get; set; }
    public ReviewDisplayViewModel? UserReview { get; set; }
}
```

## Error Handling

### Validation Rules
1. **Rating Validation**: Must be integer between 1-5
2. **Review Text Validation**: 10-1000 characters, no HTML tags
3. **Duplicate Prevention**: One review per user per book
4. **Authentication**: User must be logged in to submit reviews
5. **Book Existence**: Book must exist and not be deleted
6. **Moderation Authority**: Only Admins can moderate reviews

### Error Responses
- **400 Bad Request**: Invalid input data or validation failures
- **401 Unauthorized**: User not authenticated
- **403 Forbidden**: User lacks permission for action
- **404 Not Found**: Book or review not found
- **409 Conflict**: Duplicate review attempt
- **500 Internal Server Error**: System errors with logging

## Testing Strategy

### Unit Testing
- **Service Layer**: Test all business logic methods
- **Controller Actions**: Test HTTP responses and model binding
- **Validation**: Test all validation rules and edge cases
- **Cache Operations**: Test cache invalidation and updates
- **Database Operations**: Test CRUD operations and constraints

### Integration Testing
- **End-to-End Workflows**: Complete review submission to display
- **Authentication Integration**: Role-based access control
- **Database Transactions**: Multi-step operations with rollback
- **Cache Consistency**: Rating updates and cache invalidation
- **Moderation Workflow**: Complete moderation process testing

### Property-Based Testing
Property-based tests will validate universal correctness properties across all valid inputs, ensuring the system behaves correctly for any combination of valid data.

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property-Based Testing Overview

Property-based testing (PBT) validates software correctness by testing universal properties across many generated inputs. Each property is a formal specification that should hold for all valid inputs.

### Core Principles

1. **Universal Quantification**: Every property must contain an explicit "for all" statement
2. **Requirements Traceability**: Each property must reference the requirements it validates
3. **Executable Specifications**: Properties must be implementable as automated tests
4. **Comprehensive Coverage**: Properties should cover all testable acceptance criteria

### Correctness Properties

#### Property 1: Review Submission Validation
*For any* review submission with valid user and book IDs, the system should require both a rating (1-5) and review text (10-1000 characters), and reject submissions that don't meet these criteria
**Validates: Requirements 1.2, 1.3**

#### Property 2: Duplicate Review Prevention
*For any* user and book combination, submitting multiple reviews should update the existing review rather than creating duplicates, ensuring only one review exists per user per book
**Validates: Requirements 1.4**

#### Property 3: Review Status Management
*For any* newly submitted review, the initial status should be "Pending", and status transitions should only occur through proper moderation actions
**Validates: Requirements 1.5, 4.4**

#### Property 4: Rating Calculation Accuracy
*For any* book with approved reviews, the average rating should equal the mathematical average of all approved review ratings, excluding pending, rejected, or deleted reviews
**Validates: Requirements 2.2, 2.3**

#### Property 5: Review Display Completeness
*For any* approved review being displayed, all required fields (reviewer name, rating, review text, submission date) should be present and correctly formatted
**Validates: Requirements 3.2**

#### Property 6: Review Ownership Permissions
*For any* review being viewed, edit and delete options should be visible only to the review's original author, ensuring proper access control
**Validates: Requirements 3.3**

#### Property 7: Review Sorting and Filtering
*For any* set of reviews with applied sorting and filtering criteria, the results should be correctly ordered and contain only reviews matching all specified filters
**Validates: Requirements 5.1, 5.2, 5.4**

#### Property 8: Data Integrity Validation
*For any* review operation (create, update, delete), the system should validate that referenced books and users exist and are not deleted before proceeding
**Validates: Requirements 6.1, 6.2**

#### Property 9: Cascading Operations
*For any* book or user deletion, all associated reviews should be properly handled (soft-deleted for books, anonymized for users) while preserving review content
**Validates: Requirements 6.4, 6.5**

#### Property 10: Cache Consistency
*For any* review approval, rejection, or deletion that affects a book's rating, the cached average rating should be invalidated and recalculated to maintain data consistency
**Validates: Requirements 7.1, 7.3**

#### Property 11: Moderation Workflow Integrity
*For any* moderation action (approve, reject, flag), the system should update review status, create audit records, and trigger appropriate side effects (rating updates, notifications)
**Validates: Requirements 4.3, 4.4, 4.5**

#### Property 12: Analytics Accuracy
*For any* set of reviews in the system, analytics calculations (status counts, rating distributions, trends) should accurately reflect the current state of all reviews
**Validates: Requirements 8.1, 8.2, 8.3, 8.4**

#### Property 13: Export Data Completeness
*For any* review data export request, the generated CSV should contain all required fields for all reviews matching the export criteria, with proper formatting and no data loss
**Validates: Requirements 8.5**

### Testing Strategy

#### Dual Testing Approach
- **Unit tests**: Verify specific examples, edge cases, and error conditions
- **Property tests**: Verify universal properties across all inputs
- Both are complementary and necessary for comprehensive coverage

#### Property-Based Testing Configuration
- **Testing Framework**: Use NUnit with FsCheck.NUnit for property-based testing in C#
- **Test Iterations**: Minimum 100 iterations per property test to ensure comprehensive input coverage
- **Test Tagging**: Each property test must reference its design document property using the format:
  ```csharp
  [Test, Property]
  [Category("Feature: book-review-system, Property 1: Review Submission Validation")]
  public void Property_ReviewSubmissionValidation_RequiresBothRatingAndText(...)
  ```

#### Unit Testing Focus Areas
- **Specific Examples**: Test concrete scenarios like "user submits 5-star review with valid text"
- **Edge Cases**: Test boundary conditions like exactly 10 characters, exactly 1000 characters
- **Error Conditions**: Test invalid inputs, unauthorized access, missing data
- **Integration Points**: Test interactions between review system and existing book/user systems

#### Property Test Focus Areas
- **Universal Properties**: Test behaviors that must hold for all valid inputs
- **Input Generation**: Create smart generators that produce realistic review data
- **Comprehensive Coverage**: Ensure properties cover all business rules and constraints
- **Regression Prevention**: Catch edge cases that unit tests might miss

The testing strategy ensures both specific functionality works correctly (unit tests) and universal correctness properties hold across all possible inputs (property tests), providing comprehensive validation of the review system's correctness.