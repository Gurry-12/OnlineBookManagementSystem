using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;

namespace OnlineBookManagementSystem.Presentation.Controllers;

/// <summary>
/// Universal Books Controller - Handles all roles with skinny actions (5 lines or less)
/// Replaces separate Admin/User/Public book controllers following DRY principle
/// </summary>
public class UniversalBooksController : BaseController
{
    private readonly IUnifiedBookService _bookService;
    private readonly IActivityLogger _activityLogger;

    public UniversalBooksController(IUnifiedBookService bookService, IActivityLogger activityLogger)
    {
        _bookService = bookService;
        _activityLogger = activityLogger;
    }

    /// <summary>
    /// Universal book details - adapts to user role automatically
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRole();
        var viewModel = await _bookService.GetBookDetailsAsync(id, userId, userRole);
        return View("~/Views/Shared/_UniversalBookDetails.cshtml", viewModel);
    }

    /// <summary>
    /// Universal book listing with role-based filtering
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> List(int page = 1, string? search = null, int? categoryId = null, string? sortBy = null)
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRole();
        var books = await _bookService.GetBooksAsync(page, 12, search, categoryId, sortBy, null, null, userId, userRole);
        return View("~/Views/Shared/_UniversalBookList.cshtml", books);
    }

    /// <summary>
    /// Create book - Admin/SuperAdmin only
    /// </summary>
    [Authorize(Policy = "AdminOrHigher")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookDto createBookDto)
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRole();
        var result = await _bookService.CreateBookAsync(createBookDto, userId, userRole);
        return Json(new { success = true, data = result });
    }

    /// <summary>
    /// Update book - Admin/SuperAdmin only
    /// </summary>
    [Authorize(Policy = "AdminOrHigher")]
    [HttpPut]
    public async Task<IActionResult> Update(int id, [FromBody] CreateBookDto updateBookDto)
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRole();
        var result = await _bookService.UpdateBookAsync(id, updateBookDto, userId, userRole);
        return Json(new { success = true, data = result });
    }

    /// <summary>
    /// Delete book - Admin/SuperAdmin only
    /// </summary>
    [Authorize(Policy = "AdminOrHigher")]
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRole();
        var success = await _bookService.DeleteBookAsync(id, userId, userRole);
        return Json(new { success });
    }

    /// <summary>
    /// Toggle favorite - User or higher
    /// </summary>
    [Authorize(Policy = "UserOrHigher")]
    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = GetUserIdFromClaims();
        var success = await _bookService.ToggleFavoriteAsync(id, userId);
        return Json(new { success });
    }

    /// <summary>
    /// Get book analytics - Role-based data
    /// </summary>
    [Authorize]
    public async Task<IActionResult> Analytics(int id)
    {
        var userRole = GetUserRole();
        var analytics = await _bookService.GetBookAnalyticsAsync(id, userRole);
        return Json(analytics);
    }

    /// <summary>
    /// Helper method to get user role from claims
    /// </summary>
    private string GetUserRole()
    {
        if (User.IsInRole("SuperAdmin")) return "SuperAdmin";
        if (User.IsInRole("Admin")) return "Admin";
        if (User.IsInRole("User")) return "User";
        return "Public";
    }
}