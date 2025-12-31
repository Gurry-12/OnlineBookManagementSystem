using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using System.Diagnostics;

namespace OnlineBookManagementSystem.Controllers
{
    public class ErrorController : Controller
    {
        private readonly IErrorViewModelFactory _errorFactory;
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(IErrorViewModelFactory errorFactory, ILogger<ErrorController> logger)
        {
            _errorFactory = errorFactory;
            _logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Log the error (Centralized logging)
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeReExecuteFeature != null)
            {
                _logger.LogWarning("Error {StatusCode} for request {Path}{QueryString}",
                    statusCode,
                    statusCodeReExecuteFeature.OriginalPath,
                    statusCodeReExecuteFeature.OriginalQueryString);
            }
            else
            {
                _logger.LogWarning("Error {StatusCode} occurred.", statusCode);
            }

            var viewModel = _errorFactory.Create(statusCode, requestId);
            return View("Error", viewModel);
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception occurred. Request ID: {RequestId}", requestId);
                var viewModel = _errorFactory.Create(exceptionHandlerPathFeature.Error, requestId);

                // Return 500 status code explicitly for unhandled exceptions
                HttpContext.Response.StatusCode = 500;

                return View("Error", viewModel);
            }

            // Fallback if accessed directly without an exception
            return HttpStatusCodeHandler(500);
        }
    }
}
