using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books
{
    /// <summary>
    /// Service interface for book validation operations
    /// </summary>
    public interface IBookValidationService
    {
        // Book validation
        Task<ValidationResult> ValidateBookAsync(Book book);
        Task<ValidationResult> ValidateCreateBookAsync(Book book);
        Task<ValidationResult> ValidateUpdateBookAsync(Book book);

        // Image validation
        bool IsValidImage(IFormFile image);
        ValidationResult ValidateImageFile(IFormFile? image);

        // Business rule validation
        Task<bool> IsIsbnUniqueAsync(string isbn, int? excludeBookId = null);
        Task<bool> IsTitleUniqueAsync(string title, int? excludeBookId = null);
        Task<bool> CategoryExistsAsync(int categoryId);

        // Stock validation
        bool IsStockQuantityValid(int stockQuantity, int lowStockThreshold);
        ValidationResult ValidateStockLevels(int stockQuantity, int lowStockThreshold);
    }

    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public List<ValidationError> Errors { get; private set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Failure(params ValidationError[] errors) =>
            new() { IsValid = false, Errors = errors.ToList() };

        public static ValidationResult Failure(string errorMessage) =>
            new() { IsValid = false, Errors = new List<ValidationError> { new(string.Empty, errorMessage) } };

        public void AddError(string propertyName, string errorMessage)
        {
            IsValid = false;
            Errors.Add(new ValidationError(propertyName, errorMessage));
        }
    }

    /// <summary>
    /// Represents a validation error for a specific property
    /// </summary>
    public class ValidationError
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }

        public ValidationError(string propertyName, string errorMessage, string errorCode = "")
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }
}