using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.User
{
    public class OrderHistoryViewModel
    {
        public List<OrderHistoryItemViewModel> Orders { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}