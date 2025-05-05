using Asp.Versioning;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Models.ApiResponseModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IdentityServiceApi.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to roles.
    ///     This controller processes all incoming requests related to role management and delegates
    ///     them to the role service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[Controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : ControllerBase
	{
		private readonly IRoleService _roleService;

		/// <summary>
		///     Initializes a new instance of the <see cref="RolesController"/> 
		///     class with the specified dependencies.
		/// </summary>
		/// <param name="roleService">
		///     roles service used for all role-related operations.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     Thrown if any of the parameters are null.
		/// </exception>
		public RolesController(IRoleService roleService)
		{
			_roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
		}

		/// <summary>
		///     Asynchronously processes requests for retrieving a list of all roles in the system,
		///     delegating the operation to the required service.
		/// </summary>
		/// <returns>
		///     - <see cref="StatusCodes.Status200OK"/> (OK) with a list of identity roles.    
		///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no roles are found in the system.  
		///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who 
		///         is not authenticated or does not have the required role.
		///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.   
		/// </returns>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RolesListResponse))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[SwaggerOperation(Summary = ApiDocumentation.RolesApi.GetRoles)]
		public async Task<ActionResult<RolesListResponse>> GetRolesAsync()
		{
			var result = await _roleService.GetRolesAsync();
			if (result.Roles == null || !result.Roles.Any())
			{
				return NoContent();
			}

			return Ok(new RolesListResponse { Roles = result.Roles });
		}
	}
}
