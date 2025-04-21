namespace IdentityServiceApi.Models.ApiResponseModels.Users
{
	/// <summary>
	///      Represents the response returned by the users API when retrieving metrics for user states within the system.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2025
	/// </remarks>
	public class UserStateMetricsResponse
	{
		/// <summary>
		///     Gets or sets the total number of users in the system.
		/// </summary>
		public int TotalCount { get; set; }

		/// <summary>
		///     Gets or sets the number of activated users.
		/// </summary>
		public int ActivatedUsers { get; set; }

		/// <summary>
		///     Gets or sets the number of deactivated users.
		/// </summary>
		public int DeactivatedUsers { get; set; }
	}
}
