namespace OnlineBookManagementSystem.Models
{
    public class ErrorViewModel
    {
        /// <summary>
        /// The unique ID of the request (for tracking).
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// The HTTP Status Code (e.g., 404, 500).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// A user-friendly title for the error.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// A user-friendly message explaining what happened.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Technical details or stack trace (only for Development).
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Helper to determine if RequestId should be shown.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
