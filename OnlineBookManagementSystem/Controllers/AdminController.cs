using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.ViewModel;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly IBookService _bookService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IActivityLogger _activityLogger;
        private readonly IUsersService _userService;

        public AdminController(IBookService bookService, ICartService cartService, IOrderService orderService, IActivityLogger activityLogger, IUsersService userService)
        {
            _bookService = bookService;
            _cartService = cartService;
            _orderService = orderService;
            _activityLogger = activityLogger;
            _userService = userService;
        }

        [Authorize(Policy = "AdminOrHigher")]
        public IActionResult Dashboard()
        {
            var userId = GetUserIdFromClaims();  // Helper below
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var viewModel = _bookService.GetQuickStats(userId) ?? new AdminViewModel(); ;
            return View(viewModel);
        }

        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }
}
