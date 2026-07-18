namespace IdentityServiceApi.Shared.Models
{
    /// <summary>
    ///     Represents errors encountered during service operations.
    ///     This list of errors is returned by the API and models the universal API error response
    ///     for all bad requests or failed operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class ErrorResponse
    {
        /// <summary>
        ///     Contains a list of errors encountered during the service operation, if any.
        ///     Used as API responses for bad requests or other error scenarios.
        /// </summary>
        public List<string> Errors { get; set; } = [];
    }
}
