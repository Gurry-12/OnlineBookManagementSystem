using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OnlineBookManagementSystem.Shared.Utilities
{
    public static class ErrorHandlingUtilities
    {
        public static JsonResult CreateErrorResponse(string message, object? details = null, int statusCode = 400)
        {
            return new JsonResult(new
            {
                success = false,
                message,
                details,
                timestamp = DateTime.UtcNow
            })
            { StatusCode = statusCode };
        }

        public static JsonResult CreateSuccessResponse(string message, object? data = null)
        {
            return new JsonResult(new
            {
                success = true,
                message,
                data,
                timestamp = DateTime.UtcNow
            });
        }

        public static Dictionary<string, string[]> ExtractValidationErrors(ModelStateDictionary modelState)
        {
            return modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );
        }

        public static async Task<(bool Success, T? Result, string ErrorMessage)> SafeExecuteAsync<T>(
            Func<Task<T>> operation, 
            ILogger? logger = null, 
            string operationName = "Operation")
        {
            try
            {
                var result = await operation();
                return (true, result, string.Empty);
            }
            catch (ArgumentException ex)
            {
                logger?.LogWarning(ex, "{OperationName} failed: Invalid arguments", operationName);
                return (false, default, "Invalid input provided.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger?.LogWarning(ex, "{OperationName} failed: Unauthorized access", operationName);
                return (false, default, "Access denied.");
            }
            catch (InvalidOperationException ex)
            {
                logger?.LogError(ex, "{OperationName} failed: Invalid operation", operationName);
                return (false, default, "Operation cannot be completed at this time.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "{OperationName} failed: Unexpected error", operationName);
                return (false, default, "An unexpected error occurred. Please try again.");
            }
        }

        public static bool ValidateUserId(int userId, int? expectedUserId = null, ILogger? logger = null)
        {
            if (userId <= 0)
            {
                logger?.LogWarning("Invalid user ID: {UserId}", userId);
                return false;
            }

            if (expectedUserId.HasValue && userId != expectedUserId.Value)
            {
                logger?.LogWarning("User ID mismatch: {CurrentUserId} vs {ExpectedUserId}", userId, expectedUserId.Value);
                return false;
            }

            return true;
        }

        public static string CreateSafeFileName(string fileName, string fallbackExtension = ".jpg")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return $"file_{DateTime.UtcNow.Ticks}{fallbackExtension}";

            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            
            if (!Path.HasExtension(safeName))
                safeName += fallbackExtension;

            return safeName.Length > 100 
                ? Path.GetFileNameWithoutExtension(safeName)[..^Path.GetExtension(safeName).Length] + Path.GetExtension(safeName)
                : safeName;
        }
    }
}
