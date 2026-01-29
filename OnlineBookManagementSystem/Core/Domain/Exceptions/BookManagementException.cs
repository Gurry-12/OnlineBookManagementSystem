namespace OnlineBookManagementSystem.Core.Domain.Exceptions
{
    /// <summary>
    /// Base exception for all book management domain exceptions
    /// </summary>
    public abstract class BookManagementException : Exception
    {
        public string ErrorCode { get; }
        public Dictionary<string, object> ErrorData { get; }

        protected BookManagementException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
            ErrorData = new Dictionary<string, object>();
        }

        protected BookManagementException(string errorCode, string message, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorData = new Dictionary<string, object>();
        }

        public void AddErrorData(string key, object value)
        {
            ErrorData[key] = value;
        }
    }

    /// <summary>
    /// Exception thrown when a book is not found
    /// </summary>
    public class BookNotFoundException : BookManagementException
    {
        public int BookId { get; }

        public BookNotFoundException(int bookId) 
            : base("BOOK_NOT_FOUND", $"Book with ID {bookId} was not found")
        {
            BookId = bookId;
            AddErrorData("BookId", bookId);
        }
    }

    /// <summary>
    /// Exception thrown when a category is not found
    /// </summary>
    public class CategoryNotFoundException : BookManagementException
    {
        public int CategoryId { get; }

        public CategoryNotFoundException(int categoryId) 
            : base("CATEGORY_NOT_FOUND", $"Category with ID {categoryId} was not found")
        {
            CategoryId = categoryId;
            AddErrorData("CategoryId", categoryId);
        }
    }

    /// <summary>
    /// Exception thrown when there's insufficient stock for an operation
    /// </summary>
    public class InsufficientStockException : BookManagementException
    {
        public string BookTitle { get; }
        public int RequestedQuantity { get; }
        public int AvailableQuantity { get; }

        public InsufficientStockException(string bookTitle, int requestedQuantity, int availableQuantity)
            : base("INSUFFICIENT_STOCK", $"Insufficient stock for '{bookTitle}'. Requested: {requestedQuantity}, Available: {availableQuantity}")
        {
            BookTitle = bookTitle;
            RequestedQuantity = requestedQuantity;
            AvailableQuantity = availableQuantity;
            AddErrorData("BookTitle", bookTitle);
            AddErrorData("RequestedQuantity", requestedQuantity);
            AddErrorData("AvailableQuantity", availableQuantity);
        }
    }

    /// <summary>
    /// Exception thrown when an invalid order state transition is attempted
    /// </summary>
    public class InvalidOrderStateException : BookManagementException
    {
        public InvalidOrderStateException(string message) 
            : base("INVALID_ORDER_STATE", message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a user is not found
    /// </summary>
    public class UserNotFoundException : BookManagementException
    {
        public int UserId { get; }

        public UserNotFoundException(int userId) 
            : base("USER_NOT_FOUND", $"User with ID {userId} was not found")
        {
            UserId = userId;
            AddErrorData("UserId", userId);
        }
    }

    /// <summary>
    /// Exception thrown when an order is not found
    /// </summary>
    public class OrderNotFoundException : BookManagementException
    {
        public int OrderId { get; }

        public OrderNotFoundException(int orderId) 
            : base("ORDER_NOT_FOUND", $"Order with ID {orderId} was not found")
        {
            OrderId = orderId;
            AddErrorData("OrderId", orderId);
        }
    }

    /// <summary>
    /// Exception thrown when a business rule validation fails
    /// </summary>
    public class BusinessRuleViolationException : BookManagementException
    {
        public string RuleName { get; }

        public BusinessRuleViolationException(string ruleName, string message) 
            : base("BUSINESS_RULE_VIOLATION", message)
        {
            RuleName = ruleName;
            AddErrorData("RuleName", ruleName);
        }
    }

    /// <summary>
    /// Exception thrown when a concurrency conflict occurs
    /// </summary>
    public class ConcurrencyConflictException : BookManagementException
    {
        public string EntityType { get; }
        public int EntityId { get; }

        public ConcurrencyConflictException(string entityType, int entityId) 
            : base("CONCURRENCY_CONFLICT", $"The {entityType} with ID {entityId} has been modified by another user. Please refresh and try again.")
        {
            EntityType = entityType;
            EntityId = entityId;
            AddErrorData("EntityType", entityType);
            AddErrorData("EntityId", entityId);
        }
    }
}