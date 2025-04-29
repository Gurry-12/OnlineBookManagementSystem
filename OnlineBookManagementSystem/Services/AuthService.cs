using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineBookManagementSystem.Services
{
    public class AuthService : IAuthInterface
    {
        private readonly BookManagementContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher;

        public AuthService(BookManagementContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _hasher = new PasswordHasher<User>();
        }

        public async Task<(bool Success, string Message, User User)> ValidateUserAsync(LoginViewModel data)
        {
            if (data == null || string.IsNullOrEmpty(data.Email) || string.IsNullOrEmpty(data.Password))
                return (false, "Invalid input data.", null);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
            if (user == null)
                return (false, "Invalid login credentials.", null);

            var result = _hasher.VerifyHashedPassword(user, user.Password, data.Password);
            if (result == PasswordVerificationResult.Failed)
                return (false, "Wrong password.", null);

            return (true, "Login successful.", user);
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMonths(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> RegisterUserAsync(RegisterViewModel data)
        {
            if (!string.IsNullOrEmpty(data.Email) && _context.Users.Any(u => u.Email == data.Email))
                return false;

            var user = new User
            {
                Name = data.Name,
                Email = data.Email,
                Password = _hasher.HashPassword(null, data.Password),
                Role = data.Role == "Admin" ? "Admin" : "User",
                IsDeleted = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserViewModel> GetUserProfileAsync(int userId)
        {

            return await _context.Users
                .Where(u => u.Id == userId && !(u.IsDeleted ?? true)) // Use null-coalescing operator to handle nullable bool
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    CartItemCount = u.ShoppingCarts
                        .Where(sc => !(sc.Book.IsDeleted ?? true) && !(sc.IsDeleted ?? true)) // Use null-coalescing operator to handle nullable bool
                        .Sum(sc => sc.Quantity.GetValueOrDefault())
                })
                .FirstOrDefaultAsync();

        }
        public User GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id && u.IsDeleted == false) ?? throw new InvalidOperationException("User not found.");
        }

        public async Task<bool> UpdateUserDetailAsync(ProfileViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id && u.IsDeleted == false);
            if (user == null)
                return false;

            user.Name = model.NewName;
            user.Email = model.NewEmail;

            await _context.SaveChangesAsync();
            return true;
        }

        public void UpdateUserDetailAsync(User user)
        {

            var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id && u.IsDeleted == false);
            if (existingUser != null)
            {
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.Password = user.Password;
                existingUser.Role = user.Role;
                existingUser.IsDeleted = user.IsDeleted;
                _context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("User not found.");
            }
        }
    }
}
