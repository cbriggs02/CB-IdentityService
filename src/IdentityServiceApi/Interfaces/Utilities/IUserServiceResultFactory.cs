using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;

namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    ///     Interface for creating service results related to user operations.
    ///     This interface defines methods for creating both success and failure results
    ///     for user-related operations, such as creating or updating users, etc...
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IUserServiceResultFactory : IServiceResultFactory
    {
        /// <summary>
        ///     Creates a failed user operation service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        UserServiceResult UserOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful user operation service result with the specified user data.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> representing the successfully created or updated user.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> containing the success status and the user data.
        /// </returns>
        UserServiceResult UserOperationSuccess(UserDTO user);
    }
}
