using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Models;
using System.Diagnostics;

namespace OnlineBookManagementSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            switch (statusCode)
            {
                case 404:
                    ViewData["Title"] = "Page Not Found";
                    ViewData["ErrorMessage"] = "Sorry, the page you are looking for could not be found.";
                    ViewData["ErrorCode"] = "404";
                    break;
                case 500:
                    ViewData["Title"] = "Internal Server Error";
                    ViewData["ErrorMessage"] = "Sorry, something went wrong on our end. Please try again later.";
                    ViewData["ErrorCode"] = "500";
                    break;
                case 401:
                    ViewData["Title"] = "Unauthorized";
                    ViewData["ErrorMessage"] = "You do not have permission to access this resource.";
                    ViewData["ErrorCode"] = "401";
                    break;
                case 403:
                    ViewData["Title"] = "Forbidden";
                    ViewData["ErrorMessage"] = "Access to this resource is denied.";
                    ViewData["ErrorCode"] = "403";
                    break;
                default:
                    ViewData["Title"] = "Error";
                    ViewData["ErrorMessage"] = "An unexpected error occurred.";
                    ViewData["ErrorCode"] = statusCode.ToString();
                    break;
            }

            return View("Error", errorViewModel);
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            ViewData["Title"] = "Error";
            ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            ViewData["ErrorCode"] = "500";

            return View(errorViewModel);
        }
    }
}
