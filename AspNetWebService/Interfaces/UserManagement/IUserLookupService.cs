﻿using AspNetWebService.Models.ServiceResultModels.UserManagement;

namespace AspNetWebService.Interfaces.UserManagement
{
    /// <summary>
    ///     Defines the contract for user lookup operations within the 
    ///     user management functionality of the application. This interface 
    ///     provides methods to retrieve user information based on various 
    ///     criteria, ensuring a consistent approach to user data access.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface IUserLookupService
    {
        /// <summary>
        ///     Asynchronously retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to be retrieved.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{UserLookupServiceResult}"/> representing 
        ///     the asynchronous operation, containing the result of the user lookup 
        ///     which includes the user information if found, or error details if not.
        /// </returns>
        Task<UserLookupServiceResult> FindUserById(string id);


        /// <summary>
        ///     Asynchronously retrieves a user by their user name.
        /// </summary>
        /// <param name="id">
        ///     The user name of the user to be retrieved.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{UserLookupServiceResult}"/> representing 
        ///     the asynchronous operation, containing the result of the user lookup 
        ///     which includes the user information if found, or error details if not.
        /// </returns>
        Task<UserLookupServiceResult> FindUserByUsername(string userName);
    }
}
