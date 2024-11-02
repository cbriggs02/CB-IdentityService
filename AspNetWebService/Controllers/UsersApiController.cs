﻿using Microsoft.AspNetCore.Mvc;
using AspNetWebService.Models.DataTransferObjectModels;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using AspNetWebService.Constants;
using AspNetWebService.Models.ApiResponseModels.UsersApiResponses;
using AspNetWebService.Interfaces.UserManagement;
using AspNetWebService.Models.RequestModels.UserManagement;
using AspNetWebService.Models.ApiResponseModels.CommonApiResponses;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to users.
    ///     This controller processes all incoming requests related to user management and delegates
    ///     them to the user service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UsersApiController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="userService">
        ///     User service used for all user-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public UsersApiController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }


        /// <summary>
        ///     Asynchronously processes requests to retrieve a paginated list of users from the system, based on 
        ///     the provided page number and page size, optional account status for filtering and delegates 
        ///     to the required service.
        /// </summary>
        /// <param name="request">
        ///     A model containing pagination details, such as the page number and page size
        ///     and account status for optional filtering.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a list of user DTO objects and pagination 
        ///         metadata in headers ("X-Pagination").
        ///     
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no users are found for the specified page.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user 
        ///         who is not authenticated or does not have the required role.
        /// </returns>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUsersApiResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.GetUsers)]
        public async Task<ActionResult<GetUsersApiResponse>> GetUsers([FromQuery] UserListRequest request)
        {
            var result = await _userService.GetUsers(request);

            if (result.Users == null || !result.Users.Any())
            {
                return NoContent();
            }

            var response = new GetUsersApiResponse
            {
                Users = result.Users,
                PaginationMetadata = result.PaginationMetadata
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(response.PaginationMetadata));

            return Ok(response);
        }


        /// <summary>
        ///     Asynchronously processes requests for retrieving a user from the system by the provided ID, 
        ///     delegating the operation to the appropriate service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to retrieve.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a user DTO object if the user is found.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user
        ///         who is not authenticated or does not have the required role.
        ///     
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user tries to retrieve
        ///         another user's account or admin tries to retrieve another admins account.
        ///     
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.GetUserById)]
        public async Task<ActionResult<UserApiResponse>> GetUser([FromRoute][Required] string id)
        {
            var result = await _userService.GetUser(id);

            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }
            }

            return Ok(new UserApiResponse { User = result.User });
        }


        /// <summary>
        ///     Asynchronously processes requests for creating a new user in the system, delegating the operation 
        ///     to the required service.
        /// </summary>
        /// <param name="user">
        ///     The UserDTO object containing the new user's information.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status201Created"/> (Created) with a User DTO of the newly created user.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of validation or creation 
        ///         errors encountered during user creation.
        /// </returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.CreateUser)]
        public async Task<ActionResult<UserApiResponse>> CreateUser([FromBody] UserDTO user)
        {
            var result = await _userService.CreateUser(user);

            var response = new UserApiResponse { User = result.User };

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetUser), new { id = response.User.UserName }, response);
            }
            else
            {
                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }


        /// <summary>
        ///     Asynchronously processes requests for updating a user account in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user account to update.
        /// </param>
        /// <param name="user">
        ///     The UserDTO object containing the updated user information.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if the user account was successfully updated.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of validation or update errors 
        ///         encountered during the operation.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who is not 
        ///         authenticated or does not have the required role.
        ///     
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts to update another 
        ///         user's account or a admin attempts to update another admins account.
        ///     
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user account is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.UpdateUser)]
        public async Task<IActionResult> UpdateUser([FromRoute][Required] string id, [FromBody] UserDTO user)
        {
            var result = await _userService.UpdateUser(id, user);

            if (result.Success)
            {
                return NoContent();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }


        /// <summary>
        ///     Asynchronously processes requests for deleting a user account in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user account to delete.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the user account deletion was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors encountered during 
        ///         the user account deletion.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who is 
        ///         not authenticated or does not have the required role.
        ///     
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts to delete another 
        ///         user's account or a admin tries to delete another admin account. 
        ///     
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user account is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.DeleteUser)]
        public async Task<IActionResult> DeleteUser([FromRoute][Required] string id)
        {
            var result = await _userService.DeleteUser(id);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }


        /// <summary>
        ///     Asynchronously processes requests for activating a user account in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the user account to activate.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the user account activation was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors encountered
        ///         during the user account activation.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user 
        ///         who is not authenticated or does not have the required role.
        ///
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts to activate another 
        ///         user's account or a admin tries to activate another admin account. 
        ///         
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user account is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPut("activate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.ActivateUser)]
        public async Task<IActionResult> ActivateUser([FromRoute][Required] string id)
        {
            var result = await _userService.ActivateUser(id);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }


        /// <summary>
        ///     Asynchronously processes requests for deactivating a user account in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user account to deactivate.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the user account deactivation was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors encountered 
        ///         during the user account deactivation.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who 
        ///         is not authenticated or does not have the required role.
        /// 
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts to deactivate another 
        ///         user's account or a admin tries to deactivate another admin account.
        ///         
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user account is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPut("deactivate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.DeactivateUser)]
        public async Task<IActionResult> DeactivateUser([FromRoute][Required] string id)
        {
            var result = await _userService.DeactivateUser(id);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }
    }
}
