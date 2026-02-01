using Microsoft.AspNetCore.Mvc;

namespace OnlineBookManagementSystem.Presentation.Helpers
{
    /// <summary>
    /// Helper for handling AJAX vs full page responses consistently.
    /// Eliminates repeated AJAX detection logic in controllers.
    /// </summary>
    public static class AjaxResponseHelper
    {
        /// <summary>
        /// Checks if the request is an AJAX request
        /// </summary>
        public static bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers.Accept.ToString().Contains("application/json");
        }

        /// <summary>
        /// Returns appropriate view based on request type (AJAX vs full page)
        /// </summary>
        public static IActionResult HandleResponse<T>(
            HttpRequest request,
            T model,
            string fullViewName,
            string partialViewName,
            Controller controller)
        {
            if (IsAjaxRequest(request))
            {
                return controller.PartialView(partialViewName, model);
            }

            return controller.View(fullViewName, model);
        }

        /// <summary>
        /// Returns success response (JSON for AJAX, redirect for full page)
        /// </summary>
        public static IActionResult Success(
            HttpRequest request,
            string message,
            string redirectAction,
            Controller controller)
        {
            if (IsAjaxRequest(request))
            {
                return controller.Json(new { success = true, message });
            }

            controller.TempData["SuccessMessage"] = message;
            return controller.RedirectToAction(redirectAction);
        }

        /// <summary>
        /// Returns error response (JSON for AJAX, view with error for full page)
        /// </summary>
        public static IActionResult Error(
            HttpRequest request,
            string message,
            object? model,
            Controller controller)
        {
            if (IsAjaxRequest(request))
            {
                return controller.Json(new { success = false, message });
            }

            if (model != null)
            {
                controller.ModelState.AddModelError("", message);
                return controller.View(model);
            }

            controller.TempData["ErrorMessage"] = message;
            return controller.BadRequest(message);
        }

        /// <summary>
        /// Returns validation error response with field-level errors
        /// </summary>
        public static IActionResult ValidationError(
            HttpRequest request,
            string message,
            Dictionary<string, string[]> errors,
            object? model,
            Controller controller)
        {
            if (IsAjaxRequest(request))
            {
                return controller.Json(new 
                { 
                    success = false, 
                    message, 
                    errors 
                });
            }

            // Add errors to ModelState
            foreach (var error in errors)
            {
                foreach (var errorMessage in error.Value)
                {
                    controller.ModelState.AddModelError(error.Key, errorMessage);
                }
            }

            if (model != null)
            {
                return controller.View(model);
            }

            return controller.BadRequest(message);
        }

        /// <summary>
        /// Returns not found response
        /// </summary>
        public static IActionResult NotFound(
            HttpRequest request,
            string message,
            string redirectAction,
            Controller controller)
        {
            if (IsAjaxRequest(request))
            {
                return controller.Json(new { success = false, message });
            }

            controller.TempData["ErrorMessage"] = message;
            return controller.RedirectToAction(redirectAction);
        }

        /// <summary>
        /// Returns unauthorized response
        /// </summary>
        public static IActionResult Unauthorized(
            HttpRequest request,
            Controller controller)
        {
            if (IsAjaxRequest(request))
            {
                return controller.Json(new { success = false, message = "Unauthorized" });
            }

            return controller.RedirectToAction("Login", "Auth");
        }
    }
}
