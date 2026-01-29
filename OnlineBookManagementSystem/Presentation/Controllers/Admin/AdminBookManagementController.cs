using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;

namespace OnlineBookManagementSystem.Presentation.Controllers.Admin
{
    /// <summary>
    /// Handles admin book management functionality following SRP.
    /// Responsible only for book CRUD operations and book-related admin tasks.
    /// </summary>
    [Authorize(Policy = "AdminOrHigher")]
    public class AdminBookManagementController : BaseController
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookCommandService _bookCommandService;
        private readonly ICategoryInterface _categoryService;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<AdminBookManagementController> _logger;

        public AdminBookManagementController(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService,
            ICategoryInterface categoryService,
            IActivityLogger activityLogger,
            ILogger<AdminBookManagementController> logger)
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
            _categoryService = categoryService;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<IActionResult> Books(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null, bool? inStock = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var model = await _bookQueryService.GetPaginatedBooksAsync(page, 12, search, categoryId, sortBy, inStock: inStock);

                // Add categories for filter dropdown
                ViewBag.Categories = await _categoryService.GetCategoriesForDropdownAsync();
                ViewBag.Search = search;
                ViewBag.CategoryId = categoryId;
                ViewBag.SortBy = sortBy;
                ViewBag.InStock = inStock;

                await _activityLogger.LogAsync("ViewBooks", "Admin books page accessed", userId);

                // Return partial view for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return PartialView("_BooksGrid", model);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books for admin user {UserId}", userId);
                TempData["ErrorMessage"] = "Failed to load books.";
                return View();
            }
        }

        public async Task<IActionResult> CreateBook()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var viewModel = await _bookQueryService.GetCreateBookViewModelAsync();

                // Return partial view for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_BookForm", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create book form");
                TempData["ErrorMessage"] = "Failed to load book creation form.";
                return RedirectToAction(nameof(Books));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook(BookFormViewModel model, IFormFile? imageFile)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return HandleUnauthorized();

            var validationResult = await ValidateBookModel(model);
            if (!validationResult.IsValid)
                return validationResult.Result;

            try
            {
                var success = await _bookCommandService.AddBookAsync(model.Book!, imageFile);
                if (success)
                {
                    await _activityLogger.LogAsync("CreateBook", $"Book '{model.Book!.Title}' created", userId);
                    return HandleSuccess("Book created successfully!", nameof(Books));
                }

                return HandleError("Failed to create book. Please try again.", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book for user {UserId}", userId);
                return HandleError("An error occurred while creating the book.", model);
            }
        }

        public async Task<IActionResult> EditBook(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var viewModel = await _bookQueryService.GetEditBookViewModelAsync(id);
                if (viewModel == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = "Book not found." });

                    TempData["ErrorMessage"] = "Book not found.";
                    return RedirectToAction(nameof(Books));
                }

                // Return partial view for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_BookForm", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit book form for book {BookId}", id);
                TempData["ErrorMessage"] = "Failed to load book for editing.";
                return RedirectToAction(nameof(Books));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditBook(int id, BookFormViewModel model, IFormFile? imageFile)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return HandleUnauthorized();

            if (id != model?.Book?.Id)
                return HandleError("Invalid book ID", null);

            var validationResult = await ValidateBookModel(model);
            if (!validationResult.IsValid)
                return validationResult.Result;

            try
            {
                var success = await _bookCommandService.UpdateBookAsync(model.Book!, imageFile);
                if (success)
                {
                    await _activityLogger.LogAsync("UpdateBook", $"Book '{model.Book!.Title}' updated", userId);
                    return HandleSuccess("Book updated successfully!", nameof(Books));
                }

                return HandleError("Failed to update book. Please try again.", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book {BookId} for user {UserId}", id, userId);
                return HandleError("An error occurred while updating the book.", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _bookCommandService.SoftDeleteBookAsync(id, userId);
                if (success)
                {
                    await _activityLogger.LogAsync("DeleteBook", $"Book with ID {id} deleted", userId);
                    return Json(new { success = true, message = "Book deleted successfully" });
                }
                return Json(new { success = false, message = "Book not found or already deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book {BookId} for user {UserId}", id, userId);
                return Json(new { success = false, message = "An error occurred while deleting the book" });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            try
            {
                var book = await _bookQueryService.GetBookByIdAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Book not found.";
                    return RedirectToAction(nameof(Books));
                }

                await _activityLogger.LogAsync("ViewBookDetails", $"Admin viewed details for book '{book.Title}'", userId);
                return View(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book details for book {BookId}", id);
                TempData["ErrorMessage"] = "Failed to load book details.";
                return RedirectToAction(nameof(Books));
            }
        }

        private IActionResult HandleUnauthorized()
        {
            return IsAjaxRequest()
                ? Json(new { success = false, message = "Unauthorized" })
                : RedirectToAction("Login", "Auth");
        }

        private async Task<(bool IsValid, IActionResult Result)> ValidateBookModel(BookFormViewModel? model)
        {
            if (model?.Book == null)
            {
                var errorMessage = "Invalid book data provided.";
                return (false, IsAjaxRequest()
                    ? Json(new { success = false, message = errorMessage })
                    : BadRequest(errorMessage));
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesForModel(model);

                if (IsAjaxRequest())
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );
                    return (false, Json(new { success = false, message = "Validation failed", errors }));
                }

                return (false, View(model));
            }

            return (true, null!);
        }

        private async Task LoadCategoriesForModel(BookFormViewModel model)
        {
            try
            {
                model.Categories = await _categoryService.GetCategoriesForDropdownAsync() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories for book form");
                model.Categories = new List<SelectListItem>();
                ModelState.AddModelError("", "Unable to load categories. Please try again.");
            }
        }

        private IActionResult HandleSuccess(string message, string redirectAction)
        {
            return IsAjaxRequest()
                ? Json(new { success = true, message })
                : (TempData["SuccessMessage"] = message, RedirectToAction(redirectAction)).Item2;
        }

        private IActionResult HandleError(string message, BookFormViewModel? model)
        {
            if (IsAjaxRequest())
                return Json(new { success = false, message });

            if (model != null)
            {
                ModelState.AddModelError("", message);
                LoadCategoriesForModel(model).Wait();
                return View(model);
            }

            return BadRequest(message);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}