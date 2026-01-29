using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Core.Application.Interfaces;

namespace OnlineBookManagementSystem.Presentation.Controllers
{
    [Authorize(Policy = "AdminOrHigher")]
    [Route("api/[controller]")]
    public class HealthController : BaseController
    {
        private readonly BookManagementContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(BookManagementContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetSystemStatus()
        {
            try
            {
                var status = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Database = await CheckDatabaseHealth(),
                    Memory = GetMemoryUsage(),
                    Uptime = GetUptime(),
                    ActiveUsers = await GetActiveUsersCount(),
                    SystemLoad = GetSystemLoad()
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
            }
        }

        private async Task<object> CheckDatabaseHealth()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var userCount = await _context.Users.CountAsync();
                var bookCount = await _context.Books.CountAsync();
                
                return new
                {
                    Connected = canConnect,
                    UserCount = userCount,
                    BookCount = bookCount,
                    Status = canConnect ? "Healthy" : "Unhealthy"
                };
            }
            catch (Exception ex)
            {
                return new { Status = "Unhealthy", Error = ex.Message };
            }
        }

        private object GetMemoryUsage()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            return new
            {
                WorkingSetMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                PrivateMemoryMB = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2)
            };
        }

        private object GetUptime()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var uptime = DateTime.Now - process.StartTime;
            return new
            {
                Days = uptime.Days,
                Hours = uptime.Hours,
                Minutes = uptime.Minutes,
                TotalMinutes = Math.Round(uptime.TotalMinutes, 2)
            };
        }

        private async Task<int> GetActiveUsersCount()
        {
            // Users active in last 24 hours
            var yesterday = DateTime.UtcNow.AddDays(-1);
            return await _context.Users
                .Where(u => u.LastLoginDate >= yesterday)
                .CountAsync();
        }

        private object GetSystemLoad()
        {
            return new
            {
                ProcessorCount = Environment.ProcessorCount,
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                Is64BitOS = Environment.Is64BitOperatingSystem
            };
        }
    }
}
