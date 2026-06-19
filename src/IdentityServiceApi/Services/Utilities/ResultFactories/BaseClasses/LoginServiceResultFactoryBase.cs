using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.ServiceResultModels.Authentication;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.AbstractClasses
{
    /// <summary>
    ///     Base class for creating login service results, used for both successful and failed login operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public abstract class LoginServiceResultFactoryBase : ServiceResultFactory, ILoginServiceResultFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginServiceResultFactoryBase"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The parameter validator used to validate input parameters.
        /// </param>
        protected LoginServiceResultFactoryBase(IParameterValidator parameterValidator) : base(parameterValidator)
        {
        }

        /// <summary>
        ///     Creates a failed login service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract LoginServiceResult LoginOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful login service result with a token.
        /// </summary>
        /// <param name="token">
        ///     The authentication token generated upon successful login.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> containing the success status and the token.
        /// </returns>
        public abstract LoginServiceResult LoginOperationSuccess(string token);
    }
}
