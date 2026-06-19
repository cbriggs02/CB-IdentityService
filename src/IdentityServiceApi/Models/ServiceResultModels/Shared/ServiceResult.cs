namespace IdentityServiceApi.Models.ServiceResultModels.Shared
{
    /// <summary>
    ///     Represents the uniform model representing the result of a service operation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class ServiceResult
    {
        /// <summary>
        ///     Indicates whether the service operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Contains errors encountered during the service operation, if any.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
