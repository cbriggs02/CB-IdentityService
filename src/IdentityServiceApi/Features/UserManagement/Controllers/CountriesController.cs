using Asp.Versioning;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Responses;
using IdentityServiceApi.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IdentityServiceApi.Features.UserManagement.Controllers
{
    /// <summary>
    ///     Provides endpoints for retrieving country-related data.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025  
    ///     @Updated: 2026  
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/countries")]
    [AllowAnonymous]
    public class CountriesController(ICountryService countryService) : ControllerBase
    {
        private readonly ICountryService _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));

        /// <summary>
        ///     Retrieves all supported countries.
        /// </summary>
        /// <returns>
        ///     Returns a <see cref="CountriesListResponse"/> containing a collection of countries.
        /// </returns>
        /// <response code="200">
        ///     A list of countries was successfully retrieved.
        /// </response>
        /// <response code="204">
        ///     No countries were found.
        /// </response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CountriesListResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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