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
    ///    
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
        /// 
        /// </summary>
        /// <returns></returns>
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
