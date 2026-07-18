using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;

namespace IdentityServiceApi.Features.UserManagement.Interfaces
{
    /// <summary>
    ///     Defines the contract for country-related operations within the system, 
    ///     providing methods to retrieve and search for country information.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public interface ICountryService
    {
        /// <summary>
        ///     Asynchronously retrieves a list of all available countries in the system, 
        ///     sorted in alphabetical order.
        /// </summary>
        /// <returns>
        ///     A <see cref="CountryListResult"/> containing a collection of country 
        ///     entities if available; otherwise, an empty list.
        /// </returns>
        Task<CountryListResult> GetCountriesAsync();

        /// <summary>
        ///     Asynchronously finds a country by its unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the country to retrieve.
        /// </param>
        /// <returns>
        ///     A <see cref="Country"/> object if a match is found; otherwise, null.
        /// </returns>
        Task<Country?> FindCountryByIdAsync(int id);
    }
}
