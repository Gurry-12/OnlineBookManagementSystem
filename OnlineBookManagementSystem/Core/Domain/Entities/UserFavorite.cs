namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class UserFavorite : BaseEntity
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow; // Alias for CreatedAt

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;

        // Private constructor for EF Core
        public UserFavorite() { }

        public UserFavorite(int userId, int bookId)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be positive", nameof(userId));
            if (bookId <= 0)
                throw new ArgumentException("BookId must be positive", nameof(bookId));

            UserId = userId;
            BookId = bookId;
            CreatedAt = DateTime.UtcNow;
            AddedAt = DateTime.UtcNow;
        }
    }
}