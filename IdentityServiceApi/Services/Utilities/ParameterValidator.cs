﻿using IdentityServiceApi.Interfaces.Utilities;
using System.Collections;

namespace IdentityServiceApi.Services.Utilities
{
    /// <summary>
    ///     Provides utility methods for validating parameters in services to 
    ///     enforce consistency in parameter validation throughout the application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class ParameterValidator : IParameterValidator
    {
        /// <summary>
        ///     Validates that a string parameter is not null or empty.
        ///     If the parameter is null or an empty string, an <see cref="ArgumentNullException"/>
        ///     is thrown, indicating that the parameter is required.
        /// </summary>
        /// <param name="parameter">
        ///     The string parameter to validate.
        /// </param>
        /// <param name="parameterName">
        ///     The name of the parameter (used in the exception message).
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="parameter"/> is null or empty.
        /// </exception>
        public void ValidateNotNullOrEmpty(string parameter, string parameterName)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException(parameterName);
            }
        }


        /// <summary>
        ///     Validates that an object parameter is not null and if it is an IEnumerable, 
        ///     checks that it is not empty. If the parameter is null, an <see cref="ArgumentNullException"/>
        ///     is thrown with a message indicating that the parameter cannot be null.
        /// </summary>
        /// <param name="parameter">
        ///     The object parameter to validate.
        /// </param>
        /// <param name="parameterName">
        ///     The name of the parameter (used in the exception message).
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="parameter"/> is null.
        /// </exception>
        public void ValidateObjectNotNull(object parameter, string parameterName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(parameterName, $"{parameterName} cannot be null.");
            }

            if (parameter is IEnumerable enumerable && !enumerable.Cast<object>().Any())
            {
                throw new ArgumentException($"{parameterName} cannot be an empty collection.", parameterName);
            }
        }
    }
}
