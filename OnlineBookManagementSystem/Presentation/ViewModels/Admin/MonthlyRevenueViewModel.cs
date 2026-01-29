namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin
{
    public class MonthlyRevenueViewModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}