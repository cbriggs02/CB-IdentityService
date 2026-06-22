namespace IdentityServiceApi.Shared.Results
{
    /// <summary>
    ///     Defines the classification of errors that can occur during a service operation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public enum ErrorType
    {
        /// <summary>
        ///     Indicates that the request failed due to invalid input or validation errors.
        /// </summary>
        Validation,

        /// <summary>
        ///     Indicates that the request failed because the resource is not in an appropriate state for the requested operation.
        /// </summary>
        InvalidState,

        /// <summary>
        ///     Indicates that the request was well-formed but could not be processed due to semantic errors or business rule violations.
        /// </summary>
        UnprocessableEntity,

        /// <summary>
        ///     Indicates that the requested resource could not be found.
        /// </summary>
        NotFound,

        /// <summary>
        ///     Indicates that the request failed due to lack of authentication.
        /// </summary>
        Unauthorized,

        /// <summary>
        ///     Indicates that the request was authenticated but not permitted.
        /// </summary>
        Forbidden,
    }
}
