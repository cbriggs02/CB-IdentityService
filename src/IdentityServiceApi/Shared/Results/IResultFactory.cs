namespace IdentityServiceApi.Shared.ResultFactories
{
    /// <summary>
    ///     Provides methods for creating uniform service result objects 
    ///     for various operations within the application. This factory 
    ///     centralizes the creation logic for service results, ensuring 
    ///     consistency and reducing duplication in the codebase.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public interface IResultFactory
    {
        /// <summary>
        ///     Creates a service result indicating a general failure 
        ///     in an operation, including error messages.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="Result"/> object indicating failure.
        /// </returns>
        Result GeneralOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a service result indicating a successful operation 
        ///     without any additional data.
        /// </summary>
        /// <returns>
        ///     A <see cref="Result"/> object indicating success.
        /// </returns>
        Result GeneralOperationSuccess();
    }
}
