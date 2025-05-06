using AutoMapper;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.Logging;
using IdentityServiceApi.Models.ServiceResultModels.Shared;
using IdentityServiceApi.Services.Logging;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Logging
{
	/// <summary>
	///     Unit tests for the <see cref="AuditLoggerService"/> class.
	///     This class contains test cases for various audit logging scenarios, verifying the 
	///     behavior of the audit logger functionality.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2024
	/// </remarks>
	[Trait("TestCategory", "UnitTest")]
	public class AuditLoggerServiceTests
	{
		private readonly Mock<ApplicationDbContext> _dbContextMock;
		private readonly Mock<IParameterValidator> _parameterValidatorMock;
		private readonly Mock<IAuditLoggerServiceResultFactory> _serviceResultFactoryMock;
		private readonly Mock<IMapper> _mapperMock;
		private readonly AuditLoggerService _auditLoggerService;
		private const string AuditLogId = "test-id";

		/// <summary>
		///     Initializes a new instance of the <see cref="AuditLoggerServiceTests"/> class,
		///     setting up the necessary mocks and the service under test.
		/// </summary>
		public AuditLoggerServiceTests()
		{
			_dbContextMock = new Mock<ApplicationDbContext>();
			_parameterValidatorMock = new Mock<IParameterValidator>();
			_serviceResultFactoryMock = new Mock<IAuditLoggerServiceResultFactory>();
			_mapperMock = new Mock<IMapper>();
			_auditLoggerService = new AuditLoggerService(_dbContextMock.Object, _parameterValidatorMock.Object, _serviceResultFactoryMock.Object, _mapperMock.Object);
		}

		/// <summary>
		///     Verifies that an <see cref="ArgumentNullException"/> is thrown when the 
		///     <see cref="AuditLoggerService"/> is instantiated with null dependencies.
		/// </summary>
		[Fact]
		public void AuditLoggerService_NullDependencies_ThrowsArgumentNullException()
		{
			//Act & Assert
			Assert.Throws<ArgumentNullException>(() => new AuditLoggerService(null, null, null, null));
		}

		/// <summary>
		///     Verifies that an <see cref="ArgumentNullException"/> is thrown when the 
		///     <see cref="AuditLoggerService"/> is instantiated with null dependencies.
		/// </summary>
		[Fact]
		public async Task GetLogs_NullRequestObject_ThrowsArgumentNullException()
		{
			// Arrange
			_parameterValidatorMock
				.Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
				.Throws<ArgumentNullException>();

			// Act & Assert
			await Assert.ThrowsAsync<ArgumentNullException>(() => _auditLoggerService.GetLogsAsync(null));

			VerifyCallsToParameterServiceForObjectValidation();
		}

		/// <summary>
		///     Verifies that an <see cref="ArgumentNullException"/> is thrown when 
		///     <see cref="AuditLoggerService.GetLogAsync(string)"/> is called with null, empty, or whitespace IDs.
		/// </summary>
		/// <param name="id">
		///     The audit log ID to retrieve, which is expected to be null, empty, or whitespace.
		/// </param>
		/// <returns>
		///     A <see cref="Task"/> representing the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public async Task GetLog_NullAndEmptyId_ThrowsArgumentNullException(string id)
		{
			// Arrange
			_parameterValidatorMock
				.Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
				.Throws<ArgumentNullException>();

			// Act & Assert
			await Assert.ThrowsAsync<ArgumentNullException>(() => _auditLoggerService.GetLogAsync(id));

			VerifyCallsToParameterServiceForStringValidation(1);
		}

		/// <summary>
		///     Verifies that a failure result is returned with a "not found" error message when 
		///     <see cref="AuditLoggerService.GetLogAsync(string)"/> is called with a non-existent ID.
		/// </summary>
		/// <param name="id">
		///     The audit log ID to retrieve, which may be null, empty, whitespace, or an ID not found in the database.
		/// </param>
		/// <returns>
		///     A <see cref="Task"/> representing the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("non-existent-id")]
		public async Task GetLog_NonExistentId_ReturnsNotFoundErrorMessage(string id)
		{
			// Arrange
			const string expectedErrorMessage = Constants.ErrorMessages.AuditLog.NotFound;

			_dbContextMock
				.Setup(f => f.AuditLogs.FindAsync(id))
				.ReturnsAsync((AuditLog)null);

			ArrangeAuditLoggerFailureServiceResult(expectedErrorMessage);

			// Act
			var result = await _auditLoggerService.GetLogAsync(id);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains(expectedErrorMessage, result.Errors);

			VerifyCallsToParameterServiceForStringValidation(1);
			VerifyCallsToDbContextFindAsync(id);
		}

		/// <summary>
		///     Verifies that a successful service result is returned when 
		///     <see cref="AuditLoggerService.GetLogAsync(string)"/> is called with a valid ID that exists in the database.
		/// </summary>
		/// <returns>
		///     A <see cref="Task"/> representing the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetLog_SuccessfulEntityOperation_ReturnsSuccessResult()
		{
			// Arrange
			AuditLog log = CreateAuditLogObject(AuditAction.Exception);
			AuditLogDTO logDTO = CreateAuditLogDtoObject(log);

			_dbContextMock
			  .Setup(f => f.AuditLogs.FindAsync(AuditLogId))
			  .ReturnsAsync(log);

			_mapperMock
				.Setup(m => m.Map<AuditLogDTO>(It.IsAny<AuditLog>()))
				.Returns(logDTO);

			var serviceResult = new AuditLogServiceResult
			{
				Success = true,
				AuditLog = logDTO
			};

			_serviceResultFactoryMock
				.Setup(x => x.AuditLoggerOperationSuccess(logDTO))
				.Returns(serviceResult);

			// Act
			var result = await _auditLoggerService.GetLogAsync(AuditLogId);

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);

			VerifyCallsToParameterServiceForStringValidation(1);
			VerifyCallsToDbContextFindAsync(AuditLogId);
		}

		/// <summary>
		///     Verifies that an <see cref="ArgumentNullException"/> is thrown when 
		///     <see cref="AuditLoggerService.DeleteLogAsync"/> is called with null or empty IDs.
		/// </summary>
		/// <param name="id">
		///     The ID to validate.
		/// </param>
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public async Task DeleteLog_NullAndEmptyId_ThrowsArgumentNullException(string id)
		{
			// Arrange
			_parameterValidatorMock
				.Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
				.Throws<ArgumentNullException>();

			// Act & Assert
			await Assert.ThrowsAsync<ArgumentNullException>(() => _auditLoggerService.DeleteLogAsync(id));

			VerifyCallsToParameterServiceForStringValidation(1);
		}

		/// <summary>
		///     Verifies that a "not found" error is returned when attempting to delete a non-existent log.
		/// </summary>
		/// <param name="id">
		///     The ID of the log to delete.
		/// </param>
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("non-existent-id")]
		public async Task DeleteLog_NonExistentId_ReturnsNotFoundErrorMessage(string id)
		{
			// Arrange
			const string expectedErrorMessage = Constants.ErrorMessages.AuditLog.NotFound;

			_dbContextMock
				.Setup(f => f.AuditLogs.FindAsync(id))
				.ReturnsAsync((AuditLog)null);

			ArrangeFailureServiceResult(expectedErrorMessage);

			// Act
			var result = await _auditLoggerService.DeleteLogAsync(id);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains(expectedErrorMessage, result.Errors);

			VerifyCallsToParameterServiceForStringValidation(1);
			VerifyCallsToDbContextFindAsync(id);
		}

		/// <summary>
		///     Verifies that a "deletion failed" error is returned when the database operation fails.
		/// </summary>
		[Fact]
		public async Task DeleteLog_FailedEntityOperation_ReturnsDeletionFailedResult()
		{
			// Arrange
			const string expectedErrorMessage = Constants.ErrorMessages.AuditLog.DeletionFailed;

			AuditLog log = CreateAuditLogObject(AuditAction.Exception);
			SetupAuditLogEntityRemoveOperationMocks(log, false);
			ArrangeFailureServiceResult(expectedErrorMessage);

			// Act
			var result = await _auditLoggerService.DeleteLogAsync(AuditLogId);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains(expectedErrorMessage, result.Errors);

			VerifyCallsToParameterServiceForStringValidation(1);
			VerifyCallsToDbContextFindAsync(AuditLogId);
			VerifyCallsToDbContextSaveChanges();
		}

		/// <summary>
		///     Verifies that <see cref="AuditLoggerService.DeleteLogAsync"/> returns a success result when the 
		///     entity deletion operation is successful.
		/// </summary>
		[Fact]
		public async Task DeleteLog_SuccessfulEntityOperation_ReturnsSuccessResult()
		{
			// Arrange
			AuditLog log = CreateAuditLogObject(AuditAction.Exception);
			SetupAuditLogEntityRemoveOperationMocks(log, true);

			var serviceResult = new ServiceResult
			{
				Success = true,
				Errors = new List<string>()
			};

			_serviceResultFactoryMock
				.Setup(x => x.GeneralOperationSuccess())
				.Returns(serviceResult);

			// Act
			var result = await _auditLoggerService.DeleteLogAsync(AuditLogId);

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);

			VerifyCallsToParameterServiceForStringValidation(1);
			VerifyCallsToDbContextFindAsync(AuditLogId);
			VerifyCallsToDbContextSaveChanges();
		}

		private static AuditLog CreateAuditLogObject(AuditAction action)
		{
			return new AuditLog
			{
				Id = AuditLogId,
				Action = action,
				UserId = "user-id",
				Details = "details...",
				IpAddress = "127.0.0.22",
				TimeStamp = DateTime.UtcNow,
			};
		}

		private static AuditLogDTO CreateAuditLogDtoObject(AuditLog log)
		{
			return new AuditLogDTO
			{
				Id = log.Id,
				Action = log.Action,
				UserId = log.UserId,
				Details = log.Details,
				IpAddress = log.IpAddress,
				TimeStamp = log.TimeStamp,
			};
		}

		private void SetupAuditLogEntityRemoveOperationMocks(AuditLog log, bool operationStatus)
		{
			_dbContextMock
				.Setup(f => f.AuditLogs.FindAsync(AuditLogId))
				.ReturnsAsync(log);
			_dbContextMock
				.Setup(d => d.AuditLogs
				.Remove(log));
			_dbContextMock
				.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(operationStatus ? 1 : 0);
		}

		private void ArrangeAuditLoggerFailureServiceResult(string expectedErrorMessage)
		{
			var result = new AuditLogServiceResult
			{
				Success = false,
				Errors = new List<string> { expectedErrorMessage }
			};

			_serviceResultFactoryMock
				.Setup(x => x.AuditLoggerOperationFailure(new[] { expectedErrorMessage }))
				.Returns(result);
		}

		private void ArrangeFailureServiceResult(string expectedErrorMessage)
		{
			var result = new ServiceResult
			{
				Success = false,
				Errors = new List<string> { expectedErrorMessage }
			};

			_serviceResultFactoryMock
				.Setup(x => x.GeneralOperationFailure(new[] { expectedErrorMessage }))
				.Returns(result);
		}

		private void VerifyCallsToParameterServiceForStringValidation(int numOfTimes)
		{
			_parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numOfTimes));

		}

		private void VerifyCallsToParameterServiceForObjectValidation()
		{
			_parameterValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
		}

		private void VerifyCallsToDbContextFindAsync(string id)
		{
			_dbContextMock.Verify(f => f.AuditLogs.FindAsync(id), Times.Once);
		}

		private void VerifyCallsToDbContextSaveChanges()
		{
			_dbContextMock.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
