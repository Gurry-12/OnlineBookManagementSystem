namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class CategoryClassifyViewModel
    {
        public required string CategoryName { get; set; }
        public List<Book> Books { get; set; }
    }
}
