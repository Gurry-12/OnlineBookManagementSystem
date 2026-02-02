using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Books
{
    public class BookValidationService : IBookValidationService
    {
        private readonly ILogger<BookValidationService> _logger;

        public BookValidationService(
            ILogger<BookValidationService> logger)
        {
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateBookAsync(Book book)
        {
            // Basic validation
            if (book == null)
                return ValidationResult.Failure("Book cannot be null");

            if (string.IsNullOrWhiteSpace(book.Title))
                return ValidationResult.Failure("Book title is required");

            if (string.IsNullOrWhiteSpace(book.Author))
                return ValidationResult.Failure("Book author is required");

            if (book.Price == null || book.Price.Amount <= 0)
                return ValidationResult.Failure("Book price must be greater than zero");

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateCreateBookAsync(Book book)
        {
            var basicValidation = await ValidateBookAsync(book);
            if (!basicValidation.IsValid)
                return basicValidation;

            // Additional create-specific validation
            if (!string.IsNullOrEmpty(book.ISBN?.Value))
            {
                var isUnique = await IsIsbnUniqueAsync(book.ISBN.Value);
                if (!isUnique)
                    return ValidationResult.Failure("ISBN already exists");
            }

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateUpdateBookAsync(Book book)
        {
            var basicValidation = await ValidateBookAsync(book);
            if (!basicValidation.IsValid)
                return basicValidation;

            // Additional update-specific validation
            if (!string.IsNullOrEmpty(book.ISBN?.Value))
            {
                var isUnique = await IsIsbnUniqueAsync(book.ISBN.Value, book.Id);
                if (!isUnique)
                    return ValidationResult.Failure("ISBN already exists");
            }

            return ValidationResult.Success();
        }

        public bool IsValidImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return false;

            // Check file size (5MB limit)
            if (image.Length > 5 * 1024 * 1024)
            {
                _logger.LogWarning("Image too large: {Size} bytes", image.Length);
                return false;
            }

            // Check content type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
            if (!allowedTypes.Contains(image.ContentType.ToLower()))
            {
                _logger.LogWarning("Unsupported image type: {ContentType}", image.ContentType);
                return false;
            }

            // Check file extension
            var extension = Path.GetExtension(image.FileName)?.ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Unsupported file extension: {Extension}", extension);
                return false;
            }

            return true;
        }

        public ValidationResult ValidateImageFile(IFormFile? image)
        {
            if (image == null)
                return ValidationResult.Success(); // Image is optional

            if (!IsValidImage(image))
            {
                return ValidationResult.Failure("Invalid image file. Please upload a JPEG or PNG file under 5MB.");
            }

            return ValidationResult.Success();
        }

        public async Task<bool> IsIsbnUniqueAsync(string isbn, int? excludeBookId = null)
        {
            // Implement ISBN uniqueness check
            return await Task.FromResult(true);
        }

        public async Task<bool> IsTitleUniqueAsync(string title, int? excludeBookId = null)
        {
            // Implement title uniqueness check
            return await Task.FromResult(true);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            // Implement category existence check
            return await Task.FromResult(true);
        }

        public bool IsStockQuantityValid(int stockQuantity, int lowStockThreshold)
        {
            return stockQuantity >= 0 && lowStockThreshold >= 0 && lowStockThreshold <= stockQuantity;
        }

        public ValidationResult ValidateStockLevels(int stockQuantity, int lowStockThreshold)
        {
            var errors = new List<string>();

            if (stockQuantity < 0)
            {
                errors.Add("Stock quantity cannot be negative");
            }

            if (lowStockThreshold < 0)
            {
                errors.Add("Low stock threshold cannot be negative");
            }

            if (lowStockThreshold > stockQuantity)
            {
                errors.Add("Low stock threshold cannot be greater than stock quantity");
            }

            return errors.Any() ? ValidationResult.Failure(errors.First()) : ValidationResult.Success();
        }
    }
}