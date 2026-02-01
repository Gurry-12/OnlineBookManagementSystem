namespace OnlineBookManagementSystem.Presentation.Models
{
    /// <summary>
    /// Represents the result of a command operation.
    /// Provides a consistent way to return success/failure from handlers.
    /// </summary>
    public class CommandResult
    {
        public bool Successes { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }

        public static CommandResult Success(string message)
        {
            return new CommandResult
            {
                Successes = true,
                Message = message
            };
        }

        public static CommandResult Failure(string message, Dictionary<string, string[]>? errors = null)
        {
            return new CommandResult
            {
                Successes = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
