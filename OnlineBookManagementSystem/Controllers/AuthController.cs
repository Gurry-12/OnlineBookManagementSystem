using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Controllers
{

    public class AuthController : BaseController
    {
        private readonly IAuthInterface _authService;
        private readonly IActivityLogger _activityLoggerService;
        public AuthController(IAuthInterface authService, IActivityLogger activityLoggerService)
        {
            _authService = authService;
            _activityLoggerService = activityLoggerService;
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
            if (user.Role == "User")
            {
                await _activityLoggerService.LogAsync("Login", $"User {user.Name} logged in.");
            }
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

            if (data.Role == "User")
            {
                await _activityLoggerService.LogAsync("Register", $"User {data.Name} Registered.");
            }
            return Json(new
            {
                success = true,
                message = "Registered successfully",
                redirectUrl = Url.Action("Login", "Auth")
            });
        }

        public IActionResult Logout()
        {
            var sessionUserId = HttpContext.Session.GetString("userId");
            var userId = int.Parse(sessionUserId);
            var user = _authService.GetUserById(userId);
           if(user != null) { 
                _activityLoggerService.LogAsync("Logout", $"User with ID {user.Name} logged out.", userId);
            }
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

        public IActionResult UpdateUserDetails([FromBody] ProfileViewModel model)
        {

            var val = _authService.UpdateUserDetailAsync(model);
            if (!val.Result)
                return BadRequest(new { success = false, message = "Failed to update user details." });

            return Ok(new { success = true });
        }

        [HttpDelete]
        public IActionResult DeleteUser(int id)
        {
            var user = _authService.GetUserById(id);
            if (user == null)
                return NotFound();
            user.IsDeleted = true;
            _authService.UpdateUserDetailAsync(user);

             _activityLoggerService.LogAsync("Delete", $"User with ID {user.Name} was soft-deleted.", id);
            return Ok(new { success = true, message = "User deleted successfully." });
        }
    }
}