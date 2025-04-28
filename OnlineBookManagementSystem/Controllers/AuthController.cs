using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Services;

namespace OnlineBookManagementSystem.Controllers
{

    public class AuthController : BaseController
    {
        private readonly IAuthInterface _authService;

        public AuthController(IAuthInterface authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginData([FromBody] LoginViewModel data)
        {
            var (success, message, user) = await _authService.ValidateUserAsync(data);
            if (!success)
                return Json(new { success = false, message });

            var jwtToken = _authService.GenerateJwtToken(user);
            HttpContext.Session.SetString("userRole", user.Role);
            HttpContext.Session.SetString("userId", user.Id.ToString());

            var redirectUrl = user.Role == "Admin"
                ? Url.Action("AdminIndex", "Books")
                : Url.Action("UserIndex", "Books");

            return Json(new
            {
                success = true,
                message = "Login Successful",
                token = jwtToken,
                redirectUrl,
                userName = user.Name,
                role = user.Role
            });
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveData([FromBody] RegisterViewModel data)
        {
            if (!ModelState.IsValid || data == null)
                return Json(new { success = false, message = "Invalid input data." });

            var registered = await _authService.RegisterUserAsync(data);
            if (!registered)
                return Json(new { success = false, message = "Email already registered." });

            return Json(new
            {
                success = true,
                message = "Registered successfully",
                redirectUrl = Url.Action("Login", "Auth")
            });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        public async Task<IActionResult> ProfileView()
        {
            var sessionUserId = HttpContext.Session.GetString("userId");

            if (string.IsNullOrEmpty(sessionUserId))
                return Unauthorized();

            if (!int.TryParse(sessionUserId, out int userId))
                return BadRequest("Invalid session userId");

            var user = await _authService.GetUserProfileAsync(userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        public IActionResult EditProfile(int Id)
        {
            var user = _authService.GetUserById(Id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateUserDetails([FromBody] ProfileViewModel model)
        {
           
            var val = _authService.UpdateUserDetailAsync(model);
            if (!val.Result)
                return BadRequest(new { success = false, message = "Failed to update user details." });

            return Ok(new { success = true });
        }

    }
}
