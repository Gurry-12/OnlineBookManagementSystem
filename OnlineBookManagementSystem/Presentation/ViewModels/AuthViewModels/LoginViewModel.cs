using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Presentation.ViewModels.AuthViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
        
        // Portfolio context properties
        public bool ShowPortfolioContext { get; set; } = true;
        public string ReturnUrl { get; set; } = string.Empty;
    }

    public class EnhancedLoginViewModel : LoginViewModel
    {
        public RoleCapabilitiesViewModel RoleCapabilities { get; set; } = new();
        public SystemStatsViewModel SystemStats { get; set; } = new();
        public List<string> RecentFeatures { get; set; } = new();
    }

    public class RoleCapabilitiesViewModel
    {
        public RoleInfoViewModel User { get; set; } = new();
        public RoleInfoViewModel Admin { get; set; } = new();
        public RoleInfoViewModel SuperAdmin { get; set; } = new();
    }

    public class RoleInfoViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Capabilities { get; set; } = new();
        public string Icon { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; } = false;
        public string ApprovalProcess { get; set; } = string.Empty;
    }

    public class SystemStatsViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalCategories { get; set; }
        public int ActiveUsers { get; set; }
        public decimal AverageRating { get; set; }
        public int CompletedOrders { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
