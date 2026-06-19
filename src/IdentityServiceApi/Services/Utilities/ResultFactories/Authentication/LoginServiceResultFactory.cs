using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.ServiceResultModels.Authentication;
using IdentityServiceApi.Services.Utilities.ResultFactories.AbstractClasses;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.Authentication
{
    /// <summary>
    ///     Factory class responsible for creating login service results, 
    ///     both for successful and failed login operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class LoginServiceResultFactory : LoginServiceResultFactoryBase, ILoginServiceResultFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginServiceResultFactory"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The parameter validator used to validate input parameters.
        /// </param>
        public LoginServiceResultFactory(IParameterValidator parameterValidator) : base(parameterValidator)
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
        public override LoginServiceResult LoginOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new LoginServiceResult { Success = false, Errors = errors.ToList() };
        }

        /// <summary>
        ///     Creates a successful login service result with a token.
        /// </summary>
        /// <param name="token">
        ///     The authentication token generated upon successful login.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> containing the success status and the token.
        /// </returns>
        public override LoginServiceResult LoginOperationSuccess(string token)
        {
            _parameterValidator.ValidateNotNullOrEmpty(token, nameof(token));
            return new LoginServiceResult { Success = true, Token = token };
        }
    }
}
