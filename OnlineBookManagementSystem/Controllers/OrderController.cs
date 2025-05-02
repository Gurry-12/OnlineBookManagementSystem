using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
{
    public class OrderController : BaseController
    {
        private readonly BookManagementContext _context;
        public OrderController(BookManagementContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> AdminIndex()
        {
            var orders = await _context.Orders.Include(o => o.OrderDetails).ToListAsync();
            return View("Admin/AdminIndex", orders);
        }

        public async Task<IActionResult> AdminDetails(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails)
                                              .ThenInclude(od => od.Book)
                                              .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View("Admin/AdminDetails" , order);
        }

        public IActionResult Edit(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            return View("Admin/Edit", order);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AdminIndex));
        }

       
        public async Task<IActionResult> Index()
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(sessionUserId);
            

            var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
            return View("User/Index", orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            if (sessionUserId == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(sessionUserId);

            var order = await _context.Orders.Include(o => o.OrderDetails)
                                              .ThenInclude(od => od.Book)
                                              .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound();
            return View("User/Details" , order);
        }

    }
}
