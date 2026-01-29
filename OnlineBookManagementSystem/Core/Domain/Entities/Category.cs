namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class Category : BaseEntity
    {
        private string _name = string.Empty;

        public string Name 
        { 
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Category name cannot be null or empty", nameof(value));
                if (value.Length > 100)
                    throw new ArgumentException("Category name cannot exceed 100 characters", nameof(value));
                _name = value.Trim();
            }
        }

        public string? Description { get; set; }

        // Navigation properties
        private readonly List<Book> _books = new();
        public IReadOnlyCollection<Book> Books => _books.AsReadOnly();

        // Private constructor for EF Core
        public Category() { }

        public Category(string name, string? description = null)
        {
            Name = name;
            SetDescription(description);
        }

        public void UpdateName(string name)
        {
            Name = name;
            UpdateTimestamp();
        }

        public void SetDescription(string? description)
        {
            if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));
            
            Description = description?.Trim();
            UpdateTimestamp();
        }

        public int GetBookCount()
        {
            return _books.Count(b => !b.IsDeleted);
        }

        public bool HasBooks()
        {
            return _books.Any(b => !b.IsDeleted);
        }

        // Internal method for EF Core to add books
        internal void AddBook(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));
            
            if (!_books.Contains(book))
            {
                _books.Add(book);
            }
        }

        // Internal method for EF Core to remove books
        internal void RemoveBook(Book book)
        {
            if (book != null)
            {
                _books.Remove(book);
            }
        }
    }
}