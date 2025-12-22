namespace OnlineBookManagementSystem.Models.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public decimal Price { get; set; }
        public string? ISBN { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
