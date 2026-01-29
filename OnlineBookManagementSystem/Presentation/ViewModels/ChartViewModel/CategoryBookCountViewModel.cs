namespace OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;

public class CategoryBookCountViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public int BookCount { get; set; }
    public int Count { get; set; } // Separate property for compatibility
}
