namespace OnlineBookManagementSystem.Models.ViewModel.AuthViewModels
{
    // New VM for role assignment
    public class AssignRoleViewModel
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
