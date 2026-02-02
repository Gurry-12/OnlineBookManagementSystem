using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Presentation.ViewModels.Logs;

namespace OnlineBookManagementSystem.Presentation.Controllers;

[Authorize(Policy = "AdminOrHigher")]
public class LogsController : BaseController
{
    private readonly IActivityLogger _activityLogger;

    public LogsController(IActivityLogger activityLogger)
    {
        _activityLogger = activityLogger;
    }

    [HttpGet]
    public async Task<IActionResult> LogList(
        int page = 1,
        string? searchTerm = null,
        string? actionType = null,
        string? userName = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        try
        {
            var capabilities = GetLogCapabilities();

            if (!capabilities.CanView)
            {
                return View(new LogListViewModel
                {
                    Capabilities = capabilities
                });
            }

            var filters = new LogFilters
            {
                SearchTerm = searchTerm,
                ActionType = actionType,
                UserName = userName,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            // If no date range specified, default to last 7 days
            if (!dateFrom.HasValue && !dateTo.HasValue)
            {
                filters.DateFrom = DateTime.Today.AddDays(-7);
                filters.DateTo = DateTime.Today.AddDays(1).AddSeconds(-1);
            }

            const int pageSize = 50;
            var logs = await GetFilteredLogs(filters, capabilities, page, pageSize);
            var totalLogs = await GetTotalLogsCount(filters, capabilities);
            var totalPages = (int)Math.Ceiling((double)totalLogs / pageSize);

            var viewModel = new LogListViewModel
            {
                Logs = logs,
                Filters = filters,
                Capabilities = capabilities,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalLogs = totalLogs,
                PageSize = pageSize,
                LogsToday = await GetLogsCount(DateTime.Today, DateTime.Today.AddDays(1).AddSeconds(-1), capabilities),
                LogsThisWeek = await GetLogsCount(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1).AddSeconds(-1), capabilities)
            };

            // Set layout based on user role
            ViewData["Layout"] = User.IsInRole("SuperAdmin") ? "_LayoutSuperAdmin" : "_LayoutAdmin";

            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Error loading activity logs.";
            return RedirectToAction("Dashboard", GetDashboardController());
        }
    }

    [HttpGet]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> LogDetails(int id)
    {
        try
        {
            var capabilities = GetLogCapabilities();

            if (!capabilities.CanView || !capabilities.CanViewSensitiveData)
            {
                return View(new LogDetailViewModel
                {
                    Capabilities = capabilities
                });
            }

            var log = await GetLogById(id);
            if (log == null)
            {
                TempData["ErrorMessage"] = "Log entry not found.";
                return RedirectToAction("LogList");
            }

            var relatedLogs = await GetRelatedLogs(log, capabilities);

            var viewModel = new LogDetailViewModel
            {
                Log = log,
                Capabilities = capabilities,
                RelatedLogs = relatedLogs
            };

            ViewData["Layout"] = "_LayoutSuperAdmin";
            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Error loading log details.";
            return RedirectToAction("LogList");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportLogs(
        string? searchTerm = null,
        string? actionType = null,
        string? userName = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var capabilities = GetLogCapabilities();

        if (!capabilities.CanExport)
        {
            return Forbid();
        }

        try
        {
            var filters = new LogFilters
            {
                SearchTerm = searchTerm,
                ActionType = actionType,
                UserName = userName,
                DateFrom = dateFrom ?? DateTime.Today.AddDays(-30),
                DateTo = dateTo ?? DateTime.Today.AddDays(1).AddSeconds(-1)
            };

            var logs = await GetFilteredLogs(filters, capabilities, 1, int.MaxValue);

            // Generate CSV content
            var csv = GenerateLogsCsv(logs, capabilities);
            var fileName = $"activity_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Error exporting logs.";
            return RedirectToAction("LogList");
        }
    }

    [HttpGet]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> ExportLogEntry(int id)
    {
        var capabilities = GetLogCapabilities();

        if (!capabilities.CanExport)
        {
            return Forbid();
        }

        try
        {
            var log = await GetLogById(id);
            if (log == null)
            {
                return NotFound();
            }

            var json = System.Text.Json.JsonSerializer.Serialize(log, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"log_entry_{id}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Error exporting log entry.";
            return RedirectToAction("LogDetails", new { id });
        }
    }

    // Helper methods
    private LogCapabilities GetLogCapabilities()
    {
        var isAdmin = User.IsInRole("Admin");
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        return new LogCapabilities
        {
            CanView = isAdmin || isSuperAdmin,
            CanViewAllUsersLogs = isSuperAdmin, // Admin can only see their own logs
            CanExport = isSuperAdmin, // Only SuperAdmin can export
            CanViewSensitiveData = isSuperAdmin // Only SuperAdmin sees IP, User Agent, etc.
        };
    }

    private string GetDashboardController()
    {
        return User.IsInRole("SuperAdmin") ? "SuperAdmin" : "Admin";
    }

    private string GenerateLogsCsv(List<OnlineBookManagementSystem.Core.Domain.Entities.ActivityLog> logs, LogCapabilities capabilities)
    {
        var csv = new System.Text.StringBuilder();

        // Headers
        var headers = new List<string> { "Timestamp", "Action", "Description", "User" };
        if (capabilities.CanViewSensitiveData)
        {
            headers.AddRange(new[] { "IP Address", "User Agent" });
        }
        csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

        // Data rows
        foreach (var log in logs)
        {
            var row = new List<string>
            {
                $"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\"",
                $"\"{log.ActionType}\"",
                $"\"{log.Description?.Replace("\"", "\"\"")}\"",
                $"\"{log.User?.Name ?? "System"}\""
            };

            if (capabilities.CanViewSensitiveData)
            {
                row.Add($"\"{log.IpAddress ?? ""}\"");
                row.Add($"\"{log.UserAgent?.Replace("\"", "\"\"") ?? ""}\"");
            }

            csv.AppendLine(string.Join(",", row));
        }

        return csv.ToString();
    }

    // Placeholder methods - would need to implement these with actual data access
    private async Task<List<OnlineBookManagementSystem.Core.Domain.Entities.ActivityLog>> GetFilteredLogs(
        LogFilters filters, LogCapabilities capabilities, int page, int pageSize)
    {
        // Implementation would go here - filter logs based on capabilities and filters
        // Admin users would only see their own logs unless CanViewAllUsersLogs is true
        return new List<OnlineBookManagementSystem.Core.Domain.Entities.ActivityLog>();
    }

    private async Task<int> GetTotalLogsCount(LogFilters filters, LogCapabilities capabilities)
    {
        // Implementation would go here
        return 0;
    }

    private async Task<int> GetLogsCount(DateTime from, DateTime to, LogCapabilities capabilities)
    {
        // Implementation would go here
        return 0;
    }

    private async Task<OnlineBookManagementSystem.Core.Domain.Entities.ActivityLog?> GetLogById(int id)
    {
        // Implementation would go here
        return null;
    }

    private async Task<List<OnlineBookManagementSystem.Core.Domain.Entities.ActivityLog>> GetRelatedLogs(
        OnlineBookManagementSystem.Core.Domain.Entities.ActivityLog log, LogCapabilities capabilities)
    {
        // Implementation would go here - find logs from same user, same action type, etc.
        return new List<OnlineBookManagementSystem.Core.Domain.Entities.ActivityLog>();
    }
}
