using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineBookManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("ApiPolicy")]
[Produces("application/json")]
public abstract class ApiBaseController : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = "Success")
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
    }

    protected IActionResult Error(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null,
            Timestamp = DateTime.UtcNow
        });
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; }
}