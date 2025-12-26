namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class AdminUsersViewModel
    {
        public List<UserWithRoleViewModel> Users { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }
        public string? SelectedRole { get; set; }
    }
}