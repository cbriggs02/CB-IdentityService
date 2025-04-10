using IdentityServiceApi.Controllers;
using IdentityServiceApi.Models.ApiResponseModels.AuditLogsResponses;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using IdentityServiceApi.Tests.Integration.Helpers;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Tests.Integration.Constants;
using Newtonsoft.Json;
using IdentityServiceApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Bogus;
using IdentityServiceApi.Models.Entities;

namespace IdentityServiceApi.Tests.Integration.Controllers
{
	/// <summary>
	///     Unit tests for the <see cref="AuditLogsController"/> class.
	///     This class contains test cases for various audit logs controller HTTP/HTTPS scenarios, verifying the 
	///     behavior of the audit logs controller functionality.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2025
	/// </remarks>
	[Trait("TestCategory", "IntegrationTest")]
	public class AuditLogsControllerTests : IClassFixture<WebApplicationFactory<Program>>
	{
		private readonly WebApplicationFactory<Program> _factory;

		/// <summary>
		///     Initializes a new instance of the <see cref="AuditLogsControllerTests"/> class.
		///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
		///     to create a test server for the application.
		/// </summary>
		/// <param name="factory">
		///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
		/// </param>
		public AuditLogsControllerTests(WebApplicationFactory<Program> factory)
		{
			_factory = factory;
		}

		/// <summary>
		///     Verifies that the <see cref="AuditLogsController.GetLogsAsync"/> method returns an OK response (200) 
		///     when the user is authorized and audit logs data exists.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetLogsAsync_ReturnsOk_WhenAuthorizedAndDataExists()
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client);
			var requestUri = $"{ApiRoutes.AuditLogsController.BaseUri}?Page=1&PageSize=5&Action=0";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var getLogsResponse = JsonConvert.DeserializeObject<AuditLogListResponse>(responseBody);

			Assert.NotNull(getLogsResponse);
			Assert.NotEmpty(getLogsResponse.Logs);
		}

		/// <summary>
		///     Verifies that the <see cref="AuditLogsController.GetLogsAsync"/> method returns an Unauthorized 
		///     response (401) when the user is not authenticated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetLogsAsync_ReturnsUnauthorized_WhenNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requestUri = $"{ApiRoutes.AuditLogsController.BaseUri}?PageNumber=1&PageSize=5&AuditLogs?Page=1&PageSize=5&Action=0";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="AuditLogsController.DeleteLogAsync"/> method returns a 
		///     NotFound response (404) when trying to delete a log that does not exist.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeleteLogAsync_ReturnsNotFound_WhenLogDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();

			AuthenticateClient(client);
			var requestUri = $"{ApiRoutes.AuditLogsController.BaseUri}/nonexistent-log-id";

			// Act
			var response = await client.DeleteAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="AuditLogsController.DeleteLogAsync"/> method returns an Unauthorized 
		///     response (401) when the user is not authenticated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeleteLogAsync_ReturnsUnauthorized_WhenNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requestUri = $"{ApiRoutes.AuditLogsController.BaseUri}/valid-log-id";

			// Act
			var response = await client.DeleteAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="AuditLogsController.DeleteLogAsync"/> method returns a NoContent 
		///     response (204) when the user is authenticated and the specified audit log exists and is deleted.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeleteLogAsync_ReturnsNoContent_WhenUserIsAuthenticatedAndLogWasDeleted()
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client);

			var user = await CreateTestUserWithPasswordAsync(true);
			var auditLog = await CreateTestAuditLog(user.Id);

			string requestUri = $"{ApiRoutes.AuditLogsController.BaseUri}/{auditLog.Id}";

			// Act
			var response = await client.DeleteAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		private static void AuthenticateClient(HttpClient client)
		{
			var token = JwtTokenTestHelper.GenerateJwtToken(roles: new List<string> { Roles.SuperAdmin }, "user123", "id-123");
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		private async Task<AuditLog> CreateTestAuditLog(string userId)
		{
			var serviceProvider = GetServiceProvider();
			var applicationDbContext = GetApplicationDbContext(serviceProvider);

			var faker = CreateFakerInstance();
			string ipAddress = faker.Internet.Ipv6();

			var log = new AuditLog
			{
				Action = AuditAction.SlowPerformance,
				UserId = userId,
				Details = "slow performance log test",
				IpAddress = ipAddress,
				TimeStamp = DateTime.UtcNow
			};

			applicationDbContext.AuditLogs.Add(log);
			var result = await applicationDbContext.SaveChangesAsync();

			if (result != 1)
			{
				throw new InvalidOperationException("Failed to create audit log");
			}

			return log;
		}

		private async Task<User> CreateTestUserWithPasswordAsync(bool status)
		{
			var createTestUserHelper = CreateTestUserHelper();

			var faker = CreateFakerInstance();
			string userName = faker.Internet.UserName();
			string email = faker.Internet.Email();
			string firstName = faker.Name.FirstName();
			string lastName = faker.Name.LastName();
			string phoneNumber = faker.Phone.PhoneNumber();

			return await createTestUserHelper.CreateTestUserWithPasswordAsync(userName, firstName, lastName, email, phoneNumber, "Test@1234", status);
		}

		private async Task CleanUpTestUserAsync(string email)
		{
			var createTestUserHelper = CreateTestUserHelper();
			await createTestUserHelper.DeleteTestUserAsync(email);
		}

		private CreateTestUserHelper CreateTestUserHelper()
		{
			var serviceProvider = GetServiceProvider();
			var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
			var applicationDbContext = GetApplicationDbContext(serviceProvider);
			var createTestUserHelper = new CreateTestUserHelper(userManager, applicationDbContext);
			return createTestUserHelper;
		}

		private IServiceProvider GetServiceProvider()
		{
			var scope = _factory.Services.CreateScope();
			var serviceProvider = scope.ServiceProvider;
			return serviceProvider;
		}

		private static ApplicationDbContext GetApplicationDbContext(IServiceProvider serviceProvider)
		{
			return serviceProvider.GetRequiredService<ApplicationDbContext>();
		}

		private static Faker CreateFakerInstance()
		{
			return new Faker();
		}
	}
}
