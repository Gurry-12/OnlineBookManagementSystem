using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    internal class AdminOrderListViewModel
    {
        public List<Order> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
    }
}