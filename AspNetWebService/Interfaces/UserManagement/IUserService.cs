﻿using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.RequestModels.UserRequests;
using AspNetWebService.Models.ServiceResultModels.UserServiceResults;

namespace AspNetWebService.Interfaces.UserManagement
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for user-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
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
        ///     A task representing the asynchronous operation that returns a <see cref="UserServiceListResult"/> object.
        /// </returns>
        Task<UserServiceListResult> GetUsers(UserListRequest request);


        /// <summary>
        ///     Asynchronously retrieves a user by ID from the system.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to be retrieved.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="UserServiceResult"/> object.
        /// </returns>
        Task<UserServiceResult> GetUser(string id);


        /// <summary>
        ///     Asynchronously creates a new user in the system using the specified user data transfer object.
        /// </summary>
        /// <param name="userDTO">
        ///     A data transfer object containing information used for user creation.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="UserServiceResult"/> object.
        /// </returns>
        Task<UserServiceResult> CreateUser(UserDTO userDTO);


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
        ///     A task representing the asynchronous operation that returns a <see cref="UserServiceResult"/> object.
        /// </returns>
        Task<UserServiceResult> UpdateUser(string id, UserDTO userDTO);


        /// <summary>
        ///     Asynchronously deletes a user in the system by ID.
        /// </summary>
        /// <param name="id">
        ///     The ID used to locate the user to be deleted in the system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="UserServiceResult"/> object.
        /// </returns>
        Task<UserServiceResult> DeleteUser(string id);


        /// <summary>
        ///     Asynchronously activates a user in the system by ID.
        /// </summary>
        /// <param name="id">
        ///     The ID to identify the user to be activated within the system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="UserServiceResult"/> object.
        /// </returns>
        Task<UserServiceResult> ActivateUser(string id);


        /// <summary>
        ///     Asynchronously deactivates a user in the system by ID.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user who is being deactivated within the system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="UserServiceResult"/> object.
        /// </returns>
        Task<UserServiceResult> DeactivateUser(string id);
    }
}
