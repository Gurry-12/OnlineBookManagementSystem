using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using System.Security.Claims;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Application.Mappings;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        private readonly BookManagementContext _context;
        private readonly IActivityLogger _logger;

        public OrderController(BookManagementContext context, IActivityLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // ==================== USER VIEWS ====================

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> AdminIndex(string search = "", string status = "", int page = 1)
        {
            int pageSize = 10;
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .Where(o => !o.IsDeleted);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.FullName.Contains(search) || o.Id.ToString() == search);

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                }
            }

            var total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new AdminOrderListViewModel
            {
                Orders = orders.Select(o => o.ToAdminOrderItem()).ToList(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchTerm = search,
                StatusFilter = status
            };

            return View("Admin/AdminIndex", model);
        }

        [Authorize(Policy = "AdminOrHigher")]
        public async Task<IActionResult> AdminDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (order == null) return NotFound();

            return View("Admin/AdminDetails", order);
        }

        [Authorize(Policy = "AdminOrHigher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var oldStatus = order.Status;
            if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                order.Status = orderStatus;
            }
            else
            {
                TempData["Error"] = "Invalid status value";
                return RedirectToAction(nameof(AdminDetails), new { id });
            }
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _logger.LogAsync("OrderStatusUpdate", $"Order #{id} status changed: {oldStatus} → {status}", CurrentUserId);

            TempData["Success"] = $"Order #{id} updated to {status}";
            return RedirectToAction(nameof(AdminDetails), new { id });
        }

        // ==================== USER VIEWS ====================

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == CurrentUserId && !o.IsDeleted)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View("User/Index", orders);
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == CurrentUserId && !o.IsDeleted);

            if (order == null) return NotFound();

            return View("User/Details", order);
        }
    }
}
