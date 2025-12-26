using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

public class BookRatingCache
{
    [Key]
    public int BookId { get; set; }

    [Required]
    [Column(TypeName = "real")]
    public double AverageRating { get; set; }

    [Required]
    public int TotalReviews { get; set; }

    [Required]
    [Column(TypeName = "datetime")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Book Book { get; set; } = null!;
}