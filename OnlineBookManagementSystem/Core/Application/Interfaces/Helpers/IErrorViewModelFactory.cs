using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Helpers
{
    /// <summary>
    /// Interface for creating error view models.
    /// Follows DIP (Dependency Inversion Principle) to decouple Controller from logic.
    /// </summary>
    public interface IErrorViewModelFactory
    {
        /// <summary>
        /// Creates an ErrorViewModel based on the HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="requestId">The request ID for tracking.</param>
        /// <returns>A populated ErrorViewModel.</returns>
        ErrorViewModel Create(int statusCode, string? requestId);

        /// <summary>
        /// Creates an ErrorViewModel based on an exception (usually 500).
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="requestId">The request ID for tracking.</param>
        /// <returns>A populated ErrorViewModel.</returns>
        ErrorViewModel Create(Exception exception, string? requestId);
    }
}
