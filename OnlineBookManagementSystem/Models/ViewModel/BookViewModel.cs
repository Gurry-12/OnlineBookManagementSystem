using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Book Title")]
        public string Title { get; set; } = null!;

        [Required]
        public string? Author { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal? Price { get; set; }

        public string? Isbn { get; set; }

        public string? ImgUrl { get; set; }

        [Required]
        public string? Stock { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public bool? IsFavorite { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
