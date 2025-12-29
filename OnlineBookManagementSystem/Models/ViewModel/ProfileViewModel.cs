using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "New Name")]
        public string? NewName { get; set; }
        
        [Display(Name = "New Email")]
        [EmailAddress]
        public string? NewEmail { get; set; }
        
        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
        
        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmNewPassword { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
