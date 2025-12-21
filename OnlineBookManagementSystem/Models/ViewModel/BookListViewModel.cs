namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class BookListViewModel
    {
        public IEnumerable<BookViewModel> Books { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

}
