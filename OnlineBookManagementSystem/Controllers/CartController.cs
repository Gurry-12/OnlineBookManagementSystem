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
            var cartData = await _context.ShoppingCarts.Include(sc => sc.Book)
        .Include(sc => sc.User)
        .ToListAsync();
            return View(cartData);
        }
    }
}
