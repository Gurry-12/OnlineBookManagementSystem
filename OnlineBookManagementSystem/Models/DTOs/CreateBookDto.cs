using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Models.DTOs
{
    public class CreateBookDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string? Author { get; set; }

        [Required]
        public decimal Price { get; set; }

        [StringLength(20)]
        public string? ISBN { get; set; }

        public int StockQuantity { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int? CategoryId { get; set; }
    }
}
