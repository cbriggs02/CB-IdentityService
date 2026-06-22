using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.UserManagement.Interfaces
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for user-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public interface IUserService
	{
		/// <summary>
		///     Asynchronously retrieves a list of users in the system.
		/// </summary>
		/// <param name="request">
		///     A model containing information used in the request, such as a page number and page size.
		/// </param>
		/// <returns>
		///     A task representing the asynchronous operation that returns a <see cref="UserListResult"/> object.
		/// </returns>
		Task<UserListResult> GetUsersAsync(UserListRequest request);

        /// <summary>
        ///     Asynchronously retrieves a user by ID from the system.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to be retrieved.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="UserResult"/> object.
        /// </returns>
        Task<UserResult> GetUserAsync(string id);

        /// <summary>
        ///     Asynchronously creates a new user in the system using the specified user data transfer object.
        /// </summary>
        /// <param name="userDTO">
        ///     A data transfer object containing information used for user creation.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="UserResult"/> object.
        /// </returns>
        Task<UserResult> CreateUserAsync(UserDTO userDTO);

		/// <summary>
		///     Asynchronously updates a user in the system by ID using the specified user data transfer object.
		/// </summary>
		/// <param name="id">
		///     The ID used to locate the user in the system to be updated.
		/// </param>
		/// <param name="userDTO">
		///     An object model containing data to be adjusted in the located user object model.
		/// </param>
		/// <returns>
		///     A task representing the asynchronous operation that returns a <see cref="Result"/> object.
		/// </returns>
		Task<Result> UpdateUserAsync(string id, UserDTO userDTO);

		/// <summary>
		///     Asynchronously deletes a user in the system by ID.
		/// </summary>
		/// <param name="id">
		///     The ID used to locate the user to be deleted in the system.
		/// </param>
		/// <returns>
		///     A task representing the asynchronous operation that returns a <see cref="Result"/> object.
		/// </returns>
		Task<Result> DeleteUserAsync(string id);

		/// <summary>
		///     Asynchronously activates a user in the system by ID.
		/// </summary>
		/// <param name="id">
		///     The ID to identify the user to be activated within the system.
		/// </param>
		/// <returns>
		///     A task representing the asynchronous operation that returns a <see cref="Result"/> object.
		/// </returns>
		Task<Result> ActivateUserAsync(string id);

		/// <summary>
		///     Asynchronously deactivates a user in the system by ID.
		/// </summary>
		/// <param name="id">
		///     The ID of the user who is being deactivated within the system.
		/// </param>
		/// <returns>
		///     A task representing the asynchronous operation that returns a <see cref="Result"/> object.
		/// </returns>
		Task<Result> DeactivateUserAsync(string id);

        /// <summary>
        ///     Asynchronously assigns a specified role to a user identified by their ID.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to whom the role will be assigned.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role that is being assigned to the user.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role assignment.
        /// </returns>
        Task<Result> AssignRoleAsync(string id, string roleName);

        /// <summary>
        ///     Asynchronously removes an assigned role from a user identified by their ID.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to whom the role will be removed.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role removal.
        /// </returns>
        Task<Result> RemoveRoleAsync(string id);
    }
}
