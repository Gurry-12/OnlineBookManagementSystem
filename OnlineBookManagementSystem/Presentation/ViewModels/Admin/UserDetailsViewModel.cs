namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Name => FullName; // Compatibility property
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        // Additional details
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int FavoriteBooks { get; set; }
        public int ReviewsWritten { get; set; }
    }
}