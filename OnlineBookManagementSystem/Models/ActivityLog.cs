using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookManagementSystem.Models;

public partial class ActivityLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(100)]
    public string Action { get; set; } = string.Empty;  // Renamed from ActionType

    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;  // Renamed from Description

    [StringLength(45)]
    public string? IpAddress { get; set; }  // New

    [StringLength(500)]
    public string? UserAgent { get; set; }  // New

    [StringLength(20)]
    public string Level { get; set; } = "Info";  // New

    [Column(TypeName = "datetime")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;  // Changed to Offset

    public virtual User? User { get; set; }
    public string ActionType { get; internal set; }
    public string? Description { get; internal set; }
}