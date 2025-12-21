using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }  // FK

    [StringLength(450)]
    public string Token { get; set; } = string.Empty;  // Hashed

    [Column(TypeName = "datetimeoffset")]
    public DateTimeOffset ExpiryDate { get; set; }

    public bool IsRevoked { get; set; } = false;

    [Column(TypeName = "datetimeoffset")]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    [StringLength(450)]
    public string? ReplacedByToken { get; set; }

    [StringLength(45)]
    public string? CreatedByIp { get; set; }

    public virtual User User { get; set; } = null!;
}