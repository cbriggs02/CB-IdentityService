using Asp.Versioning;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Models.ApiResponseModels.Countries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IdentityServiceApi.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to countries.
    ///     This controller processes all incoming requests related to countries and delegates
    ///     them to the country service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[Controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CountriesController"/> class.
        /// </summary>
        /// <param name="countryService">
        ///     The country service used for retrieving country data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="countryService"/> is null.
        /// </exception>
        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
        }

        /// <summary>
        ///     Asynchronously retrieves a list of available countries from the system and delegates 
        ///     the request to the required service.
        /// </summary>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a list of country objects.
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no countries are available.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.  
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CountriesListResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.CountriesApi.GetCountries)]
        public async Task<ActionResult<CountriesListResponse>> GetCountriesAsync()
        {
            var result = await _countryService.GetCountriesAsync();
            if (result.Countries == null || !result.Countries.Any())
            {
                return NoContent();
            }

            return Ok(new CountriesListResponse { Countries = result.Countries });
        }
    }
}
