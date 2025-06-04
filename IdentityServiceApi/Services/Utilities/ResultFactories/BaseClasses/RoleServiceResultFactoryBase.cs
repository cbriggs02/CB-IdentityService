using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Authorization;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.BaseClasses
{
    /// <summary>
    ///     Base class for creating role service results, used for both successful and failed role operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public abstract class RoleServiceResultFactoryBase : ServiceResultFactory, IRoleServiceResultFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleServiceResultFactoryBase"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The parameter validator used to validate input parameters.
        /// </param>
        protected RoleServiceResultFactoryBase(IParameterValidator parameterValidator) : base(parameterValidator)
        {
        }

        /// <summary>
        ///     Creates a failed role operation service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract RoleServiceResult RoleOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful role operation service result with the specified role data.
        /// </summary>
        /// <param name="role">
        ///     The <see cref="RoleDTO"/> representing the successfully found role.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleServiceResult"/> containing the success status and the role data.
        /// </returns>
        public abstract RoleServiceResult RoleOperationSuccess(RoleDTO role);
    }
}
