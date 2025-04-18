using IdentityServiceApi.Interfaces.Utilities;

namespace IdentityServiceApi.Interfaces.Logging
{
    /// <summary>
    ///     Defines a contract for validating logging-related context data.
    ///     This interface extends <see cref="IParameterValidator"/> and provides the method to 
    ///     validate context data (such as user ID, IP address, or request path) used in logging actions.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface ILoggingValidator : IParameterValidator
    {
        /// <summary>
        ///     Validates that the provided context data is not null or empty.
        ///     This method ensures that important context information required for logging is available.
        /// </summary>
        /// <param name="value">
        ///     The value representing the context data to validate (e.g., user ID, IP address, or request path).
        /// </param>
        /// <param name="fieldName">
        ///     The name of the field being validated, used for error reporting.
        /// </param>
        void ValidateContextData(string? value, string fieldName);
    }
}
