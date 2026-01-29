namespace OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;

public class AuthorBookCountViewModel
{
    public string AuthorName { get; set; } = string.Empty;
    public int BookCount { get; set; }
    public int Count { get; set; } // Separate property for compatibility
}
