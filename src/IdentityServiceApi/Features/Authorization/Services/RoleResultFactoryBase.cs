using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Shared.ResultFactories;
using IdentityServiceApi.Shared.Utilities;

namespace IdentityServiceApi.Features.Authorization.Services
{
    /// <summary>
    ///     Base class for creating role service results, used for both successful and failed role operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    /// <remarks>
    ///     Initializes a new instance of the <see cref="RoleResultFactoryBase"/> class.
    /// </remarks>
    /// <param name="parameterValidator">
    ///     The parameter validator used to validate input parameters.
    /// </param>
    public abstract class RoleResultFactoryBase(IParameterValidator parameterValidator) : ResultFactory(parameterValidator), IRoleResultFactory
    {

        /// <summary>
        ///     Creates a failed role operation service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract RoleResult RoleOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful role operation service result with the specified role data.
        /// </summary>
        /// <param name="role">
        ///     The <see cref="RoleDTO"/> representing the successfully found role.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleResult"/> containing the success status and the role data.
        /// </returns>
        public abstract RoleResult RoleOperationSuccess(RoleDTO role);
    }
}
