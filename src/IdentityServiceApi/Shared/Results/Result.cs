namespace IdentityServiceApi.Shared.Results
{
    /// <summary>
    ///     Represents the uniform model representing the result of a service operation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class Result
    {
        /// <summary>
        ///     Indicates whether the service operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Contains errors encountered during the service operation, if any.
        /// </summary>
        public List<string> Errors { get; set; } = [];

        /// <summary>
        ///     Gets or sets the classification of the error that occurred.
        /// </summary>
        public ErrorType ErrorType { get; set; }
    }
}
