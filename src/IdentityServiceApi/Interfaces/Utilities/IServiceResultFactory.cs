using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Interfaces.Utilities
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
    /// </remarks>
    public interface IServiceResultFactory
    {
        /// <summary>
        ///     Creates a service result indicating a general failure 
        ///     in an operation, including error messages.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> object indicating failure.
        /// </returns>
        ServiceResult GeneralOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a service result indicating a successful operation 
        ///     without any additional data.
        /// </summary>
        /// <returns>
        ///     A <see cref="ServiceResult"/> object indicating success.
        /// </returns>
        ServiceResult GeneralOperationSuccess();
    }
}
