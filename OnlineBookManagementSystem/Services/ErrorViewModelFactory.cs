using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Services
{
    public class ErrorViewModelFactory : IErrorViewModelFactory
    {
        private readonly IWebHostEnvironment _environment;

        // Dictionary for OCP-compliant status code mapping.
        // Easily extensible by adding new entries here.
        private readonly Dictionary<int, (string Title, string Message)> _statusMessages = new()
        {
            { 400, ("Bad Request", "The request could not be understood by the server due to malformed syntax.") },
            { 401, ("Unauthorized", "You do not have permission to access this resource.") },
            { 403, ("Forbidden", "Access to this resource is denied.") },
            { 404, ("Page Not Found", "Sorry, the page you are looking for could not be found.") },
            { 405, ("Method Not Allowed", "The request method is not supported for the requested resource.") },
            { 408, ("Request Timeout", "The server timed out waiting for the request.") },
            { 415, ("Unsupported Media Type", "The request entity has a media type which the server or resource does not support.") },
            { 429, ("Too Many Requests", "You have sent too many requests in a given amount of time.") },
            { 500, ("Internal Server Error", "Sorry, something went wrong on our end. Please try again later.") },
            { 502, ("Bad Gateway", "The server received an invalid response from the upstream server.") },
            { 503, ("Service Unavailable", "The server is currently unable to handle the request due to a temporary overload or maintenance.") },
            { 504, ("Gateway Timeout", "The server did not receive a timely response from the upstream server.") }
        };

        public ErrorViewModelFactory(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public ErrorViewModel Create(int statusCode, string? requestId)
        {
            if (!_statusMessages.TryGetValue(statusCode, out var messageInfo))
            {
                messageInfo = ("Error", "An unexpected error occurred.");
            }

            return new ErrorViewModel
            {
                StatusCode = statusCode,
                RequestId = requestId,
                Title = messageInfo.Title,
                Message = messageInfo.Message,
                Details = _environment.IsDevelopment() ? $"Status Code: {statusCode}" : null
            };
        }

        public ErrorViewModel Create(Exception exception, string? requestId)
        {
            var viewModel = new ErrorViewModel
            {
                StatusCode = 500,
                RequestId = requestId,
                Title = "Internal Server Error",
                Message = "An unexpected error occurred. Please try again later."
            };

            if (_environment.IsDevelopment())
            {
                viewModel.Details = $"{exception.GetType().Name}: {exception.Message}\n\n{exception.StackTrace}";
                viewModel.Title = "Exception Occurred (Dev Mode)";
            }

            return viewModel;
        }
    }
}
