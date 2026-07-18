using IdentityServiceApi.Data;
using IdentityServiceApi.Features.UserManagement.Caching;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Provides country-related services for managing country information, including retrieving 
    ///     a list of all countries and finding a specific country by its unique identifier.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public class CountryService(IMemoryCache cache, ApplicationDbContext context) : ICountryService
    {
        /// <summary>
        ///     Asynchronously retrieves a list of all available countries in the system, 
        ///     ordered alphabetically by their name.
        /// </summary>
        /// <returns>
        ///     A <see cref="CountryListResult"/> containing a collection of <see cref="Country"/> 
        ///     objects sorted by their name.
        /// </returns>
        public async Task<CountryListResult> GetCountriesAsync()
        {
            if (cache.TryGetValue(CountriesCacheKeys.CountryList, out CountryListResult? cachedCountries) && cachedCountries != null)
            {
                return cachedCountries;
            }

            var countries = await context.Countries
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();

            var result = new CountryListResult { Countries = countries };
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove);

            cache.Set(CountriesCacheKeys.CountryList, cachedCountries, cacheOptions);
            return result;
        }

        /// <summary>
        ///     Asynchronously finds a country by its unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the country to retrieve.
        /// </param>
        /// <returns>
        ///     A <see cref="Country"/> object if found; otherwise, null.
        /// </returns>
        public async Task<Country?> FindCountryByIdAsync(int id)
        {
            return await context.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
