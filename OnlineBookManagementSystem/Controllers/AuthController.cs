using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.AuthViewModels;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController // Remove BaseController if it sets layout based on claims � auth views should force auth layout
    {
        private readonly IAuthService _authService;
        private readonly IActivityLogger _activityLoggerService;

        public AuthController(IAuthService authService, IActivityLogger activityLoggerService)
        {
            _authService = authService;
            _activityLoggerService = activityLoggerService;
        }

        //public IActionResult Index() => RedirectToAction(nameof(Login));
        public IActionResult Index() => View();
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> LoginData([FromBody] LoginViewModel data)
        {
            var (success, message, user) = await _authService.ValidateUserAsync(data);
            if (!success)
                return Json(new { success = false, message });

            var (accessToken, refreshToken) = _authService.GenerateTokens(user);
            var roles = await _authService.GetUserRolesAsync(user.Id);

            // FIXED: Assume SuperAdmin uses same AdminIndex or create proper dashboard
            // Change if you have real /SuperAdmin/Dashboard
            string redirectUrl = roles.Contains("SuperAdmin") ? "/SuperAdmin/Dashboard" :
                                 roles.Contains("Admin") ? "/Admin/Dashboard" :
                                 "/User/Dashboard";

            await _activityLoggerService.LogAsync("Login", $"User {user.Name} logged in.", user.Id);

            SetAccessTokenCookie(accessToken);

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

        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenViewModel model)
        {
            var result = await _authService.RefreshTokenAsync(model.RefreshToken);
            if (!result.Success)
            {
                DeleteAccessTokenCookie();
                return Json(new { success = false, message = result.Message });
            }

            SetAccessTokenCookie(result.AccessToken);

            return Json(new
            {
                success = true,
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken
            });
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")] // Keep on logout � ensures only authenticated can logout
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var id) ? id : 0;

            await _authService.RevokeRefreshTokensAsync(userId);
            // await _activityLoggerService.LogAsync("Logout", "User logged out.", userId);

            DeleteAccessTokenCookie();

            return RedirectToAction("Login");
        }

        private void SetAccessTokenCookie(string token)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps, // Only secure in HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            };
            Response.Cookies.Append("accessToken", token, options);
        }

        private void DeleteAccessTokenCookie()
        {
            Response.Cookies.Delete("accessToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps, // Only secure in HTTPS
                SameSite = SameSiteMode.Strict
            });
        }

        // Registration & Password reset � pure auth
        public IActionResult Registration() => View();

        [HttpPost]
        public async Task<IActionResult> SaveData([FromBody] RegisterViewModel data)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var (success, message, token) = await _authService.RegisterUserAsync(data);
            if (!success)
                return Json(new { success = false, message });

            // Since user is pending approval, token is null and we redirect to a pending page or login with message
            return Json(new
            {
                success = true,
                message,
                redirectUrl = Url.Action("Login") // Or a "PendingApproval" page
            });
        }

        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var confirmed = await _authService.ConfirmEmailAsync(token, email);
            ViewBag.IsSuccess = confirmed; // View uses IsSuccess
            ViewBag.Message = confirmed ? "Email confirmed! Please log in." : "Invalid token.";
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid email." });

            await _authService.GeneratePasswordResetTokenAsync(model.Email); // fire and forget for security
            return Json(new { success = true, message = "If email exists, reset link sent." });
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var success = await _authService.UpdatePasswordAsync(model.Token, model.NewPassword);
            return Json(new { success, message = success ? "Password reset successfully. Please login." : "Invalid/expired token." });
        }

        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> ProfileView()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var id) ? id : 0;
            
            if (userId == 0)
                return RedirectToAction("Login");

            var userProfile = await _authService.GetUserProfileAsync(userId);
            if (userProfile == null)
                return RedirectToAction("Login");

            var roles = await _authService.GetUserRolesAsync(userId);
            
            var viewModel = new ProfileViewModel
            {
                Id = userProfile.Id,
                Name = userProfile.Name,
                Email = userProfile.Email,
                Roles = roles,
                CreatedAt = userProfile.CreatedAt,
                LastLoginDate = userProfile.LastLoginDate
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "UserOrHigher")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var id) ? id : 0;
            
            if (userId == 0 || userId != model.Id)
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var success = await _authService.UpdateUserDetailAsync(model);
                if (success)
                {
                    await _activityLoggerService.LogAsync("ProfileUpdate", "User profile updated", userId);
                    return Json(new { success = true, message = "Profile updated successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update profile" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating profile" });
            }
        }
    }
}