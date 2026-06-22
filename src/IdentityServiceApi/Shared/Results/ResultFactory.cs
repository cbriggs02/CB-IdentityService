namespace IdentityServiceApi.Shared.Results
{
    /// <summary>
    ///     Implements the <see cref="IResultFactory"/> interface to create uniform service result 
    ///     objects for various operations within the application. This factory reduces code duplication 
    ///     by centralizing the result creation logic for service operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class ResultFactory : IResultFactory
    {
        /// <summary>
        ///     Creates a successful service result for general operations.
        /// </summary>
        /// <returns>
        ///     A <see cref="Result"/> indicating success.
        /// </returns>
        public Result GeneralOperationSuccess() => new() { Success = true };

        /// <summary>
        ///     Creates a failed service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <param name="errorType">
        ///     An <see cref="ErrorType"/> indicating the type of error that occurred during the operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Result"/> indicating failure along with the provided errors.
        /// </returns>
        public Result GeneralOperationFailure(string[] errors, ErrorType errorType) =>
            new() { Success = false, Errors = [.. errors], ErrorType = errorType };
    }
}
