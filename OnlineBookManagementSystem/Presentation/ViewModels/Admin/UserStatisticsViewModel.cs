namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin
{
    public class UserStatisticsViewModel
    {
        public int UserId { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int FavoriteBooks { get; set; }
        public int ReviewsWritten { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime MemberSince { get; set; }
        public double AverageOrderValue => TotalOrders > 0 ? (double)(TotalSpent / TotalOrders) : 0;
        public int OrdersThisMonth { get; set; }
        public decimal SpentThisMonth { get; set; }
        public int LoginCount { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}