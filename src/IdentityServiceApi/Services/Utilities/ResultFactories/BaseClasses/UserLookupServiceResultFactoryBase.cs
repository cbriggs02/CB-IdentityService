using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.AbstractClasses
{
    /// <summary>
    ///     Base class for creating user lookup service results, used for 
    ///     both successful and failed user lookup operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public abstract class UserLookupServiceResultFactoryBase : ServiceResultFactory, IUserLookupServiceResultFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserLookupServiceResultFactoryBase"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The parameter validator used to validate input parameters.
        /// </param>
        protected UserLookupServiceResultFactoryBase(IParameterValidator parameterValidator) : base(parameterValidator)
        {
        }

        /// <summary>
        ///     Creates a failed user lookup service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract UserLookupServiceResult UserLookupOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful user lookup service result with the specified user.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="User"/> object representing the successfully looked up user.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> containing the success status and the user data.
        /// </returns>
        public abstract UserLookupServiceResult UserLookupOperationSuccess(User user);
    }
}
