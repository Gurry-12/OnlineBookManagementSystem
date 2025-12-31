using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class AdminOrderListViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        
        // Additional properties for statistics
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
    }
}