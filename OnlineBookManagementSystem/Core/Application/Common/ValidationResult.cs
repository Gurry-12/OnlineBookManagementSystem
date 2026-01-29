namespace OnlineBookManagementSystem.Core.Application.Common
{
    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, List<string>> FieldErrors { get; set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Failure(params string[] errors) => new()
        {
            IsValid = false,
            Errors = errors.ToList()
        };

        public static ValidationResult Failure(Dictionary<string, List<string>> fieldErrors) => new()
        {
            IsValid = false,
            FieldErrors = fieldErrors
        };

        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }

        public void AddFieldError(string field, string error)
        {
            IsValid = false;
            if (!FieldErrors.ContainsKey(field))
                FieldErrors[field] = new List<string>();
            FieldErrors[field].Add(error);
        }
    }
}