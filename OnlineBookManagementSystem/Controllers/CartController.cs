using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Services;

namespace OnlineBookManagementSystem.Controllers
{
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> CartIndexUser()
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(sessionUserId);
            var cartData = await _cartService.GetUserCartAsync(userId);
            return View(cartData);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateCart([FromBody] ShoppingCart data)
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null || data.BookId == 0)
            {
                return BadRequest("Invalid session or book.");
            }

            int userId = int.Parse(sessionUserId);
            await _cartService.AddOrUpdateCartAsync(userId, data.BookId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] ShoppingCart data)
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null || data.BookId == 0)
            {
                return BadRequest("Invalid session or book.");
            }

            int userId = int.Parse(sessionUserId);
            await _cartService.UpdateCartQuantityAsync(userId, data.BookId, data.Quantity ?? 0);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetAllCartItems()
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null)
                return Json(new List<object>());

            int userId = int.Parse(sessionUserId);
            var cartItems = _cartService.GetAllCartItems(userId);
            return Json(cartItems);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveCartItems([FromBody] ShoppingCart data)
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null)
                return BadRequest("Invalid session.");

            int userId = int.Parse(sessionUserId);
            var removed = await _cartService.RemoveCartItemAsync(userId, data.BookId);

            if (!removed)
                return BadRequest("Invalid User or book.");

            var redirectUrl = Url.Action("CartIndexUser", "Cart");
            return Json(new { redirectUrl });
        }

        public IActionResult CheckOut(int id)
        {
            CheckOutViewModel viewModel = _cartService.CheckoutDetails(id).Result;

            return View(viewModel);
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

      

        [HttpPost]
public async Task<IActionResult> ProcessCheckout(string Name, string Address, string PaymentMethod)
{
            var userIdString = HttpContext.Session.GetString("userId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("User ID is missing from session.");
            }

            var userId = int.Parse(userIdString);
            var result = await _cartService.ProcessCheckoutAsync(userId, Name, Address, PaymentMethod);

            if (!result)
            {
                return RedirectToAction("CheckOut", "Cart"); // Cart empty
            }

            return RedirectToAction("OrderConfirmation");
        }

    }
}
