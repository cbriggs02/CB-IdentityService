using IdentityServiceApi.Models.RequestModels.UserManagement;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Interfaces.UserManagement
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for password-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IPasswordService
    {
        /// <summary>
        ///     Asynchronously sets a new password for a user identified by their ID in the system.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user for whom the password is being set.
        /// </param>
        /// <param name="request">
        ///     A model object that contains the new password and its confirmation for validation.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object that indicates the outcome of the password setting process.
        /// </returns>
        Task<ServiceResult> SetPasswordAsync(string id, SetPasswordRequest request);

        /// <summary>
        ///     Asynchronously updates the password of a user identified by their ID in the system.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user whose password is being updated.
        /// </param>
        /// <param name="request">
        ///     A model object that contains the current password and the new password for updating.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object that indicates the outcome of the password update process.
        /// </returns>
        Task<ServiceResult> UpdatePasswordAsync(string id, UpdatePasswordRequest request);
    }
}
