using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.AuthViewModels;
using OnlineBookManagementSystem.Presentation.ViewModels.User;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IActivityLogger _activityLoggerService;

        public AuthController(IAuthService authService, IActivityLogger activityLoggerService)
        {
            _authService = authService;
            _activityLoggerService = activityLoggerService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Force auth layout for all auth actions
            ViewData["Layout"] = "_LayoutAuth";
            // Don't call base.OnActionExecuting to avoid BaseController's layout logic
        }

        //public IActionResult Index() => RedirectToAction(nameof(Login));

        public async Task<IActionResult> Login()
        {
            var enhancedViewModel = new EnhancedLoginViewModel
            {
                RoleCapabilities = GetRoleCapabilities(),
                SystemStats = await GetSystemStatsAsync(),
                RecentFeatures = GetRecentFeatures()
            };

            return View(enhancedViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoginData([FromBody] LoginViewModel data)
        {
            var (success, message, user) = await _authService.ValidateUserAsync(data);
            if (!success)
                return Json(new { success = false, message });

            var (accessToken, refreshToken) = await _authService.GenerateTokensAsync(user);
            var roles = await _authService.GetUserRolesAsync(user.Id);

            // FIXED: Assume SuperAdmin uses same AdminIndex or create proper dashboard
            // Change if you have real /SuperAdmin/Dashboard
            string redirectUrl = roles.Contains("SuperAdmin") ? "/SuperAdmin/Dashboard" :
                                 roles.Contains("Admin") ? "/Admin/Dashboard" :
                                 roles.Contains("User") ? "/User/Dashboard" : "/Public/Index";

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
                roles,
                debug = new
                {
                    cookieSet = true,
                    tokenLength = accessToken.Length,
                    userIdClaim = user.Id.ToString()
                }
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
        public async Task<IActionResult> Registration()
        {
            var enhancedViewModel = new EnhancedRegisterViewModel
            {
                RoleCapabilities = GetRoleCapabilities(),
                OnboardingInfo = GetOnboardingInfo(),
                RoleDescriptions = GetRoleDescriptions(),
                SystemStats = await GetSystemStatsAsync()
            };

            return View(enhancedViewModel);
        }

        // Debug endpoint to test authentication
        [HttpGet]
        [Authorize]
        public IActionResult TestAuth()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var authType = User.Identity?.AuthenticationType;
            var name = User.Identity?.Name;

            return Json(new
            {
                isAuthenticated,
                authType,
                name,
                claims,
                cookieExists = Request.Cookies.ContainsKey("accessToken"),
                cookieValue = Request.Cookies["accessToken"]?.Substring(0, Math.Min(20, Request.Cookies["accessToken"]?.Length ?? 0)) + "..."
            });
        }

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
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating profile" });
            }
        }

        #region Helper Methods for Enhanced Authentication Views

        private RoleCapabilitiesViewModel GetRoleCapabilities()
        {
            return new RoleCapabilitiesViewModel
            {
                User = new RoleInfoViewModel
                {
                    Name = "Standard User",
                    Description = "Perfect for book enthusiasts and readers",
                    Icon = "bi-person-fill",
                    BadgeColor = "success",
                    RequiresApproval = true,
                    ApprovalProcess = "Account activation via email confirmation",
                    Capabilities = new List<string>
                    {
                        "Browse and search extensive book catalog",
                        "View detailed book information and reviews",
                        "Add books to personal favorites list",
                        "Place and track book orders",
                        "Manage personal profile and preferences",
                        "Access order history and status updates"
                    }
                },
                Admin = new RoleInfoViewModel
                {
                    Name = "Retailer/Admin",
                    Description = "For bookstore owners and inventory managers",
                    Icon = "bi-briefcase-fill",
                    BadgeColor = "primary",
                    RequiresApproval = true,
                    ApprovalProcess = "SuperAdmin review and approval required",
                    Capabilities = new List<string>
                    {
                        "All User capabilities included",
                        "Manage book inventory and catalog",
                        "Add, edit, and remove books",
                        "Process and manage customer orders",
                        "View sales analytics and reports",
                        "Manage book categories and classifications",
                        "Access admin dashboard with business insights"
                    }
                },
                SuperAdmin = new RoleInfoViewModel
                {
                    Name = "System Administrator",
                    Description = "Full system access and control",
                    Icon = "bi-shield-fill-check",
                    BadgeColor = "danger",
                    RequiresApproval = false,
                    ApprovalProcess = "System-level access only",
                    Capabilities = new List<string>
                    {
                        "All Admin and User capabilities",
                        "User account management and approval",
                        "System configuration and settings",
                        "Advanced analytics and reporting",
                        "Security monitoring and audit logs",
                        "Database management and maintenance"
                    }
                }
            };
        }

        private OnboardingInfoViewModel GetOnboardingInfo()
        {
            return new OnboardingInfoViewModel
            {
                WelcomeMessage = "Welcome to Whispering Pages! Your journey into our comprehensive book management system starts here.",
                ExpectedApprovalTime = "Account activation typically takes 5-10 minutes via email confirmation. Admin accounts require manual approval within 24-48 hours.",
                ContactInfo = "Need help? Contact our support team or check our documentation.",
                Steps = new List<OnboardingStepViewModel>
                {
                    new OnboardingStepViewModel
                    {
                        Order = 1,
                        Title = "Create Account",
                        Description = "Fill out the registration form with your details",
                        Icon = "bi-person-plus-fill",
                        IsActive = true
                    },
                    new OnboardingStepViewModel
                    {
                        Order = 2,
                        Title = "Email Verification",
                        Description = "Check your email and click the verification link",
                        Icon = "bi-envelope-check-fill"
                    },
                    new OnboardingStepViewModel
                    {
                        Order = 3,
                        Title = "Account Approval",
                        Description = "Admin accounts require SuperAdmin approval",
                        Icon = "bi-shield-check-fill"
                    },
                    new OnboardingStepViewModel
                    {
                        Order = 4,
                        Title = "Start Exploring",
                        Description = "Access your personalized dashboard and features",
                        Icon = "bi-rocket-takeoff-fill"
                    }
                }
            };
        }

        private Dictionary<string, string> GetRoleDescriptions()
        {
            return new Dictionary<string, string>
            {
                { "User", "Standard access for browsing, ordering, and managing personal book preferences" },
                { "Admin", "Business access for inventory management, order processing, and sales analytics" }
            };
        }

        private async Task<SystemStatsViewModel> GetSystemStatsAsync()
        {
            try
            {
                // In a real implementation, these would come from actual services
                // For now, we'll return sample data that represents the system's capabilities
                return new SystemStatsViewModel
                {
                    TotalBooks = 2547,
                    TotalCategories = 28,
                    ActiveUsers = 156,
                    AverageRating = 4.3m,
                    CompletedOrders = 892,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception)
            {
                // Log error and return default stats
                return new SystemStatsViewModel
                {
                    TotalBooks = 0,
                    TotalCategories = 0,
                    ActiveUsers = 0,
                    AverageRating = 0,
                    CompletedOrders = 0,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }

        private List<string> GetRecentFeatures()
        {
            return new List<string>
            {
                "Enhanced search with advanced filters",
                "Real-time order tracking system",
                "Improved mobile responsive design",
                "New analytics dashboard for admins",
                "Automated inventory management"
            };
        }

        #endregion
    }
}
