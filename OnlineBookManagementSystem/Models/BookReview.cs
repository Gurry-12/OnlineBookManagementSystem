using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

public class BookReview
{
    public int Id { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review text must be between 10 and 1000 characters")]
    public string ReviewText { get; set; } = string.Empty;

    [Required]
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public int? ModeratedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime? ModeratedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public virtual Book Book { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual User? Moderator { get; set; }
}

public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Flagged = 3
}