using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Controllers
{
    public class CartController : BaseController
    {
        private readonly BookManagementContext _context;

        public CartController(BookManagementContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> CartIndexUser()
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            int UserId = int.Parse(sessionUserId);
            var cartData = await _context.ShoppingCarts.Where(sc => sc.UserId == UserId). Include(sc => sc.Book)
        .Include(sc => sc.User)
        .ToListAsync();
            return View(cartData);
        }

        [HttpPost]
        public IActionResult AddOrUpdateCart([FromBody] ShoppingCart data)
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null || data.BookId == 0)
            {
                return BadRequest("Invalid session or book.");
            }

            int userId = int.Parse(sessionUserId);
            var existingCart = _context.ShoppingCarts
                .FirstOrDefault(c => c.UserId == userId && c.BookId == data.BookId);

            if (existingCart != null)
            {
                existingCart.Quantity = (existingCart.Quantity ?? 0) + 1;
                _context.ShoppingCarts.Update(existingCart);
            }
            else
            {
                var cart = new ShoppingCart
                {
                    UserId = userId,
                    BookId = data.BookId,
                    Quantity = 1
                };
                _context.ShoppingCarts.Add(cart);
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] ShoppingCart data)
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null || data.BookId == 0)
            {
                return BadRequest("Invalid session or book.");
            }

            int userId = int.Parse(sessionUserId);
            var cartItem = _context.ShoppingCarts
                .FirstOrDefault(c => c.UserId == userId && c.BookId == data.BookId);

            if (cartItem != null)
            {
                cartItem.Quantity = data.Quantity;

                if (data.Quantity <= 0)
                    _context.ShoppingCarts.Remove(cartItem);
                else
                    _context.ShoppingCarts.Update(cartItem);

                _context.SaveChanges();
            }

            return Json(new { success = true });
        }



        [HttpGet]
        public IActionResult GetAllCartItems()
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null)
                return Json(new List<object>());

            int userId = int.Parse(sessionUserId);

            var cartItems = _context.ShoppingCarts
                .Where(c => c.UserId == userId)
                .Select(c => new {
                    bookId = c.BookId,
                    quantity = c.Quantity ?? 0
                })
                .ToList();

            return Json(cartItems);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveCartItems([FromBody] ShoppingCart Data)
        {
            var Item = await _context.ShoppingCarts.FirstOrDefaultAsync(sc => sc.UserId == Data.UserId && sc.BookId == Data.BookId);
            if ( Item == null)
            {
                return BadRequest("Invalid User or book.");
            }

            _context.ShoppingCarts.Remove(Item);
            await _context.SaveChangesAsync();
            var redirectUrl = Url.Action("CartIndexUser", "Cart");
            return Json(new { redirectUrl });


        }

    }
}
