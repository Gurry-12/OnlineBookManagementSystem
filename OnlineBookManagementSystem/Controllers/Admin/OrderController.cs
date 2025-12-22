using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Services.Admin;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers.Admin
{
    [Authorize(Policy = "AdminOrHigher")]
    [Area("Admin")]
    [Route("Admin/Orders")]
    public class OrderController : Controller
    {
        private readonly IAdminOrderService _service;

        public OrderController(IAdminOrderService service)
        {
            _service = service;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet]
        [Route("/Admin/Orders")]
        public async Task<IActionResult> Index(string search = "", string status = "", int page = 1)
        {
            var model = await _service.GetOrdersAsync(page, 10, search, status);
            return View("Admin/AdminIndex", model);
        }

        [HttpGet]
        [Route("{id}/details")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _service.GetOrderDetailsAsync(id);
            if (order == null) return NotFound();
            return View("Admin/AdminDetails", order);
        }

        [HttpPost]
        [Route("{id}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var success = await _service.UpdateOrderStatusAsync(id, status, CurrentUserId);
            if (!success) return NotFound();

            TempData["Success"] = $"Order #{id} updated to {status}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
