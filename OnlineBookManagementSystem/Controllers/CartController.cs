using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize]  // Global; per-action overrides
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }



        [Authorize(Policy = "UserOrHigher")]
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateCart([FromBody] CartItemRequestViewModel data)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0 || data.BookId == 0 || data.Quantity <= 0)
                return BadRequest("Invalid request.");

            var success = await _cartService.AddOrUpdateCartAsync(userId, data.BookId, data.Quantity);
            return Json(new { success, message = success ? "Added to cart!" : "Failed (stock low?)" });
        }

        [Authorize(Policy = "UserOrHigher")]
        [HttpPut]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartItemRequestViewModel data)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var success = await _cartService.UpdateCartQuantityAsync(userId, data.BookId, data.Quantity);
            return Json(new { success });
        }

        [Authorize(Policy = "UserOrHigher")]
        [HttpDelete]
        public async Task<IActionResult> RemoveItem(int bookId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var success = await _cartService.RemoveCartItemAsync(userId, bookId);
            return Json(new { success, redirectUrl = Url.Action("UserCart", "User") });
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> AdminCarts()
        {
            var adminId = GetUserIdFromClaims();
            var carts = await _cartService.GetAllCartsAsync(adminId);
            return View(carts);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var viewModel = await _cartService.CheckoutDetailsAsync(userId);
            return View(viewModel);
        }

        [Authorize(Policy = "UserOrHigher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckOutRequestViewModel request)
        {
            if (!ModelState.IsValid) return View(request);

            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var success = await _cartService.ProcessCheckoutAsync(userId, request);
            if (success)
            {
                TempData["Success"] = "Order placed! Check your email.";
                return RedirectToAction("OrderConfirmation");
            }

            ModelState.AddModelError("", "Checkout failed�cart empty or error.");
            var viewModel = await _cartService.CheckoutDetailsAsync(userId);
            return View(viewModel);
        }

        public IActionResult OrderConfirmation(int? orderId = null)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        // Helper
        private int GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }
}