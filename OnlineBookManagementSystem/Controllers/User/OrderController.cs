using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Services.User;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers.User
{
    [Authorize(Policy = "UserOrHigher")]
    [Area("User")]
    [Route("User/Orders")]
    public class OrderController : Controller
    {
        private readonly IUserOrderService _service;

        public OrderController(IUserOrderService service)
        {
            _service = service;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet]
        [Route("/User/Orders")]
        public async Task<IActionResult> Index()
        {
            var orders = await _service.GetMyOrdersAsync(CurrentUserId);
            return View("User/Index", orders);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _service.GetOrderDetailsAsync(id, CurrentUserId);
            if (order == null) return NotFound();
            return View("User/Details", order);
        }
    }
}
