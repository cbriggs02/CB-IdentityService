using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Services.Utilities;

namespace IdentityServiceApi.Services.Logging
{
    /// <summary>
    ///     Validates logging-related parameters, specifically ensuring that context data used for logging is not null 
    ///     or empty. This class extends <see cref="ParameterValidator"/> and implements <see cref="ILoggingValidator"/>.
    ///     It is used to enforce validation rules for logging context data across various logging services.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class LoggingValidator : ParameterValidator, ILoggingValidator
    {
        /// <summary>
        ///     Validates that the provided context data is not null or empty.
        ///     This method is typically called before logging actions to ensure that important context 
        ///     information (such as user ID, IP address, or request path) is available.
        /// </summary>
        /// <param name="value">
        ///     The value to validate. This could represent user ID, IP address, or other logging context information.
        /// </param>
        /// <param name="fieldName">
        ///     The name of the field being validated (for error reporting purposes).
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the <paramref name="value"/> is null or empty. This ensures that all required 
        ///     logging context data is available before logging occurs.
        /// </exception>
        public void ValidateContextData(string value, string fieldName)
        {   
            ValidateNotNullOrEmpty(fieldName, nameof(fieldName));

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"{fieldName} cannot be null or empty.");
            }
        }
    }
}
