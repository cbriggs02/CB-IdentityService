using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.Common
{
    /// <summary>
    ///     Implements the <see cref="IServiceResultFactory"/> interface to create uniform service result 
    ///     objects for various operations within the application. This factory reduces code duplication 
    ///     by centralizing the result creation logic for service operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class ServiceResultFactory(IParameterValidator parameterValidator) : IServiceResultFactory
    {
        protected readonly IParameterValidator _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));

        /// <summary>
        ///     Creates a successful service result for general operations.
        /// </summary>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating success.
        /// </returns>
        public ServiceResult GeneralOperationSuccess() => new() { Success = true };
        
        /// <summary>
        ///     Creates a failed service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public ServiceResult GeneralOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new ServiceResult { Success = false, Errors = [.. errors] };
        }

        /// <summary>
        ///     Validates the provided errors array for null values.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages to validate.
        /// </param>
        protected void ValidateErrors(string[] errors) => 
            _parameterValidator.ValidateObjectNotNull(errors, nameof(errors));   
    }
}
