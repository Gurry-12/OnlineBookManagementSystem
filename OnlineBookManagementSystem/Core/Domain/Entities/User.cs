using Microsoft.AspNetCore.Identity;

namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        private string _name = string.Empty;

        public string Name 
        { 
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be null or empty", nameof(value));
                if (value.Length > 100)
                    throw new ArgumentException("Name cannot exceed 100 characters", nameof(value));
                _name = value.Trim();
            }
        }

        public bool IsDeleted { get; set; } = false;

        // Email confirmation & reset
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }

        // Approval workflow
        public bool IsPendingApproval { get; set; } = true;
        public DateTime? RequestDate { get; set; }
        public string? RequestedRole { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }

        // Contact Information
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Compatibility properties for RefreshToken (these are in RefreshToken entity)
        public string? RefreshToken => RefreshTokens.FirstOrDefault(rt => rt.IsActive())?.Token;
        public DateTime? RefreshTokenExpiryTime => RefreshTokens.FirstOrDefault(rt => rt.IsActive())?.ExpiryDate;

        // Navigation properties
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();
        public virtual ICollection<BookReview> ModeratedReviews { get; set; } = new List<BookReview>();
        public virtual ICollection<IdentityUserRole<int>> UserRoles { get; set; } = new List<IdentityUserRole<int>>();
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

        // Public constructor for EF Core
        public User() { }

        public User(string name, string email)
        {
            Name = name;
            Email = email;
            UserName = email;
            RequestDate = DateTime.UtcNow;
        }

        public void UpdateProfile(string name)
        {
            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEmailConfirmationToken(string token, DateTime expiry)
        {
            EmailConfirmationToken = token;
            EmailConfirmationTokenExpiry = expiry;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ConfirmEmail()
        {
            EmailConfirmed = true;
            EmailConfirmationToken = null;
            EmailConfirmationTokenExpiry = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPasswordResetToken(string token, DateTime expiry)
        {
            PasswordResetToken = token;
            PasswordResetExpiry = expiry;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearPasswordResetToken()
        {
            PasswordResetToken = null;
            PasswordResetExpiry = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RequestRole(string role)
        {
            RequestedRole = role;
            IsPendingApproval = true;
            RequestDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ApproveUser()
        {
            IsPendingApproval = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLastLogin()
        {
            LastLoginDate = DateTime.UtcNow;
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}