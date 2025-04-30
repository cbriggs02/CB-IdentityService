namespace IdentityServiceApi.Models.DTO
{
    /// <summary>
    ///     Represents the user creation metrics for a specific date, including the number of users created on that date.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public class UserCreationStatDTO
    {
        /// <summary>
        ///     Gets or sets the date of user creation.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///     Gets or sets the count of users created on the specified date.
        /// </summary>
        public int Count { get; set; }
    }
}
