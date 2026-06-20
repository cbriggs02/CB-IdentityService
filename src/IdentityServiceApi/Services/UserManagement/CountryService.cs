using IdentityServiceApi.Data;
using IdentityServiceApi.Helpers.CacheKeys;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection.Metadata.Ecma335;

namespace IdentityServiceApi.Services.UserManagement
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
        private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        /// <summary>
        ///     Asynchronously retrieves a list of all available countries in the system, 
        ///     ordered alphabetically by their name.
        /// </summary>
        /// <returns>
        ///     A <see cref="CountryServiceListResult"/> containing a collection of <see cref="Country"/> 
        ///     objects sorted by their name.
        /// </returns>
        public async Task<CountryServiceListResult> GetCountriesAsync()
        {
            if (_cache.TryGetValue(CountriesCacheKeys.CountryList, out CountryServiceListResult? cachedCountries) && cachedCountries != null)
            {
                return cachedCountries;
            }
            var countries = await _context.Countries
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();

            var result = new CountryServiceListResult { Countries = countries };
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove);

            _cache.Set(CountriesCacheKeys.CountryList, cachedCountries, cacheOptions);
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
        public async Task<Country?> FindCountryByIdAsync(int id) =>
            await _context.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
    }
}
