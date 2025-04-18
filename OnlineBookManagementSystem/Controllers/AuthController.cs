using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        public async Task<IActionResult> LoginData([FromBody] LoginViewModel data)
        {
            if (data == null || string.IsNullOrEmpty(data.Email) || string.IsNullOrEmpty(data.Password))
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
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
                new Claim ("userId" , user.Id.ToString()),
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
            var val = string.Empty; // Default redirect URL
            if (user.Role == "Admin")
            {
                val = Url.Action("AdminIndex", "Books");
            }
            else if (user.Role == "User")
            {
                val = Url.Action("UserIndex", "Books");

            }
            // During login
            HttpContext.Session.SetString("userId", user.Id.ToString());

            return Json(new
            {
                success = true,
                message = "Login Successful",
                token = jwtToken,
                redirectUrl = val,
                userName = user.Name,
                role = user.Role // 👈 Add this
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

        
        public async Task<IActionResult> ProfileView()
        {
            var userIdClaim = HttpContext.Session.GetString("userId");

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!int.TryParse(userIdClaim, out int userId))
                return BadRequest("Invalid session userId");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        public IActionResult EditProfile(int Id)
        {
            
            var user = _context.Users.FirstOrDefault(u => u.Id == Id);
            if (user == null)
                return NotFound();
            return View(user);
        }
    }


}
