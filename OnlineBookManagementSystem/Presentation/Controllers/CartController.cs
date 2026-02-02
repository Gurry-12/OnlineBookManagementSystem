using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Presentation.ViewModels.Cart;

namespace OnlineBookManagementSystem.Presentation.Controllers
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

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> ViewUserCart(int userId)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId == 0) return Unauthorized();

            var cartItems = await _cartService.GetUserCartAsync(userId);
            var summary = await _cartService.GetCartSummaryAsync(userId);

            // Create unified CartViewModel with admin read-only capabilities
            var model = new UnifiedCartViewModel
            {
                CartItems = cartItems.Select(item => new CartItemViewModel
                {
                    BookId = item.BookId,
                    BookTitle = item.BookTitle,
                    BookAuthor = item.BookAuthor,
                    BookPrice = item.BookPrice,
                    Quantity = item.Quantity,
                    Subtotal = item.Subtotal,
                    BookImage = item.BookImage,
                    CategoryName = item.CategoryName,
                    IsAvailable = item.IsAvailable
                }).ToList(),
                Summary = summary,
                UserId = userId,
                UserName = $"User {userId}", // This should come from user service in real implementation
                LastUpdated = DateTime.Now,
                Capabilities = new CartCapabilities
                {
                    CanViewCart = true,
                    CanViewCartDetails = true,
                    CanViewUserInfo = true,
                    CanModifyCart = false,
                    CanUpdateQuantity = false,
                    CanRemoveItems = false,
                    CanCheckout = false,
                    CanClearCart = false,
                    IsReadOnly = true,
                    IsAuthenticated = true,
                    PageTitle = "User Cart (Admin View)",
                    BackLinkText = "Back to Admin",
                    BackLinkUrl = "/Admin/Dashboard",
                    CheckoutButtonText = "Checkout Not Available"
                }
            };

            return View("~/Presentation/Views/Cart/CartView.cshtml", model);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var checkoutData = await _cartService.CheckoutDetailsAsync(userId);

            // Create unified CheckoutViewModel with user capabilities
            var model = new UnifiedCheckoutViewModel
            {
                CartItems = checkoutData.CartItems.Select(item => new CartItemViewModel
                {
                    BookId = item.BookId,
                    BookTitle = item.BookTitle,
                    BookAuthor = item.BookAuthor,
                    BookPrice = item.BookPrice,
                    Quantity = item.Quantity,
                    Subtotal = item.Subtotal,
                    BookImage = item.BookImage,
                    CategoryName = item.CategoryName,
                    IsAvailable = item.IsAvailable
                }).ToList(),
                Summary = checkoutData.Summary,
                Subtotal = checkoutData.Subtotal,
                Tax = checkoutData.Tax,
                Shipping = checkoutData.Shipping,
                GrandTotal = checkoutData.GrandTotal,
                UserId = userId,
                Name = checkoutData.Name,
                Address = checkoutData.Address,
                PhoneNumber = checkoutData.PhoneNumber,
                City = checkoutData.City,
                State = checkoutData.State,
                ZipCode = checkoutData.ZipCode,
                PaymentMethod = checkoutData.PaymentMethod,
                Capabilities = new CheckoutCapabilities
                {
                    CanViewOrderSummary = true,
                    CanViewShippingForm = true,
                    CanViewPaymentOptions = true,
                    CanConfirmCheckout = true,
                    CanModifyOrder = false,
                    CanSelectPaymentMethod = true,
                    ShowCODOption = true,
                    ShowOnlinePayment = true,
                    IsAuthenticated = true,
                    PageTitle = "Checkout",
                    BackLinkText = "Back to Cart",
                    BackLinkUrl = "/User/UserCart",
                    ConfirmButtonText = "Place Order"
                }
            };

            return View("~/Presentation/Views/Cart/CheckOut.cshtml", model);
        }

        [Authorize(Policy = "UserOrHigher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckOutRequestViewModel request)
        {
            if (!ModelState.IsValid) return View(request);

            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var result = await _cartService.ProcessCheckoutAsync(userId, request);
            if (result.Success)
            {
                TempData["Success"] = "Order placed successfully! Check your email for confirmation.";
                return RedirectToAction("OrderConfirmation", new { orderId = result.OrderId });
            }

            ModelState.AddModelError("", result.Message);
            var viewModel = await _cartService.CheckoutDetailsAsync(userId);
            return View("~/Presentation/Views/Cart/CheckOut.cshtml", viewModel);
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
