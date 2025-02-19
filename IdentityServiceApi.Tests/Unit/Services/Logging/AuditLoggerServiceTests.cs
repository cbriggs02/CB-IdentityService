using AutoMapper;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
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
        private readonly Mock<IServiceResultFactory> _serviceResultFactoryMock;
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
            _serviceResultFactoryMock = new Mock<IServiceResultFactory>();
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _auditLoggerService.GetLogs(null));

            VerifyCallsToParameterServiceForObjectValidation();
        }

        /// <summary>
        ///     Verifies that an <see cref="ArgumentNullException"/> is thrown when 
        ///     <see cref="AuditLoggerService.DeleteLog"/> is called with null or empty IDs.
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _auditLoggerService.DeleteLog(id));

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
            var result = await _auditLoggerService.DeleteLog(id);

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
            var result = await _auditLoggerService.DeleteLog(AuditLogId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToParameterServiceForStringValidation(1);
            VerifyCallsToDbContextFindAsync(AuditLogId);
            VerifyCallsToDbContextSaveChanges();
        }

        /// <summary>
        ///     Verifies that <see cref="AuditLoggerService.DeleteLog"/> returns a success result when the 
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
            var result = await _auditLoggerService.DeleteLog(AuditLogId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToParameterServiceForStringValidation(1);
            VerifyCallsToDbContextFindAsync(AuditLogId);
            VerifyCallsToDbContextSaveChanges();
        }

        /// <summary>
        ///     Creates an instance of <see cref="AuditLog"/> with predefined properties.
        /// </summary>
        /// <param name="action">
        ///     The action performed, represented by an <see cref="AuditAction"/> enumeration value.
        /// </param>
        /// <returns>
        ///     A new <see cref="AuditLog"/> object with the specified action and other preset properties.
        /// </returns>
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

        /// <summary>
        ///     Sets up mock behavior for database operations related to removing an <see cref="AuditLog"/> entity.
        /// </summary>
        /// <param name="log">
        ///     The <see cref="AuditLog"/> instance to be removed.
        /// </param>
        /// <param name="operationStatus">
        ///     A boolean indicating whether the operation should succeed (true) or fail (false).
        /// </param>
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

        /// <summary>
        ///     Sets up the mock service factory result to return a 
        ///     <see cref="ServiceResult"/> indicating a failure with the specified error message.
        /// </summary>
        /// <param name="expectedErrorMessage">
        ///     The expected error message to be included in the 
        ///     <see cref="ServiceResult"/> indicating the reason for failure.
        /// </param>
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

        /// <summary>
        ///     Verifies that the <see cref="IParameterValidator.ValidateNotNullOrEmpty"/> 
        ///     method is called a specified number of times during the test execution.
        /// </summary>
        /// <param name="numOfTimes">
        ///     The number of times the validation is expected to be called.
        /// </param>
        private void VerifyCallsToParameterServiceForStringValidation(int numOfTimes)
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numOfTimes));

        }

        /// <summary>
        ///     Verifies that the <see cref="IParameterValidator.ValidateObjectNotNull"/> 
        ///     method is called exactly once during the test execution.
        /// </summary>
        private void VerifyCallsToParameterServiceForObjectValidation()
        {
            _parameterValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="ApplicationDbContext.AuditLogs.FindAsync"/> 
        ///     method is called exactly once with the specified ID during the test execution.
        /// </summary>
        /// <param name="id">
        ///     The ID of the audit log expected to be passed to the method.
        /// </param>
        private void VerifyCallsToDbContextFindAsync(string id)
        {
            _dbContextMock.Verify(f => f.AuditLogs.FindAsync(id), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="ApplicationDbContext.SaveChangesAsync"/> 
        ///     method is called exactly once during the test execution.
        /// </summary>
        private void VerifyCallsToDbContextSaveChanges()
        {
            _dbContextMock.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
