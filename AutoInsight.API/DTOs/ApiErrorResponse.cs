namespace AutoInsight.API.DTOs
{
    /// <summary>
    /// Represents a standardized error response from the API.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// A unique, machine-readable error code (e.g., "INVALID_VIN_LENGTH", "NHTSA_API_UNAVAILABLE").
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable message describing the error.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Optional: Details about specific validation errors, if applicable.
        /// </summary>
        public Dictionary<string, string[]>? Details { get; set; }
    }
}