using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.AuthViewModels;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
{
    [AllowAnonymous]  // Per-action overrides
    public class AuthController : BaseController
    {
        private readonly IAuthInterface _authService;
        private readonly IActivityLogger _activityLoggerService;  // Assume exists

        public AuthController(IAuthInterface authService, IActivityLogger activityLoggerService)
        {
            _authService = authService;
            _activityLoggerService = activityLoggerService;
        }

        public IActionResult Index() => RedirectToAction(nameof(Login));

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> LoginData([FromBody] LoginViewModel data)
        {
            var (success, message, user) = await _authService.ValidateUserAsync(data);
            if (!success)
                return Json(new { success = false, message });

            var (accessToken, refreshToken) = _authService.GenerateTokens(user);
            var roles = await _authService.GetUserRolesAsync(user.Id);

            string redirectUrl = roles.Contains("SuperAdmin") ? "/SuperAdmin/Dashboard" :
                                 roles.Contains("Admin") ? "/Books/AdminIndex" :
                                 "/Books/UserIndex";

            // Log activity
            await _activityLoggerService.LogAsync("Login", $"User {user.Name} logged in.", user.Id);

            return Json(new
            {
                success = true,
                message = "Login successful",
                accessToken,
                refreshToken,
                redirectUrl,
                userName = user.Name,
                roles
            });
        }

        public IActionResult Registration() => View();

        [HttpPost]
        public async Task<IActionResult> SaveData([FromBody] RegisterViewModel data)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var (success, message, token) = await _authService.RegisterUserAsync(data);
            if (!success)
                return Json(new { success = false, message });

            return Json(new
            {
                success = true,
                message,
                confirmationToken = token,
                redirectUrl = Url.Action("ConfirmEmail", new { token, email = data.Email })
            });
        }

        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var confirmed = await _authService.ConfirmEmailAsync(token, email);
            ViewBag.Success = confirmed;
            ViewBag.Message = confirmed ? "Email confirmed! Please log in." : "Invalid token.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid email." });

            var token = await _authService.GeneratePasswordResetTokenAsync(model.Email);
            // Always return success for security (no email leak)
            return Json(new { success = true, message = "If email exists, reset link sent." });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var success = await _authService.UpdatePasswordAsync(model.Token, model.NewPassword);
            return Json(new { success, message = success ? "Password reset." : "Invalid/expired token." });
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> ProfileView()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var profile = await _authService.GetUserProfileAsync(userId);
            if (profile == null) return NotFound();
            return View(profile);
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> UpdateUserDetails([FromBody] ProfileViewModel model)
        {
            var success = await _authService.UpdateUserDetailAsync(model);
            return Json(new { success, message = success ? "Updated." : "Failed." });
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPost]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleViewModel model)
        {
            var success = await _authService.AssignRoleAsync(model.UserId, model.Role);
            return Json(new { success });
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _authService.RevokeRefreshTokensAsync(userId);
            await _activityLoggerService.LogAsync("Logout", "User logged out.", userId);
            return Json(new { success = true, redirectUrl = "/Auth/Login" });
        }

        // New view for SuperAdmin role management
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _authService.ManageUsers();
            // Project to VM in prod
            return View(users);
        }
    }
}

