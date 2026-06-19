namespace IdentityServiceApi.Models.RequestModels.UserManagement
{
    /// <summary>
    ///     Represents the model used for storing a user's password in the password history,
    ///     to enforce password policies like preventing password reuse.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class StorePasswordHistoryRequest
    {
        /// <summary>
        ///     Gets or sets the ID of the user whose password is being recorded in history.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the hashed password being recorded in history.
        ///     Only the hashed version of the password is stored for security purposes.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;
    }
}
