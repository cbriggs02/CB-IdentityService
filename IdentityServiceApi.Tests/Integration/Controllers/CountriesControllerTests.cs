using IdentityServiceApi.Controllers;
using IdentityServiceApi.Models.ApiResponseModels.Countries;
using IdentityServiceApi.Tests.Integration.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;

namespace IdentityServiceApi.Tests.Integration.Controllers
{
    /// <summary>
    ///     Unit tests for the <see cref="CountriesController"/> class.
    ///     This class contains test cases for various roles controller HTTP/HTTPS scenarios, verifying the 
    ///     behavior of the Country controller functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "IntegrationTest")]
    public class CountriesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CountriesControllerTests"/> class.
        ///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
        ///     to create a test server for the application.
        /// </summary>
        /// <param name="factory">
        ///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
        /// </param>
        public CountriesControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        ///     Verifies that the <see cref="CountriesController.GetCountriesAsync"/> method returns an OK response (200) 
        ///     when data exists.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task GetCountriesAsync_ReturnsOK_WhenDataExist()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(ApiRoutes.CountriesController.RequestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var getCountriesResponse = JsonConvert.DeserializeObject<CountriesListResponse>(responseBody);

            Assert.NotNull(getCountriesResponse);
            Assert.NotEmpty(getCountriesResponse.Countries);
        }
    }      
}
