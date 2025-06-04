using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Authorization;
using IdentityServiceApi.Services.Utilities.ResultFactories.BaseClasses;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.Authorization
{
    /// <summary>
    ///     Factory class for creating role service results, used for both successful and failed role operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class RoleServiceResultFactory : RoleServiceResultFactoryBase, IRoleServiceResultFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleServiceResultFactory"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The parameter validator used to validate input parameters.
        /// </param>
        public RoleServiceResultFactory(IParameterValidator parameterValidator) : base(parameterValidator)
        {
        }

        /// <summary>
        ///     Creates a failed role service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public override RoleServiceResult RoleOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new RoleServiceResult { Success = false, Errors = errors.ToList() };
        }

        /// <summary>
        ///     Creates a successful role operation result with a role DTO.
        /// </summary>
        /// <param name="role">
        ///     The <see cref="RoleDTO"/> representing the role information.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleServiceResult"/> containing the success status and role data.
        /// </returns>
        public override RoleServiceResult RoleOperationSuccess(RoleDTO role)
        {
            _parameterValidator.ValidateObjectNotNull(role, nameof(role));
            return new RoleServiceResult { Success = true, Role = role };
        }
    }
}
