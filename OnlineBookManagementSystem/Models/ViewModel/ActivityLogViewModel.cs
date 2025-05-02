namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class ActivityLogViewModel
    {
        public string ActionType { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserName { get; set; }
        public string TimeAgo { get; set; } = "";
    }

}
