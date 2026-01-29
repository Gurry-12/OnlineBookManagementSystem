using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Presentation.ViewModels.AuthViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an account type")]
        public string RequestedRole { get; set; } = "User"; // "User" or "Admin"
        
        // Enhanced registration properties
        public bool AcceptTerms { get; set; } = false;
        public bool SubscribeToUpdates { get; set; } = false;
        public string ReferralSource { get; set; } = string.Empty;
    }

    public class EnhancedRegisterViewModel : RegisterViewModel
    {
        public RoleCapabilitiesViewModel RoleCapabilities { get; set; } = new();
        public OnboardingInfoViewModel OnboardingInfo { get; set; } = new();
        public List<string> AvailableRoles { get; set; } = new() { "User", "Admin" };
        public Dictionary<string, string> RoleDescriptions { get; set; } = new();
        public SystemStatsViewModel SystemStats { get; set; } = new();
    }

    public class OnboardingInfoViewModel
    {
        public string WelcomeMessage { get; set; } = string.Empty;
        public List<OnboardingStepViewModel> Steps { get; set; } = new();
        public string ExpectedApprovalTime { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }

    public class OnboardingStepViewModel
    {
        public int Order { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public bool IsActive { get; set; } = false;
    }
}
