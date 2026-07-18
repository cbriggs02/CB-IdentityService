using IdentityServiceApi.Features.UserManagement.Models.Entities;

namespace IdentityServiceApi.Features.UserManagement.Models.Responses
{
    /// <summary>
    ///     Represents the response model for the list of countries.
    ///     This model is used to return a list of countries from the API.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public class CountriesListResponse
    {
        /// <summary>
        ///     The list of countries retrieved from the service.
        /// </summary>
        public IEnumerable<Country> Countries { get; set; } = [];
    }
}
