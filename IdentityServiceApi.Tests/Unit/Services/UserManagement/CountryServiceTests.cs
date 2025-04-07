using IdentityServiceApi.Data;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Services.UserManagement;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityServiceApi.Tests.Unit.Services.UserManagement
{
    /// <summary>
    ///     Unit tests for the <see cref="CountryService"/> class.
    ///     This class contains test cases for various country service scenarios, verifying the 
    ///     behavior of the country service functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class CountryServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CountryService _countryService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CountryServiceTests"/> class.
        ///     This constructor sets up an in-memory database context for testing the <see cref="CountryService"/>.
        /// </summary>
        public CountryServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _countryService = new CountryService(_context);
        }

        /// <summary>
        ///     Cleans up the in-memory database context after each test.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }

        /// <summary>
        ///     Tests that the <see cref="CountryService.GetCountriesAsync"/> method correctly returns
        ///     a sorted list of countries based on the country name.
        ///     This ensures that the service retrieves countries in alphabetical order as expected.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task GetCountriesAsync_ReturnsSortedCountries()
        {
            // Arrange
            _context.Countries.AddRange(
                new Country { Id = 4, Name = "CountryB" },
                new Country { Id = 5, Name = "CountryA" },
                new Country { Id = 6, Name = "CountryC" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _countryService.GetCountriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Countries.Count());
            Assert.Equal("CountryA", result.Countries.First().Name);
            Assert.Equal("CountryC", result.Countries.Last().Name);
        }

        /// <summary>
        ///     Tests that the <see cref="CountryService.FindCountryByIdAsync"/> method correctly returns a
        ///     country when the provided country ID exists in the database.
        ///     This ensures that the service correctly retrieves a country by its ID.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
		[Fact]
        public async Task FindCountryByIdAsync_CountryExists_ReturnsCountry()
        {
            // Arrange
            _context.Countries.Add(new Country { Id = 1, Name = "CountryName" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _countryService.FindCountryByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("CountryName", result.Name);
        }

        /// <summary>
        ///     Tests that the <see cref="CountryService.FindCountryByIdAsync"/> method correctly returns null
        ///     when the provided country ID does not exist in the database.
        ///     This ensures that the service handles missing data gracefully by returning null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
		[Fact]
        public async Task FindCountryByIdAsync_CountryNotFound_ReturnsNull()
        {
            // Arrange
            _context.Countries.Add(new Country { Id = 2, Name = "CountryName" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _countryService.FindCountryByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}
