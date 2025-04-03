using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Services.UserManagement
{
	/// <summary>
	///     Provides country-related services for managing country information, including retrieving 
	///     a list of all countries and finding a specific country by its unique identifier.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio  
	///     @Created: 2025  
	/// </remarks>
	public class CountryService : ICountryService
	{
		private readonly ApplicationDbContext _context;

		/// <summary>
		///     Initializes a new instance of the <see cref="CountryService"/> class.
		///     The constructor accepts an instance of <see cref="ApplicationDbContext"/> 
		///     to interact with the database.
		/// </summary>
		/// <param name="context">
		///     The database context to be used for retrieving country data.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     Thrown if the <paramref name="context"/> is <c>null</c>.
		/// </exception>
		public CountryService(ApplicationDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

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
			var countries = await _context.Countries
				.OrderBy(x => x.Name)
				.AsNoTracking()
				.ToListAsync();

			return new CountryServiceListResult { Countries = countries };
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
		public async Task<Country> FindCountryByIdAsync(int id)
		{
			return await _context.Countries
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Id == id);
		}
	}
}
