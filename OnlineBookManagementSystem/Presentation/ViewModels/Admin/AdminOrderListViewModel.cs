using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Admin;

public class AdminOrderListViewModel
{
    public List<AdminOrderItemViewModel> Orders { get; set; } = new List<AdminOrderItemViewModel>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    
    // Additional properties for statistics
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();
}