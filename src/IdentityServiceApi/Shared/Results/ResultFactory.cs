using IdentityServiceApi.Shared.Utilities;

namespace IdentityServiceApi.Shared.ResultFactories
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
    public class ResultFactory(IParameterValidator parameterValidator) : IResultFactory
    {
        protected readonly IParameterValidator _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));

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
        /// <returns>
        ///     A <see cref="Result"/> indicating failure along with the provided errors.
        /// </returns>
        public Result GeneralOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new Result { Success = false, Errors = [.. errors] };
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
