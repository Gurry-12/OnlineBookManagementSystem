namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class CategoryViewModel
    {
        public required List<Category> CategoryList { get; set; }
        public required Category NewCategory { get; set; }
    }
}
