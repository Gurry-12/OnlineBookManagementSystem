using System;
using System.Collections.Generic;

namespace OnlineBookManagementSystem.Models;

public partial class ActivityLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual User? User { get; set; }
}
