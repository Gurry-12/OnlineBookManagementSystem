using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using OnlineBookManagementSystem.Models;

using OnlineBookManagementSystem.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace OnlineBookManagementSystem.Controllers
{

    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher;

        public AuthController(BookManagementContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _hasher = new PasswordHasher<User>();
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
        public IActionResult LoginData([FromBody] LoginViewModel data)
        {
            if (data == null || string.IsNullOrEmpty(data.Email) || string.IsNullOrEmpty(data.Password))
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == data.Email);
            if (user == null)
            {
                return Json(new { success = false, message = "Invalid login credentials." });
            }

            var result = _hasher.VerifyHashedPassword(user, user.Password, data.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Json(new { success = false, message = "Wrong password." });
            }

            // Create JWT Token
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                // Add more claims as needed
                new Claim(ClaimTypes.Role, value: user.Role)
                
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Json(new
            {
                success = true,
                message = "Login Successful",
                token = jwtToken,
                redirectUrl = Url.Action("Index", "Books"),
                userName = user.Name
            });


            // With this corrected line:
            // return RedirectToAction("Index", "Books");
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveData([FromBody] RegisterViewModel data)
        {
            if (!ModelState.IsValid || data == null)
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            if (_context.Users.Any(u => u.Email == data.Email))
            {
                return Json(new { success = false, message = "Email already registered." });
            }

            var user = new User
            {
                Name = data.Name,
                Email = data.Email,
                Password = _hasher.HashPassword(null, data.Password),
                Role = data.Role == "Admin" ? "Admin" : "User",
            };


            _context.Users.Add(user);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Registered successfully",
                redirectUrl = Url.Action("Login", "Auth")
            });
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login", "Auth");
        }
    }
}
