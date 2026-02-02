namespace OnlineBookManagementSystem.Core.Domain.Enums
{
    public enum ReviewStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Flagged = 3
    }

    public static class ReviewStatusExtensions
    {
        public static string ToDisplayString(this ReviewStatus status)
        {
            return status switch
            {
                ReviewStatus.Pending => "Pending",
                ReviewStatus.Approved => "Approved",
                ReviewStatus.Rejected => "Rejected",
                ReviewStatus.Flagged => "Flagged",
                _ => status.ToString()
            };
        }

        public static ReviewStatus Parse(string value)
        {
            if (Enum.TryParse<ReviewStatus>(value, true, out var result))
                return result;

            throw new ArgumentException($"Invalid ReviewStatus value: {value}");
        }

        public static ReviewStatus? TryParse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Enum.TryParse<ReviewStatus>(value, true, out var result))
                return result;

            return null;
        }
    }
}
