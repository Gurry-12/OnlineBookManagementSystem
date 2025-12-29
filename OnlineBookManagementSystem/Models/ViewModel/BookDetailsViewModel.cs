namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class BookDetailsViewModel : Book
    {
        public Book Book { get; set; } = new();
        public new string Title { get; set; }

        public new bool IsFavorite { get; set; }
        public List<Book> RelatedBooks { get; set; } = new();
        public List<Models.BookReview> Reviews { get; set; } = new();
        public new double AverageRating { get; set; }
        public int ReviewCount { get; set; }

    }
}
