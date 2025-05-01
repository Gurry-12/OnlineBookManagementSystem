namespace OnlineBookManagementSystem.Interfaces
{
    public interface IActivityLogger
    {
        Task LogAsync(string actionType, string? description, int? userId = null);
    }

}
