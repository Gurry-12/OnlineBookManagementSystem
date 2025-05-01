namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class AdminViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public User User { get; set; }
    }
}