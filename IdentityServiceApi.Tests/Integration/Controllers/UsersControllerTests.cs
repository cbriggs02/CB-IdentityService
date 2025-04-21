using Microsoft.AspNetCore.Mvc.Testing;
using IdentityServiceApi.Controllers;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Tests.Integration.Constants;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Bogus;
using IdentityServiceApi.Data;
using IdentityServiceApi.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ApiResponseModels.Users;

namespace IdentityServiceApi.Tests.Integration.Controllers
{
	/// <summary>
	///     Integration tests for the <see cref="UsersController"/> class.
	///     This class contains test cases for various password controller HTTP/HTTPS scenarios, verifying the 
	///     behavior of the password controller functionality.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2025
	/// </remarks>
	[Trait("TestCategory", "IntegrationTest")]
	public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
	{
		private readonly WebApplicationFactory<Program> _factory;

		/// <summary>
		///     Initializes a new instance of the <see cref="UsersControllerTests"/> class.
		///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
		///     to create a test server for the application.
		/// </summary>
		/// <param name="factory">
		///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
		/// </param>
		public UsersControllerTests(WebApplicationFactory<Program> factory)
		{
			_factory = factory;
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUsersAsync"/> method returns an 
		///     Unauthorized (401) response when the user is not authenticated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetUsersAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/?Page=1&PageSize=5&AccountStatus=0";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUsersAsync"/> method returns a 
		///     Forbidden (403) response when the user has insufficient privileges.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetUsersAsync_ReturnsForbidden_WhenUserHasInsufficientPrivileges()
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client, Roles.User, "user123", "id-123");

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/?Page=1&PageSize=5&AccountStatus=0";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUsersAsync"/> method returns an 
		///     Ok (200) response when the data exists and the user is authenticated with the correct privileges.
		/// </summary>
		/// <param name="roleName">
		///     The role of the user making the request, which can either be 
		///     <see cref="Roles.Admin"/> or <see cref="Roles.SuperAdmin"/>.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.Admin)]
		[InlineData(Roles.SuperAdmin)]
		public async Task GetUsersAsync_ReturnsOk_WhenDataExistAndUserIsAuthenticatedWithCorrectPrivileges(string roleName)
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, roleName);

			AuthenticateClient(client, roleName, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/?Page=1&PageSize=5&AccountStatus=0";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var getUsersResponse = JsonConvert.DeserializeObject<UserListResponse>(responseBody);

			Assert.NotNull(getUsersResponse);
			Assert.NotEmpty(getUsersResponse.Users);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUserAsync"/> method returns an 
		///     Unauthorized (401) response when the user is not authenticated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/user-id";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUserAsync"/> method returns a 
		///     Forbidden (403) response when a user attempts to access another user's data without sufficient permissions.
		/// </summary>
		/// <param name="requesterRole">
		///     The role of the requester attempting to access another user's data.
		/// </param>
		/// <param name="targetRole">
		///     The role of the target user whose data is being accessed.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.User, Roles.User)] // User trying to get User's data
		[InlineData(Roles.User, Roles.Admin)] // User trying to get Admin's data
		[InlineData(Roles.User, Roles.SuperAdmin)] // User trying to get SuperAdmin's data 
		[InlineData(Roles.Admin, Roles.Admin)] // Admin trying to get Admin's data
		[InlineData(Roles.Admin, Roles.SuperAdmin)] // Admin trying to get SuperAdmin's data 
		public async Task GetUserAsync_ReturnsForbidden_WhenUserTriesToGetAnotherUsersDataWithoutPermissions(string requesterRole, string targetRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, requesterRole);
			var target = await CreateTestUserWithoutPasswordAsync(true, targetRole);

			AuthenticateClient(client, requesterRole, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/{target.Id}";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUserAsync"/> method returns a 
		///     Forbidden 403 response when the requested user does not exist.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetUserAsync_ReturnsForbidden_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.User);

			AuthenticateClient(client, Roles.User, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/nonexistent-user-id";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[Fact]
		public async Task GetUserAsync_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/nonexistent-user-id";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUserAsync"/> method returns an 
		///     OK (200) response when a user attempts to get their own data.
		/// </summary>
		/// <param name="roleName">
		///     The role of the user requesting their own data.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.User)]
		[InlineData(Roles.Admin)]
		[InlineData(Roles.SuperAdmin)]
		public async Task GetUserAsync_ReturnsOK_WhenUserIsGettingHisOwnData(string roleName)
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, roleName);

			AuthenticateClient(client, roleName, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/{user.Id}";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var getUserResponse = JsonConvert.DeserializeObject<UserResponse>(responseBody);

			Assert.NotNull(getUserResponse);
			Assert.NotNull(getUserResponse.User);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUserAsync"/> method returns an 
		///     OK (200) response when a user with the correct privileges attempts to access another user's data.
		/// </summary>
		/// <param name="requesterRole">
		///     The role of the requester attempting to access another user's data.
		/// </param>
		/// <param name="targetRole">
		///     The role of the target user whose data is being accessed.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.Admin, Roles.User)] // Admin trying to get User's data
		[InlineData(Roles.SuperAdmin, Roles.Admin)] // Super Admin trying to get Admin's data
		[InlineData(Roles.SuperAdmin, Roles.User)] // Super Admin trying to get User's data 
		public async Task GetUserAsync_ReturnsOK_WhenUserIsGettingOtherUsersDataWithCorrectPrivileges(string requesterRole, string targetRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, requesterRole);
			var target = await CreateTestUserWithoutPasswordAsync(true, targetRole);

			AuthenticateClient(client, requesterRole, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/{target.Id}";

			// Act
			var response = await client.GetAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var getUserResponse = JsonConvert.DeserializeObject<UserResponse>(responseBody);

			Assert.NotNull(getUserResponse);
			Assert.NotNull(getUserResponse.User);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.CreateUserAsync"/> method returns a 
		///     BadRequest (400) response when an invalid or non-existent country ID is provided.
		/// </summary>
		/// <param name="countryId">
		///     The invalid country ID used when attempting to create a user.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(0)]
		[InlineData(-100)]
		[InlineData(1000)]
		public async Task CreateUserAsync_ReturnsBadRequest_WhenProvidedCountryIdDoesNotExist(int countryId)
		{
			// Arrange
			var client = _factory.CreateClient();
			var json = CreateJsonPayLoadForUserOperation(countryId);

			// Act
			var response = await client.PostAsync(ApiRoutes.UsersController.BaseUri, json);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.CreateUserAsync"/> method returns a 
		///     Created (201) response when a valid country ID is provided.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task CreateUserAsync_ReturnsCreated_WhenProvidedCountryIdExists()
		{
			// Arrange
			var client = _factory.CreateClient();
			var json = CreateJsonPayLoadForUserOperation(10);

			// Act
			var response = await client.PostAsync(ApiRoutes.UsersController.BaseUri, json);

			// Assert
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.UpdateUserAsync"/> method returns an 
		///     Unauthorized (401) response when no authentication token is supplied with the request.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task UpdateUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/user-id";
			var json = CreateJsonPayLoadForUserOperation(10);

			// Act
			var response = await client.PutAsync(requestUri, json);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.UpdateUserAsync"/> method returns a 
		///     Forbidden (403) response when a user attempts to update another user's data without 
		///     sufficient permissions.
		/// </summary>
		/// <param name="requesterRole">
		///     The role of the authenticated user initiating the update request.
		/// </param>
		/// <param name="targetRole">
		///     The role of the user whose data is being updated.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.User, Roles.User)] // User trying to update User's data
		[InlineData(Roles.User, Roles.Admin)] // User trying to update Admin's data
		[InlineData(Roles.User, Roles.SuperAdmin)] // User trying to update SuperAdmin's data 
		[InlineData(Roles.Admin, Roles.Admin)] // Admin trying to update Admin's data
		[InlineData(Roles.Admin, Roles.SuperAdmin)] // Admin trying to update SuperAdmin's data 
		public async Task UpdateUserAsync_ReturnsForbidden_WhenUserTriesToUpdateAnotherUsersDataWithoutPermissions(string requesterRole, string targetRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, requesterRole);
			var target = await CreateTestUserWithoutPasswordAsync(true, targetRole);

			AuthenticateClient(client, requesterRole, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/{target.Id}";
			var json = CreateJsonPayLoadForUserOperation(10);

			// Act
			var response = await client.PutAsync(requestUri, json);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.UpdateUserAsync"/> method returns a 
		///     NotFound (404) response when the specified target user does not exist in the system.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task UpdateUserAsync_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/nonexistent-user-id";
			var json = CreateJsonPayLoadForUserOperation(10);

			// Act
			var response = await client.PutAsync(requestUri, json);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.UpdateUserAsync"/> method returns a 
		///     Forbidden (403) response when the target user ID does not exist and the requester 
		///     lacks sufficient privileges.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task UpdateUserAsync_ReturnsForbidden_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.User);

			AuthenticateClient(client, Roles.User, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/nonexistent-user-id";
			var json = CreateJsonPayLoadForUserOperation(10);

			// Act
			var response = await client.PutAsync(requestUri, json);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.UpdateUserAsync"/> method returns a 
		///     BadRequest (400) response when an invalid or non-existent country ID is used during user update.
		/// </summary>
		/// <param name="countryId">
		///     The invalid country ID used when attempting to update the user's country information.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(0)]
		[InlineData(-100)]
		[InlineData(1000)]
		public async Task UpdateUserAsync_ReturnsBadRequest_WhenProvidedCountryDoesNotExist(int countryId)
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.User);

			AuthenticateClient(client, Roles.User, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/{user.Id}";
			var json = CreateJsonPayLoadForUserOperation(countryId);

			// Act
			var response = await client.PutAsync(requestUri, json);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.UpdateUserAsync"/> method returns a 
		///     NoContent (204) response when the user is successfully updated with valid data and credentials.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task UpdateUserAsync_ReturnsNoContent_WhenUserIsAuthenticatedAndUserIsSuccessfullyUpdated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.User);

			AuthenticateClient(client, Roles.User, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/{user.Id}";
			var json = CreateJsonPayLoadForUserOperation(10);

			// Act
			var response = await client.PutAsync(requestUri, json);

			// Assert
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeleteUserAsync"/> method returns an 
		///     Unauthorized (401) response when the request is made without authentication.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeleteUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/user-id";

			// Act
			var response = await client.DeleteAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeleteUserAsync"/> method returns a 
		///     Forbidden (403) response when a user attempts to delete another user's account without sufficient permissions.
		/// </summary>
		/// <param name="requesterRole">
		///     The role of the user attempting to delete another user's account.
		/// </param>
		/// <param name="targetRole">
		///     The role of the target user whose account is being deleted.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.User, Roles.User)] // User trying to delete User's data
		[InlineData(Roles.User, Roles.Admin)] // User trying to delete Admin's data
		[InlineData(Roles.User, Roles.SuperAdmin)] // User trying to delete SuperAdmin's data 
		[InlineData(Roles.Admin, Roles.Admin)] // Admin trying to delete Admin's data
		[InlineData(Roles.Admin, Roles.SuperAdmin)] // Admin trying to delete SuperAdmin's data 
		public async Task DeleteUserAsync_ReturnsForbidden_WhenUserTriesToDeleteAnotherUsersWithoutPermissions(string requesterRole, string targetRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, requesterRole);
			var target = await CreateTestUserWithoutPasswordAsync(true, targetRole);

			AuthenticateClient(client, requesterRole, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/{target.Id}";

			// Act
			var response = await client.DeleteAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeleteUserAsync"/> method returns a 
		///     NotFound (404) response when the target user to be deleted does not exist in the system.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeleteUserAsync_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/nonexistent-user-id";

			// Act
			var response = await client.DeleteAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeleteUserAsync"/> method returns a 
		///     NoContent (204) response when a user with sufficient privileges successfully deletes their own account.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeleteUserAsync_ReturnsNoContent_WhenUserIsAuthenticatedAndSuccessfullyDeleted()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/{user.Id}";

			// Act
			var response = await client.DeleteAsync(requestUri);

			// Assert
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.ActivateUserAsync"/> method returns 
		///     an Unauthorized (401) response when the user is not authenticated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task ActivateUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/activate/user-id";

			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.ActivateUserAsync"/> method returns 
		///     a Forbidden (403) response when a user without sufficient permissions attempts to activate another user.
		/// </summary>
		/// <param name="requesterRole">The role of the user making the request.</param>
		/// <param name="targetRole">The role of the user being activated.</param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.Admin, Roles.Admin)] // Admin trying to activate another Admin
		[InlineData(Roles.Admin, Roles.SuperAdmin)] // Admin trying to activate a super admin
		public async Task ActivateUserAsync_ReturnsForbidden_WhenUserTriesToActivateAnotherUserWithOutPermission(string requesterRole, string targetRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, requesterRole);
			var target = await CreateTestUserWithoutPasswordAsync(false, targetRole);

			AuthenticateClient(client, requesterRole, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/activate/{target.Id}";
			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.ActivateUserAsync"/> method returns 
		///     a NotFound (404) response when the target user does not exist.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task ActivateUserAsync_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(false, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/activate/nonexistent-user-id";
			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.ActivateUserAsync"/> method returns 
		///     a BadRequest (400) response when the target user is already activated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task ActivateUserAsync_ReturnsBadRequest_WhenUserIsAlreadyActivated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.Admin);
			var target = await CreateTestUserWithoutPasswordAsync(true, Roles.User);

			AuthenticateClient(client, Roles.Admin, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/activate/{target.Id}";
			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.ActivateUserAsync"/> method returns 
		///     a NoContent (204) response when the user is successfully activated and was not previously activated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task ActivateUserAsync_ReturnsNoContent_WhenUserIsNotAlreadyActivated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.Admin);
			var target = await CreateTestUserWithoutPasswordAsync(false, Roles.User);

			AuthenticateClient(client, Roles.Admin, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/activate/{target.Id}";
			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeactivateUserAsync"/> method returns an 
		///     Unauthorized (401) response when the request is made without user authentication.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeactivateUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/deactivate/user-id";

			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeactivateUserAsync"/> method returns a 
		///     Forbidden (403) response when a user without the required permissions attempts to 
		///     deactivate another user with a protected role.
		/// </summary>
		/// <param name="requesterRole">
		///     The role of the user attempting the deactivation (e.g., Admin).
		/// </param>
		/// <param name="targetRole">
		///     The role of the user being deactivated (e.g., Admin or SuperAdmin).
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.Admin, Roles.Admin)] // Admin trying to deactivate another Admin
		[InlineData(Roles.Admin, Roles.SuperAdmin)] // Admin trying to deactivate a super admin
		public async Task DeactivateUserAsync_ReturnsForbidden_WhenUserTriesToDeactivateAnotherUserWithOutPermission(string requesterRole, string targetRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, requesterRole);
			var target = await CreateTestUserWithoutPasswordAsync(true, targetRole);

			AuthenticateClient(client, requesterRole, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/deactivate/{target.Id}";
			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeactivateUserAsync"/> method returns a 
		///     NotFound (404) response when the target user to deactivate does not exist.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeactivateUserAsync_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/deactivate/nonexistent-user-id";
			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeactivateUserAsync"/> method returns a 
		///     BadRequest (400) response when the target user is not already activated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeactivateUserAsync_ReturnsBadRequest_WhenUserIsNotAlreadyActivated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.Admin);
			var target = await CreateTestUserWithoutPasswordAsync(false, Roles.User);

			AuthenticateClient(client, Roles.Admin, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/deactivate/{target.Id}";
			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.DeactivateUserAsync"/> method returns a 
		///     NoContent (204) response when a valid request is made to deactivate an already activated user.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task DeactivateUserAsync_ReturnsNoContent_WhenUserIsAlreadyActivated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.Admin);
			var target = await CreateTestUserWithoutPasswordAsync(true, Roles.User);

			AuthenticateClient(client, Roles.Admin, requester.UserName, requester.Id);

			string requestUri = $"{ApiRoutes.UsersController.BaseUri}/deactivate/{target.Id}";
			var content = new StringContent("{}", Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync(requestUri, content);

			// Assert
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.AssignRoleAsync(string, string)"/> method returns an 
		///     Unauthorized (401) response when the request is made without authentication.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task AssignRoleAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var jsonBody = CreateJsonPayloadForRoleOperations(Roles.User);
			string RequestUri = ApiRoutes.UsersController.BaseUri + "/id/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.AssignRoleAsync(string, string)"/> method returns a 
		///     Forbidden (403) response when the authenticated user does not have sufficient privileges to assign a role.
		/// </summary>
		/// <param name="userRole">
		///     The role of the authenticated user making the request (e.g., Admin, User).
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.Admin)]
		[InlineData(Roles.User)]
		public async Task AssignRoleAsync_ReturnsForbidden_WhenUserHasInsufficientPrivileges(string userRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client, userRole, "user123", "id-123");

			var jsonBody = CreateJsonPayloadForRoleOperations(Roles.User);
			string RequestUri = ApiRoutes.UsersController.BaseUri + "/id-123/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.AssignRoleAsync(string, string)"/> method returns a 
		///     NotFound (404) response when the target user does not exist in the system.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task AssignRoleAsync_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

			var jsonBody = CreateJsonPayloadForRoleOperations(Roles.User);
			string RequestUri = ApiRoutes.UsersController.BaseUri + "/id-123/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.AssignRoleAsync(string, string)"/> method returns a 
		///     BadRequest (400) response when the target user exists but is not activated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task AssignRoleAsync_ReturnsBadRequest_WhenUserIsNotActivated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

			var target = await CreateTestUserWithoutPasswordAsync(false);

			var jsonBody = CreateJsonPayloadForRoleOperations(Roles.User);
			string RequestUri = ApiRoutes.UsersController.BaseUri + $"/{target.Id}/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.AssignRoleAsync(string, string)"/> method returns a 
		///     BadRequest (400) response when the specified role does not exist in the system.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task AssignRoleAsync_ReturnsBadRequest_WhenRequestedRoleDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

			var target = await CreateTestUserWithoutPasswordAsync(true);

			var jsonBody = CreateJsonPayloadForRoleOperations("nonexistent-role");
			string RequestUri = ApiRoutes.UsersController.BaseUri + $"/{target.Id}/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.AssignRoleAsync(string, string)"/> method returns a 
		///     BadRequest (400) response when the target user already has the role being assigned.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task AssignRoleAsync_ReturnsBadRequest_WhenTargetUserAlreadyHasRole()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

			var target = await CreateTestUserWithoutPasswordAsync(true, Roles.User);

			var jsonBody = CreateJsonPayloadForRoleOperations(Roles.User);
			string RequestUri = ApiRoutes.UsersController.BaseUri + $"/{target.Id}/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.AssignRoleAsync(string, string)"/> method returns 
		///     an NoContent (204) response when the role is successfully assigned to a user who does not already have it.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task AssignRoleAsync_ReturnsNoContent_WhenTargetUserDoesNotHaveRoleAlready()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

			var target = await CreateTestUserWithoutPasswordAsync(true);

			var jsonBody = CreateJsonPayloadForRoleOperations(Roles.User);
			string RequestUri = ApiRoutes.UsersController.BaseUri + $"/{target.Id}/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.RemoveRoleAsync(string, string)"/> method 
		///     returns an Unauthorized (401) response when a request is made by an unauthenticated user.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task RemoveRoleAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			string RequestUri = ApiRoutes.UsersController.BaseUri + "/id-123/roles/role-name";

			// Act
			var response = await client.DeleteAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.RemoveRoleAsync(string, string)"/>
		///     method returns a Forbidden (403) response when the authenticated user has insufficient privileges
		///     to remove a role.
		/// </summary>
		/// <param name="userRole">
		///     The role used to authenticate the client, which should not grant sufficient privileges
		///     for removing the role from the user.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.Admin)]
		[InlineData(Roles.User)]
		public async Task RemoveRoleAsync_ReturnsForbidden_WhenUserHasInsufficientPrivileges(string userRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client, userRole, "user123", "id-123");

			string RequestUri = ApiRoutes.UsersController.BaseUri + "/id-123/roles/role-name";

			// Act
			var response = await client.DeleteAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.RemoveRoleAsync(string, string)"/>
		///     method returns a NotFound (404) response when the specified user does not exist.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task RemoveRoleAsync_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

			string RequestUri = ApiRoutes.UsersController.BaseUri + "/id-123/roles/role-name";

			// Act
			var response = await client.DeleteAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			await CleanUpTestUserAsync(user.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.RemoveRoleAsync(string, string)"/> method 
		///     returns a BadRequest (400) response when the requested role does not exist in the system.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task RemoveRoleAsync_ReturnsBadRequest_WhenRequestedRoleDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

			var target = await CreateTestUserWithoutPasswordAsync(true);
			string RequestUri = ApiRoutes.UsersController.BaseUri + $"/{target.Id}/roles/nonexistent-role";

			// Act
			var response = await client.DeleteAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.RemoveRoleAsync(string, string)"/> method 
		///     returns a BadRequest (400) response when the target user does not have the specified role assigned.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task RemoveRoleAsync_ReturnsBadRequest_WhenTargetUserDoesNotHaveRole()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

			var target = await CreateTestUserWithoutPasswordAsync(true);
			string RequestUri = ApiRoutes.UsersController.BaseUri + $"/{target.Id}/roles/{Roles.User}";

			// Act
			var response = await client.DeleteAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

			Assert.NotNull(errorResponse);
			Assert.NotEmpty(errorResponse.Errors);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.RemoveRoleAsync(string, string)"/> method 
		///     returns a NoContent (204) response when the specified role is successfully removed from the target user.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task RemoveRoleAsync_ReturnsNoContent_WhenRoleIsSuccessfullyRemoved()
		{
			// Arrange
			var client = _factory.CreateClient();
			var requester = await CreateTestUserWithoutPasswordAsync(true, Roles.SuperAdmin);

			AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

			var target = await CreateTestUserWithoutPasswordAsync(true, Roles.User);
			string RequestUri = ApiRoutes.UsersController.BaseUri + $"/{target.Id}/roles/{Roles.User}";

			// Act
			var response = await client.DeleteAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

			await CleanUpTestUserAsync(requester.Email);
			await CleanUpTestUserAsync(target.Email);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUserStateMetricsAsync"/> method
		///     returns an Unauthorized (401) response when the request is made without authentication.
		/// </summary>
		/// <returns>
		///     A task representing the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetUserStateMetricsAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			string RequestUri = ApiRoutes.UsersController.BaseUri + "/state-metrics";

			// Act
			var response = await client.GetAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUserStateMetricsAsync"/> method
		///     returns a Forbidden (403) response when the authenticated user lacks the necessary privileges.
		/// </summary>
		/// <returns>
		///     A task representing the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetUserStateMetricsAsync_ReturnsForbidden_WhenUserHasInsufficientPrivileges()
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client, Roles.User, "user123", "id-123");

			string RequestUri = ApiRoutes.UsersController.BaseUri + "/state-metrics";

			// Act
			var response = await client.GetAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="UsersController.GetUserStateMetricsAsync"/> method
		///     returns an OK (200) response and a valid <see cref="UserStateMetricsResponse"/> object
		///     when the request is made by an authenticated user with appropriate privileges.
		/// </summary>
		/// <returns>
		///     A task representing the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetUserStateMetricsAsync_ReturnsOK_WhenIsAuthenticatedWithCorrectPrivileges()
		{
			// Arrange
			var client = _factory.CreateClient();
			var user = await CreateTestUserWithoutPasswordAsync(true, Roles.Admin);

			AuthenticateClient(client, Roles.Admin, user.UserName, user.Id);

			string RequestUri = ApiRoutes.UsersController.BaseUri + "/state-metrics";

			// Act
			var response = await client.GetAsync(RequestUri);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var metricsResponse = JsonConvert.DeserializeObject<UserStateMetricsResponse>(responseBody);

			Assert.NotNull(metricsResponse);

			await CleanUpTestUserAsync(user.Email);
		}

		private static StringContent CreateJsonPayLoadForUserOperation(int countryId)
		{
			var faker = new Faker();
			string userName = faker.Internet.UserName();
			string email = faker.Internet.Email();
			string firstName = faker.Name.FirstName();
			string lastName = faker.Name.LastName();
			string phoneNumber = faker.Phone.PhoneNumber("###-###-####");

			var newUserRequest = new UserDTO
			{
				UserName = userName,
				FirstName = firstName,
				LastName = lastName,
				Email = email,
				PhoneNumber = phoneNumber,
				CountryId = countryId,
			};

			var jsonContent = new StringContent(
				JsonConvert.SerializeObject(newUserRequest),
				Encoding.UTF8,
				"application/json"
			);
			return jsonContent;
		}

		private static StringContent CreateJsonPayloadForRoleOperations(string roleName)
		{
			return new StringContent(
							JsonConvert.SerializeObject(roleName),
							Encoding.UTF8,
							"application/json"
						);
		}

		private static void AuthenticateClient(HttpClient client, string role, string userName, string userId)
		{
			var token = JwtTokenTestHelper.GenerateJwtToken(roles: new List<string> { role }, userName, userId);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		private async Task<User> CreateTestUserWithoutPasswordAsync(bool status, string role = null)
		{
			var createTestUserHelper = CreateTestUserHelper();

			var faker = new Faker();
			string userName = faker.Internet.UserName();
			string email = faker.Internet.Email();
			string firstName = faker.Name.FirstName();
			string lastName = faker.Name.LastName();
			string phoneNumber = faker.Phone.PhoneNumber();

			return await createTestUserHelper.CreateTestUserWithoutPasswordAsync(userName, firstName, lastName, email, phoneNumber, status, role);
		}

		private async Task CleanUpTestUserAsync(string email)
		{
			var createTestUserHelper = CreateTestUserHelper();
			await createTestUserHelper.DeleteTestUserAsync(email);
		}

		private CreateTestUserHelper CreateTestUserHelper()
		{
			var scope = _factory.Services.CreateScope();
			var serviceProvider = scope.ServiceProvider;
			var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
			var applicationDbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
			var createTestUserHelper = new CreateTestUserHelper(userManager, applicationDbContext);
			return createTestUserHelper;
		}
	}
}
