using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

[Table("AspNetUsers")]  // Identity table name
public partial class User : IdentityUser<int>  // PK int for consistency
{
    [PersonalData]  // GDPR
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool? IsDeleted { get; set; } = false;

    // From prior upgrades: Email confirmation & reset
    public bool IsEmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; }
    public string? PasswordResetToken { get; set; }  // Hashed
    public DateTime? PasswordResetExpiry { get; set; }

    // Timestamps
    [Column(TypeName = "DateTime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "DateTime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Existing nav props
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();  // New
}