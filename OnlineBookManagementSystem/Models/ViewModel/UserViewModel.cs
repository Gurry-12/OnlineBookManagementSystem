
namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int CartItemCount { get; set; }
        public List<string> Roles { get; internal set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
