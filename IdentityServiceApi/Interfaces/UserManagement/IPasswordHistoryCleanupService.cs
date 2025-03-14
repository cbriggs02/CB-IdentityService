namespace IdentityServiceApi.Interfaces.UserManagement
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for PasswordHistoryCleaning-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public interface IPasswordHistoryCleanupService
    {
        /// <summary>
        ///     Asynchronously removes the password history for a specified user.
        /// </summary>
        /// <param name="userId">
        ///     The unique identifier of the user whose password history is being removed.
        /// </param>
        /// <returns>
        ///      A task representing the asynchronous operation of deleting the password history.
        /// </returns>
        Task DeletePasswordHistory(string userId);

        /// <summary>
        ///     Asynchronously removes old entries for a users password history only keeping most recent 5 records.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password history is being cleaned up.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of removing old password histories.
        /// </returns>
        Task RemoveOldPasswords(string id);
    }
}
