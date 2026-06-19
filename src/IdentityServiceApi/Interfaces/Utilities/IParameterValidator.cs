using System.Collections;

namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    ///     Defines a contract for parameter validation methods.
    ///     This interface specifies methods for validating input parameters 
    ///     in services, ensuring that they meet required conditions before 
    ///     being processed.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IParameterValidator
    {
        /// <summary>
        ///     Validates that a string parameter is not null or empty.
        ///     Implementations should throw an exception if the parameter
        ///     does not meet this requirement.
        /// </summary>
        /// <param name="parameter">
        ///     The string parameter to validate.
        /// </param>
        /// <param name="parameterName">
        ///     The name of the parameter (used in the exception message).
        /// </param>
        void ValidateNotNullOrEmpty(string? parameter, string parameterName);

        /// <summary>
        ///     Validates that an object parameter is not null.
        ///     Implementations should throw an exception if the 
        ///     parameter is null.
        /// </summary>
        /// <param name="parameter">
        ///     The object parameter to validate.
        /// </param>
        /// <param name="parameterName">
        ///     The name of the parameter (used in the exception message).
        /// </param>
        void ValidateObjectNotNull(object? parameter, string parameterName);

        /// <summary>
        ///     Validates that a collection parameter is neither null nor empty.
        ///     This method ensures that the provided collection contains at least one element.
        /// </summary>
        /// <param name="collection">
        ///     The collection to validate. This parameter must not be null or empty.
        /// </param>
        /// <param name="parameterName">
        ///     The name of the parameter being validated. Used in exception messages to identify the invalid parameter.
        /// </param>
        void ValidateCollectionNotEmpty(IEnumerable? collection, string parameterName);
    }
}
