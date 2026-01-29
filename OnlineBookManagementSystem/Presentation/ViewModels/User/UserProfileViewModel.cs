using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Presentation.ViewModels.User;

public class UserProfileViewModel
{
    public int Id { get; set; }
    
    [Required]
    [Display(Name = "Full Name")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Email Address")]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Display(Name = "Phone Number")]
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
    
    [Display(Name = "Street Address")]
    [StringLength(200)]
    public string? Address { get; set; }
    
    [Display(Name = "City")]
    [StringLength(50)]
    public string? City { get; set; }
    
    [Display(Name = "State")]
    [StringLength(50)]
    public string? State { get; set; }
    
    [Display(Name = "ZIP Code")]
    [StringLength(10)]
    public string? ZipCode { get; set; }
    
    [Display(Name = "Country")]
    [StringLength(50)]
    public string? Country { get; set; }
    
    // Read-only properties for display
    public int TotalOrders { get; set; }
    public int TotalFavorites { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime MemberSince { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
}